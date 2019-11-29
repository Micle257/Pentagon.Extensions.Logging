// -----------------------------------------------------------------------
//  <copyright file="RollingFileSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Core;
    using Debugging;
    using Events;
    using Formatting;

    sealed class RollingFileSink : ILogEventSink, IFlushableFileSink, IDisposable
    {
        readonly PathRoller _roller;
        readonly ITextFormatter _textFormatter;
        readonly long? _fileSizeLimitBytes;
        readonly int? _retainedFileCountLimit;
        readonly Encoding _encoding;
        readonly bool _buffered;
        readonly bool _rollOnFileSizeLimit;
        readonly FileLifecycleHooks _hooks;

        readonly object _syncRoot = new object();
        bool _isDisposed;
        DateTime? _nextCheckpoint;
        IFileSink _currentFile;
        int? _currentFileSequence;
        string _currentFilePath;
        string _lastFilePath;

        public RollingFileSink(PathRoller roller,
                               ITextFormatter textFormatter,
                               long? fileSizeLimitBytes,
                               int? retainedFileCountLimit,
                               Encoding encoding,
                               bool buffered,
                               bool rollOnFileSizeLimit,
                               FileLifecycleHooks hooks)
        {
            if (roller == null)
                throw new ArgumentNullException(nameof(roller));
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
                throw new ArgumentException(message: "Negative value provided; file size limit must be non-negative.");
            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
                throw new ArgumentException(message: "Zero or negative value provided; retained file count limit must be at least 1.");

            _roller                 = roller;
            _textFormatter          = textFormatter;
            _fileSizeLimitBytes     = fileSizeLimitBytes;
            _retainedFileCountLimit = retainedFileCountLimit;
            _encoding               = encoding;
            _buffered               = buffered;
            _rollOnFileSizeLimit    = rollOnFileSizeLimit;
            _hooks                  = hooks;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));

            lock (_syncRoot)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(objectName: "The log file has been disposed.");

                var now = Clock.DateTimeNow;
                AlignCurrentFileTo(now: now);

                while (_currentFile?.EmitOrOverflow(logEvent: logEvent) == false && _rollOnFileSizeLimit)
                    AlignCurrentFileTo(now: now, true);
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_currentFile == null)
                    return;
                CloseFile();
                _isDisposed = true;
            }
        }

        public void FlushToDisk()
        {
            lock (_syncRoot)
            {
                _currentFile?.FlushToDisk();
            }
        }

        public void Compress(FileInfo fi)
        {
            // Get the stream of the source file.
            using var inFile = fi.OpenRead();

            // Prevent compressing hidden and 
            // already compressed files.
            if (((File.GetAttributes(path: fi.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden) & (fi.Extension != ".gz"))
            {
                // Create the compressed file.
                using var outFile =
                        File.Create(fi.FullName + ".gz");

                using var Compress =
                        new GZipStream(stream: outFile,
                                       compressionLevel: CompressionLevel.Fastest);

                // Copy the source file into 
                // the compression stream.
                inFile.CopyTo(destination: Compress);
            }
        }

        void AlignCurrentFileTo(DateTime now, bool nextSequence = false)
        {
            if (!_nextCheckpoint.HasValue)
                OpenFile(now: now);
            else if (nextSequence || now >= _nextCheckpoint.Value)
            {
                int? minSequence = null;
                if (nextSequence)
                {
                    if (_currentFileSequence == null)
                        minSequence = 1;
                    else
                        minSequence = _currentFileSequence.Value + 1;
                }

                CloseFile();

                OpenFile(now: now, minSequence: minSequence);

                GzipFile();

                ZipAllDay();

                RemoveLastFile();
            }
        }

        void ZipAllDay()
        {
            if (_lastFilePath == null)
                return;

            var logFile = new FileInfo(fileName: _lastFilePath);

            var fileToZip = new FileInfo($"{_lastFilePath}.gz");

            if (!fileToZip.Exists)
                return;

            var rollingLogFile = _roller.SelectMatches(new[] {logFile}).FirstOrDefault();

            var baseFileName = rollingLogFile?.PathWithoutSequence ?? Path.Combine(path1: logFile.DirectoryName, path2: "compress");

            try
            {
                using var zipArchive = ZipFile.Open($"{baseFileName}.zip", mode: ZipArchiveMode.Update);

                zipArchive.CreateEntryFromFile(sourceFileName: fileToZip.FullName, entryName: fileToZip.Name, compressionLevel: CompressionLevel.Fastest);
            }
            finally
            {
                fileToZip.Delete();
            }
        }

        void RemoveLastFile()
        {
            if (_lastFilePath == null)
                return;

            File.Delete(path: _lastFilePath);
        }

        void GzipFile()
        {
            if (_lastFilePath == null)
                return;

            Compress(new FileInfo(fileName: _lastFilePath));
        }

        void OpenFile(DateTime now, int? minSequence = null)
        {
            var currentCheckpoint = _roller.GetCurrentCheckpoint(instant: now);

            // We only try periodically because repeated failures
            // to open log files REALLY slow an app down.
            _nextCheckpoint = _roller.GetNextCheckpoint(instant: now) ?? now.AddMinutes(30);

            var existingFiles = Enumerable.Empty<FileInfo>();
            try
            {
                existingFiles = new DirectoryInfo(path: _roller.LogFileDirectory)
                       .GetFiles(searchPattern: _roller.DirectorySearchPattern, _roller.SupportsSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (DirectoryNotFoundException) { }

            var latestForThisCheckpoint = _roller
                                         .SelectMatches(files: existingFiles)
                                         .Where(m => m.DateTime == currentCheckpoint)
                                         .OrderByDescending(m => m.SequenceNumber)
                                         .FirstOrDefault();

            var sequence = latestForThisCheckpoint?.SequenceNumber;
            if (minSequence != null)
            {
                if (sequence == null || sequence.Value < minSequence.Value)
                    sequence = minSequence;
            }

            const int maxAttempts = 3;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                _roller.GetLogFilePath(date: now, sequenceNumber: sequence, out var path);

                try
                {
                    _lastFilePath = _currentFilePath;

                    _currentFile = new FileSink(path: path, textFormatter: _textFormatter, fileSizeLimitBytes: _fileSizeLimitBytes, encoding: _encoding, buffered: _buffered, hooks: _hooks);

                    _currentFileSequence = sequence;

                    _currentFilePath = path;
                }
                catch (IOException ex)
                {
                    if (IOErrors.IsLockedFile(ex: ex))
                    {
                        SelfLog.WriteLine(format: "File target {0} was locked, attempting to open next in sequence (attempt {1})", arg0: path, attempt + 1);
                        sequence = (sequence ?? 0) + 1;
                        continue;
                    }

                    throw;
                }

                ApplyRetentionPolicy(currentFilePath: path);
                return;
            }
        }

        void ApplyRetentionPolicy(string currentFilePath)
        {
            if (_retainedFileCountLimit == null)
                return;

            var currentFile = new FileInfo(fileName: currentFilePath);

            // We consider the current file to exist, even if nothing's been written yet,
            // because files are only opened on response to an event being processed.

            // 1. Get files in the directory (and subdirectories) that match the current DirectorySearchPattern (which would select a superset of actual log files), also add `currentFilePath` too:
            // e.g. "\logs\log-20190222.log" and "\logs\log-2019-not-a-logfile-0222.log"
            var potentialMatches = new DirectoryInfo(path: _roller.LogFileDirectory)
                                  .GetFiles(searchPattern: _roller.DirectorySearchPattern, _roller.SupportsSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                  .Union(new[] {currentFile}, comparer: FileInfoComparer.Instance);

            // 2. For each matched file, filter out to files that exactly match the current IRollingFilePathProvider's format, then put in descending chronological order.
            // e.g. "\logs\log-20190222.log"
            var newestFirst = _roller
                             .SelectMatches(files: potentialMatches)
                             .OrderByDescending(m => m.DateTime)
                             .ThenByDescending(m => m.SequenceNumber)
                             .Select(m => m.File);

            // 3. Delete all files after the retained file limit, *excluding* the file for `currentFile`.
            IEnumerable<FileInfo> toRemove = newestFirst
                                            .Where(n => !FileInfoComparer.Instance.Equals(x: currentFile, y: n))
                                            .Skip(_retainedFileCountLimit.Value - 1)
                                            .ToList();

            foreach (var obsoleteLogFile in toRemove)
            {
                try
                {
                    obsoleteLogFile.Delete();
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine(format: "Error {0} while removing obsolete log file {1}", arg0: ex, arg1: obsoleteLogFile.FullName);
                }
            }
        }

        void CloseFile()
        {
            if (_currentFile != null)
            {
                (_currentFile as IDisposable)?.Dispose();
                _currentFile = null;
            }

            _nextCheckpoint = null;
        }
    }
}
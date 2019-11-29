// -----------------------------------------------------------------------
//  <copyright file="PathRoller.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    class PathRoller
    {
        readonly IRollingFilePathProvider pathProvider;

        PathRoller(string logDirectoryAbsolutePath, IRollingFilePathProvider pathProvider)
        {
            if (logDirectoryAbsolutePath == null)
                throw new ArgumentNullException(nameof(logDirectoryAbsolutePath));

            this.pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));

            LogFileDirectory = logDirectoryAbsolutePath;
        }

        public string LogFileDirectory { get; }

        public bool SupportsSubdirectories => pathProvider.SupportsSubdirectories;

        public string DirectorySearchPattern => pathProvider.DirectorySearchPattern;

        public static PathRoller CreateForFormattedPath(string logDirectoryPath, string filePathFormat, RollingInterval interval)
        {
            var logDirectoryAbsolutePath = string.IsNullOrEmpty(value: logDirectoryPath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(path: logDirectoryPath);

            IRollingFilePathProvider pathProvider = new FormattedRollingFilePathProvider(logDirectoryAbsolutePath: logDirectoryAbsolutePath, interval: interval, filePathFormat: filePathFormat);

            return new PathRoller(logDirectoryAbsolutePath: logDirectoryAbsolutePath, pathProvider: pathProvider);
        }

        public static PathRoller CreateForLegacyPath(string path, RollingInterval interval)
        {
            IRollingFilePathProvider pathProvider = new DefaultRollingFilePathProvider(interval: interval, Path.GetFullPath(path: path));

            var logFileDirectory = Path.GetDirectoryName(path: path);
            if (string.IsNullOrEmpty(value: logFileDirectory))
                logFileDirectory = Directory.GetCurrentDirectory();
            logFileDirectory = Path.GetFullPath(path: logFileDirectory);

            return new PathRoller(logDirectoryAbsolutePath: logFileDirectory, pathProvider: pathProvider);
        }

        public void GetLogFilePath(DateTime date, int? sequenceNumber, out string path)
        {
            // The IRollingFilePathProvider will include the log directory path in the output file-name, so this method doesn't need to prefix `this.LogFileDirectory`.
            path = pathProvider.GetRollingLogFilePath(instant: date, sequenceNumber: sequenceNumber);
        }

        /// <summary> Filters <paramref name="files" /> to only those files that match the current log file name format, then converts them into <see cref="RollingLogFile" /> instances. </summary>
        public IEnumerable<RollingLogFile> SelectMatches(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                if (pathProvider.MatchRollingLogFilePath(file: file, out var periodStart, out var sequenceNumber, out var pathWithoutSequence))
                    yield return new RollingLogFile(file: file, periodStart: periodStart, sequenceNumber: sequenceNumber, pathWithoutSequence: pathWithoutSequence);
            }
        }

        public DateTime? GetCurrentCheckpoint(DateTime instant) => pathProvider.Interval.GetCurrentCheckpoint(instant: instant);

        public DateTime? GetNextCheckpoint(DateTime instant) => pathProvider.Interval.GetNextCheckpoint(instant: instant);
    }
}
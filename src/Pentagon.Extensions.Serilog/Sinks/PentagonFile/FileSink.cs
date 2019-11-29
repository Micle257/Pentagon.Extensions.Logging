// -----------------------------------------------------------------------
//  <copyright file="FileSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.IO;
    using System.Text;
    using Events;
    using Formatting;

    /// <summary> Write log events to a disk file. </summary>
    public sealed class FileSink : IFileSink, IDisposable
    {
        internal readonly FileStream _underlyingStream;
        readonly TextWriter _output;
        readonly ITextFormatter _textFormatter;
        readonly long? _fileSizeLimitBytes;
        readonly bool _buffered;
        readonly object _syncRoot = new object();
        readonly WriteCountingStream _countingStreamWrapper;

        /// <summary> Construct a <see cref="FileSink" />. </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="textFormatter"> Formatter used to convert log events to text. </param>
        /// <param name="fileSizeLimitBytes"> The approximate maximum size, in bytes, to which a log file will be allowed to grow. For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial events, the last event within the limit will be written in full even if it exceeds the limit. </param>
        /// <param name="encoding"> Character encoding used to write the text file. The default is UTF-8 without BOM. </param>
        /// <param name="buffered"> Indicates if flushing to the output file can be buffered or not. The default is false. </param>
        /// <returns> Configuration object allowing method chaining. </returns>
        /// <remarks> This constructor preserves compatibility with early versions of the public API. New code should not depend on this type. </remarks>
        /// <exception cref="IOException"> </exception>
        [Obsolete(message: "This type and constructor will be removed from the public API in a future version; use `WriteTo.File()` instead.")]
        public FileSink(string path, ITextFormatter textFormatter, long? fileSizeLimitBytes, Encoding encoding = null, bool buffered = false)
                : this(path: path, textFormatter: textFormatter, fileSizeLimitBytes: fileSizeLimitBytes, encoding: encoding, buffered: buffered, null) { }

        // This overload should be used internally; the overload above maintains compatibility with the earlier public API.
        internal FileSink(
                string path,
                ITextFormatter textFormatter,
                long? fileSizeLimitBytes,
                Encoding encoding,
                bool buffered,
                FileLifecycleHooks hooks)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
                throw new ArgumentException(message: "Negative value provided; file size limit must be non-negative.");
            _textFormatter      = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            _fileSizeLimitBytes = fileSizeLimitBytes;
            _buffered           = buffered;

            var directory = Path.GetDirectoryName(path: path);
            if (!string.IsNullOrWhiteSpace(value: directory) && !Directory.Exists(path: directory))
                Directory.CreateDirectory(path: directory);

            Stream outputStream = _underlyingStream = File.Open(path: path, mode: FileMode.Append, access: FileAccess.Write, share: FileShare.Read);
            if (_fileSizeLimitBytes != null)
                outputStream = _countingStreamWrapper = new WriteCountingStream(stream: _underlyingStream);

            // Parameter reassignment.
            encoding = encoding ?? new UTF8Encoding(false);

            if (hooks != null)
            {
                outputStream = hooks.OnFileOpened(underlyingStream: outputStream, encoding: encoding) ??
                               throw new InvalidOperationException($"The file lifecycle hook `{nameof(FileLifecycleHooks.OnFileOpened)}(...)` returned `null`.");
            }

            _output = new StreamWriter(stream: outputStream, encoding: encoding);
        }

        bool IFileSink.EmitOrOverflow(LogEvent logEvent)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));
            lock (_syncRoot)
            {
                if (_fileSizeLimitBytes != null)
                {
                    if (_countingStreamWrapper.CountedLength >= _fileSizeLimitBytes.Value)
                        return false;
                }

                _textFormatter.Format(logEvent: logEvent, output: _output);
                if (!_buffered)
                    _output.Flush();

                return true;
            }
        }

        /// <inheritdoc />
        public FileStream GetStream() => _underlyingStream;

        /// <summary> Emit the provided log event to the sink. </summary>
        /// <param name="logEvent"> The log event to write. </param>
        public void Emit(LogEvent logEvent)
        {
            ((IFileSink) this).EmitOrOverflow(logEvent: logEvent);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_syncRoot)
            {
                _output.Dispose();
            }
        }

        /// <inheritdoc />
        public void FlushToDisk()
        {
            lock (_syncRoot)
            {
                _output.Flush();
                _underlyingStream.Flush(true);
            }
        }
    }
}
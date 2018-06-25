// -----------------------------------------------------------------------
//  <copyright file="FileLoggerProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.File
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class FileLoggerProvider : ILoggerProvider
    {
        readonly IFileAsyncWriter _fileWriter;
        readonly IOptionsMonitor<FileLoggerOptions> _options;

        readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        readonly IDisposable _optionsReloadToken;

        public FileLoggerProvider(IFileAsyncWriter fileWriter, IOptionsMonitor<FileLoggerOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _options = options;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(_fileWriter, name, _options));
        }
    }
}
// -----------------------------------------------------------------------
//  <copyright file="FileLogger.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.File
{
    using System;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class FileLogger : ILogger, IDisposable
    {
        readonly IFileAsyncWriter _fileWriter;
        readonly string _categoryName;

        FileLoggerOptions _options;
        readonly IDisposable _optionsReloadToken;

        public FileLogger(IFileAsyncWriter fileWriter, string categoryName, IOptionsMonitor<FileLoggerOptions> options)
        {
            _fileWriter = fileWriter;
            _categoryName = categoryName;

            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => null;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _options.LogLevel;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var currentTime = DateTimeOffset.Now;

            var logMessage = new StringBuilder();

            if (_options.IncludeTimeStamp)
                logMessage.Append($"{currentTime:yyyy-MM-dd HH:mm:ss.fffff} ");

            logMessage.Append($"[{logLevel}] ");

            if (_options.IncludeCategory)
                logMessage.Append($"{_categoryName}[{eventId.Id}]: ");

            var message = formatter(state, exception);

            message = LoggerSourceFormatter.GetOffsetLines(message, _options.IndentMultiLinesFormat);

            logMessage.Append(message);

            logMessage.Append(Environment.NewLine);

            var output = logMessage.ToString();

            _fileWriter.AddMessage(currentTime, output);
        }

        void ReloadLoggerOptions(FileLoggerOptions options)
        {
            _options = options;
        }
    }
}
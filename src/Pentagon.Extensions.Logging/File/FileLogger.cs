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
    using Microsoft.Extensions.Logging.Abstractions.Internal;
    using Microsoft.Extensions.Options;

    public class FileLogger : ILogger
    {
        readonly IFileAsyncWriter _fileWriter;

        public FileLogger()
        {

        }
        
        internal FileLogger(string name, Func<string, LogLevel, bool> filter, IExternalScopeProvider scopeProvider,IFileAsyncWriter fileWriter )
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _fileWriter = fileWriter;

            Filter = filter ?? ((c, l) => true);
            ScopeProvider = scopeProvider;
        }

        public bool IncludeScopes { get; }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        private Func<string, LogLevel, bool> _filter;
        public Func<string, LogLevel, bool> Filter
        {
            get => _filter;
            set
            {
                _filter = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public string Name { get;  }
        
        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            return Filter(Name, logLevel);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var currentTime = DateTimeOffset.Now;

            var logMessage = new StringBuilder();

            if (IncludeTimeStamp)
                logMessage.Append($"{currentTime:yyyy-MM-dd HH:mm:ss.fffff} ");

            logMessage.Append($"[{logLevel}] ");

            if (IncludeCategory)
                logMessage.Append($"{Name}[{eventId.Id}]: ");

            var message = formatter(state, exception);

            message = LoggerSourceFormatter.GetOffsetLines(message, IndentMultiLinesFormat);

            logMessage.Append(message);

            logMessage.Append(Environment.NewLine);

            var output = logMessage.ToString();

            _fileWriter.AddMessage(currentTime, output);
        }

        public string IndentMultiLinesFormat { get; set; }

        public bool IncludeCategory { get; set; }

        public bool IncludeTimeStamp { get; set; }
    }
}
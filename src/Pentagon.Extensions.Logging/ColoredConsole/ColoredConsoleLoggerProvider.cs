// -----------------------------------------------------------------------
//  <copyright file="ColoredConsoleLoggerProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.ColoredConsole
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;

    public class ColoredConsoleLoggerProvider : ILoggerProvider
    {
        readonly ColoredConsoleLoggerConfiguration _config;
        readonly ConcurrentDictionary<string, ColoredConsoleLogger> _loggers = new ConcurrentDictionary<string, ColoredConsoleLogger>();

        public ColoredConsoleLoggerProvider(ColoredConsoleLoggerConfiguration config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ColoredConsoleLogger(name, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
namespace Pentagon.Extensions.Logging {
    using System;
    using Microsoft.Extensions.Logging;

    public class ColoredConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly ColoredConsoleLoggerConfiguration _config;

        public ColoredConsoleLogger(string name, ColoredConsoleLoggerConfiguration config)
        {
            _name = name;
            _config = config;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;

            try
            {
                if (_config.Colors.TryGetValue(logLevel, out var color))
                {
                    Console.ForegroundColor = color.Foreground;
                    Console.BackgroundColor = color.Background;
                }

                Console.WriteLine($"{logLevel.ToString()} - {eventId.Id} - {_name} - {formatter(state, exception)}");
            }
            finally
            {

                Console.ForegroundColor = foregroundColor;
                Console.BackgroundColor = backgroundColor;
            }
        }
    }
}
namespace Pentagon.Extensions.Logging {
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public static class ColoredConsoleLoggerExtensions
    {
        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, ColoredConsoleLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(config));

            return loggerFactory;
        }

        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory)
        {
            var config = new ColoredConsoleLoggerConfiguration();

            return loggerFactory.AddColoredConsoleLogger(config);
        }

        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, Action<ColoredConsoleLoggerConfiguration> configure)
        {
            var config = new ColoredConsoleLoggerConfiguration();

            configure?.Invoke(config);

            return loggerFactory.AddColoredConsoleLogger(config);
        }

        public static ILoggingBuilder AddColoredConsole(this ILoggingBuilder loggerFactory, ColoredConsoleLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(config));

            return loggerFactory;
        }

        public static ILoggingBuilder AddColoredConsole(this ILoggingBuilder loggerFactory)
        {
            var config = new ColoredConsoleLoggerConfiguration();

            return loggerFactory.AddColoredConsole(config);
        }

        public static ILoggingBuilder AddColoredConsole(this ILoggingBuilder loggerFactory, Action<ColoredConsoleLoggerConfiguration> configure)
        {
            var config = new ColoredConsoleLoggerConfiguration();

            configure?.Invoke(config);

            return loggerFactory.AddColoredConsole(config);
        }
    }
}
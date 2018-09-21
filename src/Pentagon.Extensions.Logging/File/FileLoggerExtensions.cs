namespace Pentagon.Extensions.Logging.File {
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Configuration;
    using Microsoft.Extensions.Options;

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<FileLoggerOptions>, FileLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<FileLoggerOptions>, LoggerProviderOptionsChangeTokenSource<FileLoggerOptions, FileLoggerProvider>>());

            builder.Services.AddTransient<IFileAsyncWriter, FileAsyncWriter>();

            return builder;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            builder.AddFile();
            builder.Services.Configure(configure);

            return builder;
        }

        public static ILoggerFactory AddFile(this ILoggerFactory factory)
        {
            return factory.AddFile(includeScopes: false);
        }

        public static ILoggerFactory AddFile(this ILoggerFactory factory, bool includeScopes)
        {
            factory.AddFile((n, l) => l >= LogLevel.Information, includeScopes);
            return factory;
        }

        public static ILoggerFactory AddFile(this ILoggerFactory factory, LogLevel minLevel)
        {
            factory.AddFile(minLevel, includeScopes: false);
            return factory;
        }

        public static ILoggerFactory AddFile(
                this ILoggerFactory factory,
                LogLevel minLevel,
                bool includeScopes)
        {
            factory.AddFile((category, logLevel) => logLevel >= minLevel, includeScopes);
            return factory;
        }

        public static ILoggerFactory AddFile(
                this ILoggerFactory factory,
                Func<string, LogLevel, bool> filter)
        {
            factory.AddFile(filter, includeScopes: false);
            return factory;
        }

        public static ILoggerFactory AddFile(
                this ILoggerFactory factory,
                Func<string, LogLevel, bool> filter,
                bool includeScopes)
        {
            factory.AddProvider(new FileLoggerProvider(filter, includeScopes));
            return factory;
        }
    }
}
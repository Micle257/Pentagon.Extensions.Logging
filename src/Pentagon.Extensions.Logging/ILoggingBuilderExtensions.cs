// -----------------------------------------------------------------------
//  <copyright file="ILoggingBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;

    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder WithOptions([NotNull] this ILoggingBuilder loggingBuilder, [NotNull] StaticLoggingOptions options)
        {
            if (loggingBuilder == null)
                throw new ArgumentNullException(nameof(loggingBuilder));

            LoggerSourceFormatter.SuppressSource = options.SuppressSource;
            LoggerSourceFormatter.OffsetLinePrepend = options.OffsetLinePrepend;
            MethodLogger.Options = options.Options;

            return loggingBuilder;
        }

        public static ILoggingBuilder WithOptions([NotNull] this ILoggingBuilder loggingBuilder, [NotNull] Action<StaticLoggingOptions> configure)
        {
            if (loggingBuilder == null)
                throw new ArgumentNullException(nameof(loggingBuilder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new StaticLoggingOptions();

            configure(options);

            return WithOptions(loggingBuilder, options);
        }
    }
}
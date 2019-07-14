// -----------------------------------------------------------------------
//  <copyright file="ILoggerExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;

    /// <summary> Provides extension methods for <see cref="ILogger" />. </summary>
    public static class ILoggerExtensions
    {
        public static IDisposable LogMethod(this ILogger logger,
                                            object input = null,
                                            [CallerMemberName] string methodName = null,
                                            [CallerFilePath] string typePath = null) =>
                MethodLogger.Log(logger, input, methodName, typePath);

        /// <summary> Begins a logical operation scope with given data. Calls <see cref="ILogger.BeginScope{TState}" />. </summary>
        /// <param name="logger"> The logger. </param>
        /// <param name="items"> The items. </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> logger or items </exception>
        public static IDisposable InScope([NotNull] this ILogger logger, [NotNull] params (string key, object value)[] items)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (items == null || items.Length == 0)
                throw new ArgumentNullException(nameof(items));

            var data = items.ToDictionary(a => a.key, a => a.value);

            return logger.BeginScope(data);
        }
    }
}
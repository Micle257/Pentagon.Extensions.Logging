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
    using Microsoft.Extensions.Logging;

    /// <summary> Provides extension methods for <see cref="ILogger" />. </summary>
    public static class ILoggerExtensions
    {
        public static IDisposable LogMethod(this ILogger logger,
                                            string info = null,
                                            [CallerMemberName] string methodName = null,
                                            [CallerFilePath] string typePath = null,
                                            [CallerLineNumber] int lineNumber = 0) =>
                MethodLogger.Log(logger, info, methodName, typePath, lineNumber);

        public static void LogSource(
                this ILogger logger,
                LogLevel logLevel,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args) => logger.Log(logLevel, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogCriticalSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Critical, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogErrorSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Error, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogWarningSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Warning, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogInformationSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Information, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogDebugSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Debug, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);

        public static void LogTraceSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Trace, eventId, new object[] {origin, filePath, lineNumber, message}.Concat(args), exception, LoggerSourceFormatter.Format);
    }
}
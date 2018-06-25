// -----------------------------------------------------------------------
//  <copyright file="ILoggerExtensions.cs">
//   Copyright (c) Smartdata s.r.o. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;

    public static class ILoggerExtensions
    {
        public static ArgumentNullException LogArgumentNullException(this ILogger logger, string parameterName, string message = null)
        {
            var ex = message == null ? new ArgumentNullException(parameterName) : new ArgumentNullException(parameterName, message);
            logger.LogErrorSource(message, exception:ex);
            return ex;
        }

        public static ArgumentException LogArgumentException(this ILogger logger, string message, string parameterName = null)
        {
            var ex = parameterName == null ? new ArgumentException(message) : new ArgumentException(message, parameterName);
            logger.LogErrorSource(message, exception: ex);
            return ex;
        }

        public static void LogBeginMethod(this ILogger logger, [CallerMemberName] string methodName = null, [CallerFilePath] string typePath = null)
        {
            var name = Path.GetFileName(typePath);
            logger.LogTrace($"Begin of method '{methodName}' in file '{name}'");
        }

        public static void LogEndMethod(this ILogger logger, [CallerMemberName] string methodName = null, [CallerFilePath] string typePath = null)
        {
            var name = Path.GetFileName(typePath);
            logger.LogTrace($"End of method '{methodName}' in file '{name}'");
        }

        public static void LogSource(
                this ILogger logger,
                LogLevel logLevel,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args) => logger.Log(logLevel, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);

        public static void LogCriticalSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args) => logger.Log(LogLevel.Critical, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message}), exception, LoggerSourceFormatter.Format);

        public static void LogErrorSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args) 
            => logger.Log(LogLevel.Error, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);
        
        public static void LogWarningSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Warning, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);

        public static void LogInformationSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Information, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);

        public static void LogDebugSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Debug, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);

        public static void LogTraceSource(
                this ILogger logger,
                string message,
                EventId eventId = new EventId(),
                Exception exception = null,
                [CallerMemberName] string origin = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                params object[] args)
            => logger.Log(LogLevel.Debug, eventId, args.Prepend(new object[] { origin, filePath, lineNumber, message }), exception, LoggerSourceFormatter.Format);
    }
}
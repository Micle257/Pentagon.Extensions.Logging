// -----------------------------------------------------------------------
//  <copyright file="FileLoggerConfigurationExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable RedundantArgumentDefaultValue, MethodOverloadWithOptionalParameter

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Text;
    using Configuration;
    using Core;
    using Debugging;
    using Events;
    using Formatting;
    using Formatting.Display;
    using Formatting.Json;

    /// <summary> Extends <see cref="LoggerConfiguration" /> with methods to add file sinks. </summary>
    public static class FileLoggerConfigurationExtensions
    {
        const int DefaultRetainedFileCountLimit = 31; // A long month of logs
        const long DefaultFileSizeLimitBytes = 1L * 1024 * 1024 * 1024;
        const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary> Write log events to the specified file. </summary>
        /// <param name="sinkConfiguration"> Logger sink configuration. </param>
        /// <param name="path"> Path to the file. </param>
        /// <param name="restrictedToMinimumLevel"> The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch" /> is specified. </param>
        /// <param name="levelSwitch"> A switch allowing the pass-through minimum level to be changed at runtime. </param>
        /// <param name="formatProvider"> Supplies culture-specific formatting information, or null. </param>
        /// <param name="outputTemplate"> A message template describing the format used to write to the sink. the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}". </param>
        /// <param name="fileSizeLimitBytes"> The approximate maximum size, in bytes, to which a log file will be allowed to grow. For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial events, the last event within the limit will be written in full even if it exceeds the limit. </param>
        /// <param name="buffered"> Indicates if flushing to the output file can be buffered or not. The default is false. </param>
        /// <param name="shared"> Allow the log file to be shared by multiple processes. The default is false. </param>
        /// <param name="flushToDiskInterval"> If provided, a full disk flush will be performed periodically at the specified interval. </param>
        /// <param name="rollingInterval"> The interval at which logging will roll over to a new file. </param>
        /// <param name="rollOnFileSizeLimit"> If <code>true</code>, a new file will be created when the file size limit is reached. Filenames will have a number appended in the format <code>_NNN</code>, with the first filename given no number. </param>
        /// <param name="retainedFileCountLimit"> The maximum number of log files that will be retained, including the current log file. For unlimited retention, pass null. The default is 31. </param>
        /// <param name="encoding"> Character encoding used to write the text file. The default is UTF-8 without BOM. </param>
        /// <param name="hooks"> Optionally enables hooking into log file lifecycle events. </param>
        /// <returns> Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration File(
                this LoggerSinkConfiguration sinkConfiguration,
                string path,
                string pathFormat = null,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                string outputTemplate = DefaultOutputTemplate,
                IFormatProvider formatProvider = null,
                long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
                LoggingLevelSwitch levelSwitch = null,
                bool buffered = false,
                TimeSpan? flushToDiskInterval = null,
                RollingInterval rollingInterval = RollingInterval.Infinite,
                bool rollOnFileSizeLimit = false,
                int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
                Encoding encoding = null,
                bool useGzip = false)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (outputTemplate == null)
                throw new ArgumentNullException(nameof(outputTemplate));

            var formatter = new MessageTemplateTextFormatter(outputTemplate: outputTemplate, formatProvider: formatProvider);
            return File(sinkConfiguration: sinkConfiguration,
                        formatter: formatter,
                        path: path,
                        pathFormat: pathFormat,
                        restrictedToMinimumLevel: restrictedToMinimumLevel,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        levelSwitch: levelSwitch,
                        buffered: buffered,
                        flushToDiskInterval: flushToDiskInterval,
                        rollingInterval: rollingInterval,
                        rollOnFileSizeLimit: rollOnFileSizeLimit,
                        retainedFileCountLimit: retainedFileCountLimit,
                        encoding: encoding,
                        useGzip: useGzip);
        }

        /// <summary> Write log events to the specified file. </summary>
        /// <param name="sinkConfiguration"> Logger sink configuration. </param>
        /// <param name="formatter">
        ///     A formatter, such as <see cref="JsonFormatter" />, to convert the log events into text for the file. If control of regular text formatting is required, use the other overload of <see
        ///                                                                                                                                                                                           cref="File(LoggerSinkConfiguration, string, LogEventLevel, string, IFormatProvider, long?, LoggingLevelSwitch, bool, bool, TimeSpan?, RollingInterval, bool, int?, Encoding, FileLifecycleHooks)" /> and specify the outputTemplate parameter instead.
        /// </param>
        /// <param name="path"> Path to the file. </param>
        /// <param name="restrictedToMinimumLevel"> The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch" /> is specified. </param>
        /// <param name="levelSwitch"> A switch allowing the pass-through minimum level to be changed at runtime. </param>
        /// <param name="fileSizeLimitBytes"> The approximate maximum size, in bytes, to which a log file will be allowed to grow. For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial events, the last event within the limit will be written in full even if it exceeds the limit. </param>
        /// <param name="buffered"> Indicates if flushing to the output file can be buffered or not. The default is false. </param>
        /// <param name="shared"> Allow the log file to be shared by multiple processes. The default is false. </param>
        /// <param name="flushToDiskInterval"> If provided, a full disk flush will be performed periodically at the specified interval. </param>
        /// <param name="rollingInterval"> The interval at which logging will roll over to a new file. </param>
        /// <param name="rollOnFileSizeLimit"> If <code>true</code>, a new file will be created when the file size limit is reached. Filenames will have a number appended in the format <code>_NNN</code>, with the first filename given no number. </param>
        /// <param name="retainedFileCountLimit"> The maximum number of log files that will be retained, including the current log file. For unlimited retention, pass null. The default is 31. </param>
        /// <param name="encoding"> Character encoding used to write the text file. The default is UTF-8 without BOM. </param>
        /// <param name="hooks"> Optionally enables hooking into log file lifecycle events. </param>
        /// <returns> Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration File(
                this LoggerSinkConfiguration sinkConfiguration,
                ITextFormatter formatter,
                string path,
                string pathFormat = null,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
                LoggingLevelSwitch levelSwitch = null,
                bool buffered = false,
                TimeSpan? flushToDiskInterval = null,
                RollingInterval rollingInterval = RollingInterval.Infinite,
                bool rollOnFileSizeLimit = false,
                int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
                Encoding encoding = null,
                bool useGzip = false)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return ConfigureFile(addSink: sinkConfiguration.Sink,
                                 formatter: formatter,
                                 path: path,
                                 pathFormat: pathFormat,
                                 restrictedToMinimumLevel: restrictedToMinimumLevel,
                                 fileSizeLimitBytes: fileSizeLimitBytes,
                                 levelSwitch: levelSwitch,
                                 buffered: buffered,
                                 false,
                                 flushToDiskInterval: flushToDiskInterval,
                                 encoding: encoding,
                                 rollingInterval: rollingInterval,
                                 rollOnFileSizeLimit: rollOnFileSizeLimit,
                                 retainedFileCountLimit: retainedFileCountLimit,
                                 useGzip: useGzip);
        }

        /// <summary> Write audit log events to the specified file. </summary>
        /// <param name="sinkConfiguration"> Logger sink configuration. </param>
        /// <param name="path"> Path to the file. </param>
        /// <param name="restrictedToMinimumLevel"> The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch" /> is specified. </param>
        /// <param name="levelSwitch"> A switch allowing the pass-through minimum level to be changed at runtime. </param>
        /// <param name="formatProvider"> Supplies culture-specific formatting information, or null. </param>
        /// <param name="outputTemplate"> A message template describing the format used to write to the sink. the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}". </param>
        /// <param name="encoding"> Character encoding used to write the text file. The default is UTF-8 without BOM. </param>
        /// <param name="hooks"> Optionally enables hooking into log file lifecycle events. </param>
        /// <returns> Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration File(
                this LoggerAuditSinkConfiguration sinkConfiguration,
                string path,
                string pathFormat = null,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                string outputTemplate = DefaultOutputTemplate,
                IFormatProvider formatProvider = null,
                LoggingLevelSwitch levelSwitch = null,
                Encoding encoding = null,
                bool useGzip = false)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (outputTemplate == null)
                throw new ArgumentNullException(nameof(outputTemplate));

            var formatter = new MessageTemplateTextFormatter(outputTemplate: outputTemplate, formatProvider: formatProvider);
            return File(sinkConfiguration: sinkConfiguration,
                        formatter: formatter,
                        path: path,
                        pathFormat: pathFormat,
                        restrictedToMinimumLevel: restrictedToMinimumLevel,
                        levelSwitch: levelSwitch,
                        encoding: encoding,
                        useGzip: useGzip);
        }

        /// <summary> Write audit log events to the specified file. </summary>
        /// <param name="sinkConfiguration"> Logger sink configuration. </param>
        /// <param name="formatter">
        ///     A formatter, such as <see cref="JsonFormatter" />, to convert the log events into text for the file. If control of regular text formatting is required, use the other overload of <see
        ///                                                                                                                                                                                           cref="File(LoggerAuditSinkConfiguration, string, LogEventLevel, string, IFormatProvider, LoggingLevelSwitch, Encoding, FileLifecycleHooks)" /> and specify the outputTemplate parameter instead.
        /// </param>
        /// <param name="path"> Path to the file. </param>
        /// <param name="restrictedToMinimumLevel"> The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch" /> is specified. </param>
        /// <param name="levelSwitch"> A switch allowing the pass-through minimum level to be changed at runtime. </param>
        /// <param name="encoding"> Character encoding used to write the text file. The default is UTF-8 without BOM. </param>
        /// <param name="hooks"> Optionally enables hooking into log file lifecycle events. </param>
        /// <returns> Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration File(
                this LoggerAuditSinkConfiguration sinkConfiguration,
                ITextFormatter formatter,
                string path,
                string pathFormat = null,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                LoggingLevelSwitch levelSwitch = null,
                Encoding encoding = null,
                bool useGzip = false)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return ConfigureFile(addSink: sinkConfiguration.Sink,
                                 formatter: formatter,
                                 path: path,
                                 pathFormat: pathFormat,
                                 restrictedToMinimumLevel: restrictedToMinimumLevel,
                                 null,
                                 levelSwitch: levelSwitch,
                                 false,
                                 true,
                                 null,
                                 encoding: encoding,
                                 rollingInterval: RollingInterval.Infinite,
                                 false,
                                 null,
                                 useGzip: useGzip);
        }

        static LoggerConfiguration ConfigureFile(
                this Func<ILogEventSink, LogEventLevel, LoggingLevelSwitch, LoggerConfiguration> addSink,
                ITextFormatter formatter,
                string path,
                string pathFormat,
                LogEventLevel restrictedToMinimumLevel,
                long? fileSizeLimitBytes,
                LoggingLevelSwitch levelSwitch,
                bool buffered,
                bool propagateExceptions,
                TimeSpan? flushToDiskInterval,
                Encoding encoding,
                RollingInterval rollingInterval,
                bool rollOnFileSizeLimit,
                int? retainedFileCountLimit,
                bool useGzip)
        {
            if (addSink == null)
                throw new ArgumentNullException(nameof(addSink));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
                throw new ArgumentException(message: "Negative value provided; file size limit must be non-negative.", nameof(fileSizeLimitBytes));
            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
                throw new ArgumentException(message: "At least one file must be retained.", nameof(retainedFileCountLimit));

            PathRoller pathRoller;
            if (pathFormat == null)
                pathRoller = PathRoller.CreateForLegacyPath(path: path, interval: rollingInterval);
            else
                pathRoller = PathRoller.CreateForFormattedPath(logDirectoryPath: path, filePathFormat: pathFormat, interval: rollingInterval);

            ILogEventSink      sink;
            FileLifecycleHooks hooks = null;

            if (useGzip)
                hooks = new GZipHooks();

            if (rollOnFileSizeLimit || rollingInterval != RollingInterval.Infinite)
            {
                sink = new RollingFileSink(roller: pathRoller,
                                           textFormatter: formatter,
                                           fileSizeLimitBytes: fileSizeLimitBytes,
                                           retainedFileCountLimit: retainedFileCountLimit,
                                           encoding: encoding,
                                           buffered: buffered,
                                           rollOnFileSizeLimit: rollOnFileSizeLimit,
                                           hooks: hooks);
            }
            else
            {
                try
                {
                    sink = new FileSink(path: path, textFormatter: formatter, fileSizeLimitBytes: fileSizeLimitBytes, encoding: encoding, buffered: buffered, hooks: hooks);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine(format: "Unable to open file sink for {0}: {1}", arg0: path, arg1: ex);

                    if (propagateExceptions)
                        throw;

                    return addSink(new NullSink(), arg2: LevelAlias.Maximum, null);
                }
            }

            if (flushToDiskInterval.HasValue)
            {
#pragma warning disable 618
                sink = new PeriodicFlushToDiskSink(sink: sink, flushInterval: flushToDiskInterval.Value);
#pragma warning restore 618
            }

            return addSink(arg1: sink, arg2: restrictedToMinimumLevel, arg3: levelSwitch);
        }
    }
}
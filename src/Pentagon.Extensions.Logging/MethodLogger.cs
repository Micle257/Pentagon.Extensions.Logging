// -----------------------------------------------------------------------
//  <copyright file="MethodLogger.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;

    public class MethodLogger : IDisposable
    {
        readonly ILogger _logger;
        readonly object _info;
        readonly string _methodName;
        readonly string _typePath;
        readonly Stopwatch _sw;
        readonly string _fileName;
        readonly Guid _trace;
        readonly int _lineNumber;

        MethodLogger(ILogger logger, object info, string methodName, string typePath, int lineNumber)
        {
            _logger = logger;
            _info = info;
            _methodName = methodName;
            _typePath = typePath;
            _lineNumber = lineNumber;
            _fileName = Path.GetFileName(typePath);
            _trace = Guid.NewGuid();

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
            {
                _sw = new Stopwatch();
                _sw.Start();
            }

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.Entry))
                _logger.LogTraceSource($"Begin of method '{methodName}' in file '{_fileName}'{info}.", default, null, _methodName, _typePath, _lineNumber, $"{{Trace: {_trace}}}");
        }

        public void Dispose()
        {
            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
            {
                _sw?.Stop();
                _logger.LogTraceSource($"Method '{_methodName}' finished in {_sw.Elapsed}.", default, null, _methodName, _typePath, _lineNumber, $"{{Trace: {_trace}}}");
            }

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.Exit) && !StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
                _logger.LogTraceSource($"End of method '{_methodName}' in file '{_fileName}'.", default, null, _methodName, _typePath, _lineNumber, $"{{Trace: {_trace}}}");
        }

        /// <summary> Log method entry </summary>
        /// <param name="methodName"> The name of the method being logged </param>
        /// <param name="options"> The log options </param>
        /// <returns> A disposable object or none if logging is disabled </returns>
        public static IDisposable Log(ILogger logger,
                                      object info = null,
                                      [CallerMemberName] string methodName = null,
                                      [CallerFilePath] string typePath = null,
                                      [CallerLineNumber] int lineNumber = 0) => new MethodLogger(logger, info, methodName, typePath, lineNumber);
    }
}
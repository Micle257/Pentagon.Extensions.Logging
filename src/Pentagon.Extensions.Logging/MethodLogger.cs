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
        IDisposable _loggerScope;
        readonly ILogger _logger;
        readonly object _info;
        readonly string _methodName;
        readonly string _typePath;
        readonly Stopwatch _sw;
        readonly string _fileName;
        readonly Guid _trace;

        MethodLogger(ILogger logger, object info, string methodName, string typePath)
        {
            _logger = logger;
            _info = info;
            _methodName = methodName;
            _typePath = typePath;
            _fileName = Path.GetFileName(typePath);
            _trace = Guid.NewGuid();

            if (StaticLoggingOptions.Options == 0)
                return;

            _loggerScope = _logger.InScope(("MethodName", _methodName), ("MethodTrace", _trace), ("MethodFilePath", _typePath), ("MethodInfo", info));

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
            {
                _sw = new Stopwatch();
                _sw.Start();
            }

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.Entry))
                _logger.LogTrace("Begin of method '{MethodName}' in file '{FileName}'.", _methodName, _fileName);
        }

        public void Dispose()
        {
            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
            {
                _sw?.Stop();
                _logger.LogTrace("Method '{MethodName}' finished in {TimeElapsed}.", _methodName, _sw.Elapsed);
            }

            if (StaticLoggingOptions.Options.HasFlag(MethodLogOptions.Exit) && !StaticLoggingOptions.Options.HasFlag(MethodLogOptions.ExecutionTime))
                _logger.LogTrace("End of method '{MethodName}' in file '{FileName}'.", _methodName, _fileName);

            _loggerScope?.Dispose();
        }

        /// <summary>
        /// Log method entry
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="info">The information.</param>
        /// <param name="methodName">The name of the method being logged</param>
        /// <param name="typePath">The type path.</param>
        /// <returns>
        /// A disposable object or none if logging is disabled.
        /// </returns>
        public static IDisposable Log(ILogger logger,
                                      object info = null,
                                      [CallerMemberName] string methodName = null,
                                      [CallerFilePath] string typePath = null) => new MethodLogger(logger, info, methodName, typePath);
    }
}
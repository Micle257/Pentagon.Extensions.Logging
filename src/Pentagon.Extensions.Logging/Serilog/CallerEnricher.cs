// -----------------------------------------------------------------------
//  <copyright file="CallerEnricher.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Serilog
{
    using System.Diagnostics;
    using System.Linq;
    using global::Serilog;
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;

            while (true)
            {
                var stack = new StackFrame(skip);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty(name: "Caller", new ScalarValue(value: "<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();

                if (method.DeclaringType.Assembly != typeof(Log).Assembly
                    && method.DeclaringType.Name != nameof(Logger)
                    && method.DeclaringType.Name != "SerilogLogger")
                {
                    var caller = $"{method.DeclaringType.FullName}.{method.Name}({method.GetParameters().Select(pi => pi.ParameterType.FullName).Aggregate((a,b) => $"{a}, {b}")})";

                    logEvent.AddPropertyIfAbsent(new LogEventProperty(name: "Caller", new ScalarValue(caller)));

                    var line = stack.GetFileLineNumber();

                    var fileName = stack.GetFileName();

                    if (line != 0)
                        logEvent.AddPropertyIfAbsent(new LogEventProperty(name: "Line", new ScalarValue(caller)));

                    if (fileName != null)
                        logEvent.AddPropertyIfAbsent(new LogEventProperty(name: "File", new ScalarValue(caller)));
                }

                skip++;
            }
        }
    }
}
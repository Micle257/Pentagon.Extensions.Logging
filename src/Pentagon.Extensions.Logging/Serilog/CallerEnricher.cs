namespace Pentagon.Extensions.Logging {
    using System.Diagnostics;
    using System.Linq;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

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
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();

                if (method.DeclaringType.Assembly != typeof(Log).Assembly
                    && method.DeclaringType.Name != nameof(Logger))
                {

                    var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";

                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));

                    var line = stack.GetFileLineNumber();

                    var fileName = stack.GetFileName();

                    if (line != 0)
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("Line", new ScalarValue(caller)));

                    if (fileName != null)
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("File", new ScalarValue(caller)));
                }

                skip++;
            }
        }
    }
}
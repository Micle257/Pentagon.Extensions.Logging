namespace Pentagon.Extensions.Logging {
    using System.Diagnostics;
    using Serilog.Core;
    using Serilog.Events;

    public class DemystifyExceptionEnricher : ILogEventEnricher
    {
        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent?.Exception?.Demystify();
        }
    }
}
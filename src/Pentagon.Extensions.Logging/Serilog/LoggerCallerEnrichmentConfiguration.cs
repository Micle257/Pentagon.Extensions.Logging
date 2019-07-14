namespace Pentagon.Extensions.Logging {
    using Serilog;
    using Serilog.Configuration;

    public static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }

        public static LoggerConfiguration WithDemystifiedException(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<DemystifyExceptionEnricher>();
        }
    }
}
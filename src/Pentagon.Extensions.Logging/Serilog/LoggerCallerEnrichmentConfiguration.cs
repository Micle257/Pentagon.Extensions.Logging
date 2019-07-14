namespace Pentagon.Extensions.Logging.Serilog {
    using global::Serilog;
    using global::Serilog.Configuration;

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
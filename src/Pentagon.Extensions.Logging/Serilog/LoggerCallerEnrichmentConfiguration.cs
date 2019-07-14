// -----------------------------------------------------------------------
//  <copyright file="LoggerCallerEnrichmentConfiguration.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Serilog
{
    using global::Serilog;
    using global::Serilog.Configuration;

    public static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration) => enrichmentConfiguration.With<CallerEnricher>();

        public static LoggerConfiguration WithDemystifiedException(this LoggerEnrichmentConfiguration enrichmentConfiguration) => enrichmentConfiguration.With<DemystifyExceptionEnricher>();
    }
}
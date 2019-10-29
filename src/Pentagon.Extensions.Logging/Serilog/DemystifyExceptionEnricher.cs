﻿// -----------------------------------------------------------------------
//  <copyright file="DemystifyExceptionEnricher.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Serilog
{
    using System.Diagnostics;
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class DemystifyExceptionEnricher : ILogEventEnricher
    {
        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent?.Exception?.Demystify();
        }
    }
}
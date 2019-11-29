// -----------------------------------------------------------------------
//  <copyright file="Clock.cs">
//   Copyright (c) Smartdata s.r.o. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Enrichers.Assembly
{
    using System.Diagnostics;
    using System.Reflection;
    using Configuration;
    using JetBrains.Annotations;

    /// <summary> Extends <see cref="LoggerConfiguration" /> to add enrichers for <see cref="System.Reflection.AssemblyName" />. capabilities. </summary>
    public static class AssemblyNameLoggerConfigurationExtensions
    {
        [NotNull]
        public static LoggerConfiguration WithAssemblyName([NotNull] this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            return enrichmentConfiguration.WithProperty(name: "AssemblyName", value: assembly.GetName().Name);
        }

        [NotNull]
        public static LoggerConfiguration WithAssemblyVersion([NotNull] this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            var assemblyLocation = assembly.Location;
            var ver              = FileVersionInfo.GetVersionInfo(fileName: assemblyLocation).ProductVersion;

            return enrichmentConfiguration.WithProperty(name: "AssemblyVersion", value: assembly.GetName().Version);
        }

        [NotNull]
        public static LoggerConfiguration WithProductVersion([NotNull] this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            var assemblyLocation = assembly.Location;
            var ver              = FileVersionInfo.GetVersionInfo(fileName: assemblyLocation).ProductVersion;

            return enrichmentConfiguration.WithProperty(name: "ProductVersion", value: ver);
        }
    }
}
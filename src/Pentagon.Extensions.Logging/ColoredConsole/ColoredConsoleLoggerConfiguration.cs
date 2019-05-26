// -----------------------------------------------------------------------
//  <copyright file="ColoredConsoleLoggerConfiguration.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.ColoredConsole
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public class ColoredConsoleLoggerConfiguration
    {
        public IDictionary<LogLevel, (ConsoleColor Foreground, ConsoleColor Background)> Colors { get; set; } =
            new Dictionary<LogLevel, (ConsoleColor Foreground, ConsoleColor Background)>
            {
                    {LogLevel.Critical, (ConsoleColor.White, ConsoleColor.Red)},
                    {LogLevel.Error, (ConsoleColor.Red, ConsoleColor.Black)},
                    {LogLevel.Warning, (ConsoleColor.Yellow, ConsoleColor.Black)},
                    {LogLevel.Information, (ConsoleColor.Blue, ConsoleColor.Black)},
                    {LogLevel.Debug, (ConsoleColor.White, ConsoleColor.Black)},
                    {LogLevel.Trace, (ConsoleColor.Gray, ConsoleColor.Black)}
            };
    }
}
// -----------------------------------------------------------------------
//  <copyright file="RollingIntervalExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;

    static class RollingIntervalExtensions
    {
        public static string GetFormat(this RollingInterval interval)
        {
            switch (interval)
            {
                case RollingInterval.Infinite:
                    return "";
                case RollingInterval.Year:
                    return "yyyy";
                case RollingInterval.Month:
                    return "yyyyMM";
                case RollingInterval.Day:
                    return "yyyyMMdd";
                case RollingInterval.Hour:
                    return "yyyyMMddHH";
                case RollingInterval.Minute:
                    return "yyyyMMddHHmm";
                default:
                    throw new ArgumentException(message: "Invalid rolling interval");
            }
        }

        public static DateTime? GetCurrentCheckpoint(this RollingInterval interval, DateTime instant)
        {
            switch (interval)
            {
                case RollingInterval.Infinite:
                    return null;
                case RollingInterval.Year:
                    return new DateTime(year: instant.Year, 1, 1, 0, 0, 0, kind: instant.Kind);
                case RollingInterval.Month:
                    return new DateTime(year: instant.Year, month: instant.Month, 1, 0, 0, 0, kind: instant.Kind);
                case RollingInterval.Day:
                    return new DateTime(year: instant.Year, month: instant.Month, day: instant.Day, 0, 0, 0, kind: instant.Kind);
                case RollingInterval.Hour:
                    return new DateTime(year: instant.Year, month: instant.Month, day: instant.Day, hour: instant.Hour, 0, 0, kind: instant.Kind);
                case RollingInterval.Minute:
                    return new DateTime(year: instant.Year, month: instant.Month, day: instant.Day, hour: instant.Hour, minute: instant.Minute, 0, kind: instant.Kind);
                default:
                    throw new ArgumentException(message: "Invalid rolling interval");
            }
        }

        public static DateTime? GetNextCheckpoint(this RollingInterval interval, DateTime instant)
        {
            var current = GetCurrentCheckpoint(interval: interval, instant: instant);
            if (current == null)
                return null;

            switch (interval)
            {
                case RollingInterval.Year:
                    return current.Value.AddYears(1);
                case RollingInterval.Month:
                    return current.Value.AddMonths(1);
                case RollingInterval.Day:
                    return current.Value.AddDays(1);
                case RollingInterval.Hour:
                    return current.Value.AddHours(1);
                case RollingInterval.Minute:
                    return current.Value.AddMinutes(1);
                default:
                    throw new ArgumentException(message: "Invalid rolling interval");
            }
        }
    }
}
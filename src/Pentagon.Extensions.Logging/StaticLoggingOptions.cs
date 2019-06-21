// -----------------------------------------------------------------------
//  <copyright file="StaticLoggingOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    public static class StaticLoggingOptions
    {
        public static bool SuppressSource { get; set; }

        public static string OffsetLinePrepend { get; set; } = "  >";

        public static MethodLogOptions Options { get; set; } = MethodLogOptions.All;
    }
}
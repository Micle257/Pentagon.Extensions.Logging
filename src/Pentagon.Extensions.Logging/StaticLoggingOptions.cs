// -----------------------------------------------------------------------
//  <copyright file="StaticLoggingOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    public class StaticLoggingOptions
    {
        public bool SuppressSource { get; set; }

        public string OffsetLinePrepend { get; set; } = "  >";

        public MethodLogOptions Options { get; set; }
    }
}
// -----------------------------------------------------------------------
//  <copyright file="MethodLogOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;

    /// <summary> Options used when logging a method </summary>
    [Flags]
    public enum MethodLogOptions
    {
        /// <summary> Log entry into the method </summary>
        Entry = 0x01,

        /// <summary> Log exit from the method </summary>
        Exit = 0x02,

        /// <summary> Log the execution time of the method </summary>
        ExecutionTime = 0x04,

        /// <summary> Log all data </summary>
        All = 0xFF
    }
}
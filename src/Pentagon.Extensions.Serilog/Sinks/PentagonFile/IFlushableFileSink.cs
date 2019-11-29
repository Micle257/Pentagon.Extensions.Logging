// -----------------------------------------------------------------------
//  <copyright file="IFlushableFileSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    /// <summary> Supported by (file-based) sinks that can be explicitly flushed. </summary>
    public interface IFlushableFileSink
    {
        /// <summary> Flush buffered contents to disk. </summary>
        void FlushToDisk();
    }
}
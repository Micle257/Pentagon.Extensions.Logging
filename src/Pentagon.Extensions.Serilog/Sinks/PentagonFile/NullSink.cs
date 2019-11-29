// -----------------------------------------------------------------------
//  <copyright file="NullSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using Core;
    using Events;

    /// <summary> An instance of this sink may be substituted when an instance of the <see cref="FileSink" /> is unable to be constructed. </summary>
    class NullSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent) { }
    }
}
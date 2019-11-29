// -----------------------------------------------------------------------
//  <copyright file="IFileSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System.IO;
    using Core;
    using Events;

    /// <summary> Exists only for the convenience of <see cref="RollingFileSink" />, which switches implementations based on sharing. Would refactor, but preserving backwards compatibility. </summary>
    interface IFileSink : ILogEventSink, IFlushableFileSink
    {
        bool EmitOrOverflow(LogEvent logEvent);

        FileStream GetStream();
    }
}
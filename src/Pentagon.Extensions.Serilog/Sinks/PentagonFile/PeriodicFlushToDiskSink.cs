// -----------------------------------------------------------------------
//  <copyright file="PeriodicFlushToDiskSink.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Threading;
    using Core;
    using Debugging;
    using Events;

    /// <summary> A sink wrapper that periodically flushes the wrapped sink to disk. </summary>
    [Obsolete(message: "This type will be removed from the public API in a future version; use `WriteTo.File(flushToDiskInterval:)` instead.")]
    public class PeriodicFlushToDiskSink : ILogEventSink, IDisposable
    {
        readonly ILogEventSink _sink;
        readonly Timer _timer;
        int _flushRequired;

        /// <summary> Construct a <see cref="PeriodicFlushToDiskSink" /> that wraps <paramref name="sink" /> and flushes it at the specified <paramref name="flushInterval" />. </summary>
        /// <param name="sink"> The sink to wrap. </param>
        /// <param name="flushInterval"> The interval at which to flush the underlying sink. </param>
        /// <exception cref="ArgumentNullException" />
        public PeriodicFlushToDiskSink(ILogEventSink sink, TimeSpan flushInterval)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));

            if (sink is IFlushableFileSink flushable)
                _timer = new Timer(_ => FlushToDisk(flushable: flushable), null, dueTime: flushInterval, period: flushInterval);
            else
            {
                _timer = new Timer(_ => { }, null, dueTime: Timeout.InfiniteTimeSpan, period: Timeout.InfiniteTimeSpan);
                SelfLog.WriteLine(format: "{0} configured to flush {1}, but {2} not implemented", typeof(PeriodicFlushToDiskSink), arg1: sink, nameof(IFlushableFileSink));
            }
        }

        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            _sink.Emit(logEvent: logEvent);
            Interlocked.Exchange(location1: ref _flushRequired, 1);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _timer.Dispose();
            (_sink as IDisposable)?.Dispose();
        }

        void FlushToDisk(IFlushableFileSink flushable)
        {
            try
            {
                if (Interlocked.CompareExchange(location1: ref _flushRequired, 0, 1) == 1)
                {
                    // May throw ObjectDisposedException, since we're not trying to synchronize
                    // anything here in the wrapper.
                    flushable.FlushToDisk();
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(format: "{0} could not flush the underlying sink to disk: {1}", typeof(PeriodicFlushToDiskSink), arg1: ex);
            }
        }
    }
}
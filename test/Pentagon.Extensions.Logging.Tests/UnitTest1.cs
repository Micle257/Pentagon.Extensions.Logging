using System;
using Xunit;

namespace Pentagon.Extensions.Logging.Tests
{
    using System.IO;
    using System.Text;
    using ColoredConsole;
    using Exceptions;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;
    using Xunit.Abstractions;

    public class UnitTest1
    {
        readonly ITestOutputHelper _outputHelper;

        public UnitTest1(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Test1()
        {
            var f = new Foo();

            f.DoDoing(2, "sad");
        }

        [Fact]
        public void Test2()
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Verbose()
                         .Enrich.WithCaller()
                         .Enrich.WithDemystifiedException()
                         .WriteTo.Sink(new Sink(_outputHelper, new NewLineOffsetFormatter()))
                    .CreateLogger();

            Log.Debug("test\ndsa\nqe");
            Log.Debug(new CryptographicException("sa"), "DESCRITON {B}", 15);
        }

        class Sink : ILogEventSink
        {
            readonly ITestOutputHelper _outputHelper;
            readonly ITextFormatter _formatter;

            public Sink(ITestOutputHelper outputHelper,
                        ITextFormatter formatter)
            {
                _outputHelper = outputHelper;
                _formatter = formatter;
            }

            /// <inheritdoc />
            public void Emit(LogEvent logEvent)
            {
                var original = new StringBuilder();

                using (var inWriter = new StringWriter(original))
                {
                    _formatter.Format(logEvent, inWriter);
                }

                _outputHelper.WriteLine(original.ToString());
            }
        }
    }

    class Foo
    {

        public void DoDoing(int mos, string nos)
        {
            var log = new LoggerFactory(new[]
                                        {
                                                new ColoredConsoleLoggerProvider(new ColoredConsoleLoggerConfiguration
                                                                                 { }),
                                        }).CreateLogger("Foo");

            using (log.LogMethod(input: new
            {
                mos,
                nos
            }))
            {
                Console.WriteLine(nos);
            }
        }
    }
}

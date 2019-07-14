// -----------------------------------------------------------------------
//  <copyright file="UnitTest1.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Tests
{
    using System.IO;
    using System.Text;
    using Exceptions;
    using global::Serilog;
    using global::Serilog.Core;
    using global::Serilog.Events;
    using global::Serilog.Formatting;
    using Serilog;
    using Xunit;
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

            f.DoDoing(2, nos: "sad");
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

            Log.Debug(messageTemplate: "test\ndsa\nqe");
            Log.Debug(new CryptographicException(message: "sa"), messageTemplate: "DESCRITON {B}", 15);
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
}
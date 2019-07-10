using System;
using Xunit;

namespace Pentagon.Extensions.Logging.Tests
{
    using ColoredConsole;
    using Microsoft.Extensions.Logging;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var f =new Foo();

            f.DoDoing(2, "sad");
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

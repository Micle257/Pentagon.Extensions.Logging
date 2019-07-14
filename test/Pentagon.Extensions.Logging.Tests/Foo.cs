namespace Pentagon.Extensions.Logging.Tests {
    using System;
    using ColoredConsole;
    using Microsoft.Extensions.Logging;

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
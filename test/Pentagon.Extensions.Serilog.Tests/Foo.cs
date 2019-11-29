// -----------------------------------------------------------------------
//  <copyright file="Foo.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Tests
{
    using System;
    using ColoredConsole;
    using Microsoft.Extensions.Logging;

    class Foo
    {
        public void DoDoing(int mos, string nos)
        {
            var log = new LoggerFactory(new[]
                                        {
                                                new ColoredConsoleLoggerProvider(new ColoredConsoleLoggerConfiguration())
                                        }).CreateLogger(categoryName: "Foo");

            using (log.LogMethod(new
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
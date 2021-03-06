﻿// -----------------------------------------------------------------------
//  <copyright file="Clock.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;

    static class Clock
    {
        static Func<DateTime> _dateTimeNow = () => DateTime.Now;

        [ThreadStatic]
        static DateTime _testDateTimeNow;

        public static DateTime DateTimeNow => _dateTimeNow();

        // Time is set per thread to support parallel
        // If any thread uses the clock in test mode, all threads
        // must use it in test mode; once set to test mode only
        // terminating the application returns it to normal use.
        public static void SetTestDateTimeNow(DateTime now)
        {
            _testDateTimeNow = now;
            _dateTimeNow     = () => _testDateTimeNow;
        }
    }
}
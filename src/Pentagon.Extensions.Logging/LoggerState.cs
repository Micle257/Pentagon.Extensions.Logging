// -----------------------------------------------------------------------
//  <copyright file="LoggerState.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class LoggerState
    {
        public string MethodName { get; set; }

        public string FilePath { get; set; }

        public int? LineNumber { get; set; }

        public string Message { get; set; }

        public static bool TryParse(IEnumerable<object> value, out LoggerState state, out object[] otherState)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            state = default;
            otherState = Array.Empty<object>();

            var logState = new LoggerState();
            var otherStateBuilder = new List<object>();

            using (var enumerator = value.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    logState.MethodName = enumerator.Current as string;
                else
                    return false;

                if (enumerator.MoveNext())
                    logState.FilePath = enumerator.Current as string;
                else
                    return false;

                if (enumerator.MoveNext())
                    logState.LineNumber = enumerator.Current as int?;
                else
                    return false;

                if (enumerator.MoveNext())
                    logState.Message = enumerator.Current as string;
                else
                    return false;

                while (enumerator.MoveNext())
                    otherStateBuilder.Add(enumerator.Current);
            }

            state = logState;
            otherState = otherStateBuilder.ToArray();

            return true;
        }

        public static LoggerState Parse(IEnumerable<object> value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!TryParse(value, out var state, out var other))
                throw new FormatException(message: "The format value of state is not valid");

            return state;
        }

        public static LoggerState FromCurrentPosition(string message,
                                                      [CallerMemberName] string origin = "",
                                                      [CallerFilePath] string filePath = "",
                                                      [CallerLineNumber] int lineNumber = 0)
            => new LoggerState
               {
                       MethodName = origin,
                       FilePath = filePath,
                       LineNumber = lineNumber,
                       Message = message
               };
    }
}
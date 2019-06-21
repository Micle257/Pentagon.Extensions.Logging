﻿// -----------------------------------------------------------------------
//  <copyright file="LoggerSourceFormatter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class LoggerSourceFormatter
    {
        public static string Format(IEnumerable<object> state, Exception exception)
        {
            if (!LoggerState.TryParse(state, out var logState, out var otherState))
                throw new ArgumentException();

            var msg = GetLogMessage(logState, otherState, exception);

            return GetOffsetLines(msg, StaticLoggingOptions.OffsetLinePrepend);
        }

        public static string GetOffsetLines(string value, string offsetFormat)
        {
            var lines = value.SplitToLines().ToArray();

            if (lines.Length > 1)
            {
                var messageBuilder = new StringBuilder();

                messageBuilder.Append(lines[0]);
                foreach (var line in lines.Skip(1))
                {
                    messageBuilder.Append(Environment.NewLine);
                    messageBuilder.Append(offsetFormat);
                    messageBuilder.Append(line);
                }

                return messageBuilder.ToString();
            }

            return value;
        }

        public static string GetLogMessage(LoggerState state, object[] otherState, Exception exception = null)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.Append(string.IsNullOrWhiteSpace(state.Message)
                                          ? "No message specified."
                                          : state.Message);

            if (!StaticLoggingOptions.SuppressSource
                && !string.IsNullOrWhiteSpace(state.FilePath)
                && !string.IsNullOrWhiteSpace(state.MethodName)
                && state.LineNumber.HasValue
                && !(state.LineNumber <= 0))
                messageBuilder.Append($" [{Path.GetFileName(state.FilePath)} > {state.MethodName}() > Line {state.LineNumber}]");

            if (otherState.Length != 0)
            {
                foreach (var o in otherState)
                    messageBuilder.Append(o as string ?? (o?.ToString() ?? string.Empty));
            }

            if (exception != null)
                messageBuilder.Append($"{Environment.NewLine}{exception}");

            var message = messageBuilder.ToString();

            return message;
        }
    }
}
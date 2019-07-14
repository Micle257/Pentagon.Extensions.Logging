namespace Pentagon.Extensions.Logging {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using JetBrains.Annotations;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;

    [Obsolete("WIP")]
    public class NewLineOffsetFormatter : ITextFormatter
    {
        [NotNull]
        readonly ITextFormatter _formatter;
        readonly string _offsetFormat;

        public NewLineOffsetFormatter(string offsetFormat = null, ITextFormatter formatter = null)
        {
            _formatter = formatter ?? new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", null);
            _offsetFormat = offsetFormat ?? "  > ";
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var original = new StringBuilder();

            using (var inWriter = new StringWriter(original))
            {
                _formatter.Format(logEvent, inWriter);
            }

            var offsetLines = GetOffsetLines(original.ToString(), _offsetFormat);

            output?.Write(offsetLines);
        }

        public static string GetOffsetLines(string value, string offsetFormat)
        {
            var lines = value.SplitToLines()
                             .ToArray();

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
    }
}
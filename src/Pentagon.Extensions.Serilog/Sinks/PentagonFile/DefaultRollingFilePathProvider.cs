// -----------------------------------------------------------------------
//  <copyright file="DefaultRollingFilePathProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary> Implements <see cref="IRollingFilePathProvider" /> with the default behaviour for Serilog Rolling-files as of version 4.0.0. This implementation uses hard-coded, fixed-length date formats that are appended to the configured file-name before the file extension. </summary>
    class DefaultRollingFilePathProvider : IRollingFilePathProvider
    {
        const string PeriodMatchGroup = "period";
        const string SequenceNumberMatchGroup = "sequence";

        readonly string filePathPrefix; // An absolute path that MAY contain a file-name or start of a file-name: e.g. "C:\logfiles\log-" or "C:\logfiles\".
        readonly string filePathSuffix;
        readonly string periodFormat;
        readonly Regex fileNameRegex;

        readonly string filePathFormat;

        /// <param name="logFilePathTemplate"> Path to the log file. The RollingInterval and sequence number (if applicable) will be inserted before the file-name extension. </param>
        /// <param name="interval"> Interval from which to generate file names. </param>
        public DefaultRollingFilePathProvider(RollingInterval interval, string logFilePathTemplate)
        {
            if (!Path.IsPathRooted(path: logFilePathTemplate))
                throw new ArgumentException(message: "Path format must be absolute.", nameof(logFilePathTemplate));

            Interval = interval;

            filePathPrefix = PathUtility.GetFilePathWithoutExtension(filePath: logFilePathTemplate);
            filePathSuffix = Path.GetExtension(path: logFilePathTemplate);
            periodFormat   = interval.GetFormat();

            // NOTE: This technique using Regex only works reliably if periodFormat will always generate output of the same length as periodFormat itself.
            // So "yyyy-MM-dd" is okay (as 'yyyy' is always 4, 'MM' and 'dd' are always 2)
            // But "yyyyy MMMM h" isn't, because 'MMMM' could be "May" or "August" and 'h' could be "1" or "12".

            // It isn't necessary to validate `periodFormat` if it contains any variable-length DateTime specifiers as all possible format-strings are internal constants in this assembly.

            // e.g. "^fileNamePrefix(?<period>\d{8})(?<sequence>_([0-9]){3,}){0,1}fileNameSuffix$" would match "filename20190222_001fileNameSuffix"
            var pattern = "^" + Regex.Escape(str: filePathPrefix) + "(?<" + PeriodMatchGroup + ">\\d{" + periodFormat.Length + "})" + "(?<" + SequenceNumberMatchGroup + ">_[0-9]{3,}){0,1}"
                        + Regex.Escape(str: filePathSuffix) + "$";

            fileNameRegex = new Regex(pattern: pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase); // IgnoreCase to ensure that incorrect casing in configuration will still work.

            filePathFormat = "{0}{1:" + periodFormat + "}{2:'_'000}{3}"; // e.g. "{0}{1:yyyyMMdd}{2:'_'000}{3}" to render as "C:\logs\File20190222_001.log".

            string inDirectoryPrefix;
            {
                var c = filePathPrefix[filePathPrefix.Length - 1];
                if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                    inDirectoryPrefix = "";
                else
                {
                    var lastDirIdx = filePathPrefix.LastIndexOfAny(new[] {'\\', '/'}, filePathPrefix.Length - 2);
                    if (lastDirIdx == -1)
                        throw new ArgumentException(message: "Cannot determine file-name characters from directory-path characters.", nameof(logFilePathTemplate)); // This should never happen, btw.

                    inDirectoryPrefix = filePathPrefix.Substring(lastDirIdx + 1);
                }
            }

            DirectorySearchPattern = inDirectoryPrefix + "*" + filePathSuffix;
        }

        public RollingInterval Interval { get; }

        public bool SupportsSubdirectories => false;

        public string DirectorySearchPattern { get; }

        public string GetRollingLogFilePath(DateTime instant, int? sequenceNumber)
        {
            var periodStart = Interval.GetCurrentCheckpoint(instant: instant);

            return string.Format(provider: CultureInfo.InvariantCulture, format: filePathFormat, filePathPrefix, periodStart, sequenceNumber, filePathSuffix);
        }

        public bool MatchRollingLogFilePath(FileInfo file, out DateTime? periodStart, out int? sequenceNumber, out string pathWithouSequence)
        {
            periodStart        = null;
            sequenceNumber     = null;
            pathWithouSequence = file.FullName;

            var match = fileNameRegex.Match(input: file.FullName);
            if (!match.Success)
                return false;

            var sequenceGrp = match.Groups[groupname: SequenceNumberMatchGroup];
            if (sequenceGrp.Success)
            {
                var sequenceText = sequenceGrp.Captures[0].Value.Substring(1);

                if (int.TryParse(s: sequenceText, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, out var sequenceNumberValue))
                    sequenceNumber = sequenceNumberValue;
                else
                    return false; // The regex accepts 3 or more consecutive digits with no upper-bound, so a string like "_99999999999999999" would match the regex but fail in `Int32.TryParse`.

                pathWithouSequence = file.FullName.Replace($"_{sequenceText}", newValue: "");
            }

            if (Interval == RollingInterval.Infinite)
                return true;

            var periodGrp = match.Groups[groupname: PeriodMatchGroup];
            if (periodGrp.Success)
            {
                var periodText = periodGrp.Captures[0].Value;

                if (DateTime.TryParseExact(s: periodText, format: periodFormat, provider: CultureInfo.InvariantCulture, style: DateTimeStyles.None, out var periodStartValue))
                    periodStart = periodStartValue;
                else
                    return false; // This could happen if the file-name matches the regex but isn't a valid DateTime, e.g. "12349940" (for 1234-99-40)
            }

            // A file-name can match the regex and lack both a DateTime value and a Sequence number and still be valid and match, e.g. an Infinite rolling-period with the path specified as "C:\logs\mylog.log".
            return true;
        }
    }
}
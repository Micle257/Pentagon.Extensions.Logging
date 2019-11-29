// -----------------------------------------------------------------------
//  <copyright file="FormattedRollingFilePathProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary> Implements <see cref="IRollingFilePathProvider" /> around a Custom .NET DateTime format string which should (but is not required to) contain the file-name extension as an enquoted literal. Any sequence numbers are inserted before the file-name extension with a leading underscore '_' character. </summary>
    class FormattedRollingFilePathProvider : IRollingFilePathProvider
    {
        static readonly Regex
                _sequenceSuffixRegex =
                        new Regex(pattern: @"_([0-9]{3,})$",
                                  options: RegexOptions
                                         .Compiled); // Matches "_000", "_001", "_999", "_1000", "_999999", but not "_", "_0", "_a", "_99", etc. Requiring 3 digits avoids matching "_dd", "_mm" in a file-name.

        readonly string logDirectory;
        readonly string filePathFormat;

        public FormattedRollingFilePathProvider(string logDirectoryAbsolutePath, RollingInterval interval, string filePathFormat)
        {
            Interval = interval;

            if (string.IsNullOrWhiteSpace(value: logDirectoryAbsolutePath))
                throw new ArgumentNullException(nameof(logDirectoryAbsolutePath));
            if (!Path.IsPathRooted(path: logDirectoryAbsolutePath))
                throw new ArgumentException(message: "Path must be absolute.", nameof(logDirectoryAbsolutePath));

            logDirectory        = PathUtility.EnsureTrailingDirectorySeparator(directoryPath: logDirectoryAbsolutePath);
            this.filePathFormat = filePathFormat ?? throw new ArgumentNullException(nameof(filePathFormat));

            // Test the format before using it:
            // Also use the rendered string to get any prefix and file-name extensions for generating a glob pattern.
            ValidateFilePathFormat(interval: interval, filePathFormat: filePathFormat, out var exampleFormatted);

            DirectorySearchPattern = CreateDirectorySearchPattern(formatted: exampleFormatted);
        }

        public RollingInterval Interval { get; }

        public bool SupportsSubdirectories => true;

        public string DirectorySearchPattern { get; }

        public string GetRollingLogFilePath(DateTime instant, int? sequenceNumber)
        {
            // Get period-start for the given point-in-time instant based on the interval:
            // e.g. if `instant == 2019-02-22 23:59:59` and `interval == Month`, then use `2019-02-01 00:00:00`.
            // This is to ensure that if the format string "yyyy-MM-dd HH:mm'.log" is used with a Monthly interval, for example, the dd, HH, and mm components will be normalized.

            string filePath;
            var periodStart = Interval.GetCurrentCheckpoint(instant: instant)
                           ?? DateTime.MinValue; // NOTE: `GetCurrentCheckpoint == null` when Infinite is used with a max file-size limit. While it would be nice to assume the file name format does not contain any DateTime format specifiers, use DateTime.MinValue just in case.

            var year  = periodStart.Year;
            var month = periodStart.Month;
            var day   = periodStart.Day;

            filePath = filePathFormat.Replace(oldValue: "yyyy", year.ToString())
                                     .Replace(oldValue: "MM", month.ToString())
                                     .Replace(oldValue: "dd", day.ToString());

            if (sequenceNumber != null)
            {
                // Insert the sequence number immediately before the extension.
                filePath = PathUtility.GetFilePathWithoutExtension(filePath: filePath) + "_" + sequenceNumber.Value.ToString(format: "000", provider: CultureInfo.InvariantCulture)
                         + Path.GetExtension(path: filePath);
            }

            return Path.Combine(path1: logDirectory, path2: filePath);
        }

        public bool MatchRollingLogFilePath(FileInfo file, out DateTime? periodStart, out int? sequenceNumber, out string pathWithoutSequence)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            // Remove the logDirectory prefix:
            if (!file.FullName.StartsWith(value: logDirectory, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                periodStart         = null;
                sequenceNumber      = null;
                pathWithoutSequence = null;
                return false;
            }

            var logDirectoryRelativeFilePath = file.FullName.Substring(startIndex: logDirectory.Length); // `this.logDirectory` always has a trailing slash.

            // Don't use `Path.GetFileNameWithoutExtension( fileName );`, we want something like `Path.GetFullPathToFileWithoutExtension( fileName );`
            var pathWithoutExtension = PathUtility.GetFilePathWithoutExtension(filePath: logDirectoryRelativeFilePath);

            // If there is a sequence suffix, trim it so that `DateTime::TryParseExact` will still work:
            GetSequenceNumber(pathWithoutExtension: pathWithoutExtension, ext: file.Extension, pathWithoutSequenceSuffix: out pathWithoutSequence, sequenceNumber: out sequenceNumber);

            try
            {
                var lastPath   = pathWithoutSequence.Split('\\', '/').LastOrDefault();
                var lastFormat = filePathFormat.Split('\\', '/').LastOrDefault();

                var indexStart = lastFormat.IndexOf(value: "yyyy");
                var indexEnd   = lastFormat.IndexOf(value: ".");

                var date = lastPath.Substring(startIndex: indexStart, indexEnd - indexStart);

                if (DateTime.TryParse(s: date, out var periodStartValue))
                {
                    periodStart = periodStartValue;
                    return true;
                }
            }
            catch (Exception e) { }

            periodStart = null;

            return false;
        }

        static void ValidateFilePathFormat(RollingInterval interval, string filePathFormat, out string exampleFormatted)
        {
            const string DefaultMessage = "The rolling file name format is invalid. ";

            // Using `DateTime.MaxValue` to get an example formatted value. This is better than `DateTime.Now` because it's constant and deterministic, and because all the components are at their maximum (e.g. Hour == 23) it means it tests for the improper use of 'h' (instead of 'hh') and if 'tt' is used, for example.

            var      pointInTime = interval.GetCurrentCheckpoint(instant: DateTime.MaxValue) ?? DateTime.MinValue;
            DateTime parsed;
            string   formatted;

            try
            {
                formatted = pointInTime.ToString(format: filePathFormat, provider: CultureInfo.InvariantCulture);
                parsed    = DateTime.ParseExact(s: formatted, format: filePathFormat, provider: CultureInfo.InvariantCulture);
            }
            catch (ArgumentException argEx)
            {
                throw new ArgumentException(DefaultMessage + "See the inner ArgumentException for details.", innerException: argEx);
            }
            catch (FormatException formatEx)
            {
                throw new ArgumentException(DefaultMessage + "See the inner FormatException for details.", innerException: formatEx);
            }

            if (parsed != pointInTime)
            {
                throw new ArgumentException(DefaultMessage
                                          + "The format does not round-trip DateTime values correctly. Does the file path format have sufficient specifiers for the selected interval? (e.g. did you specify "
                                          + nameof(RollingInterval) + "." + nameof(RollingInterval.Hour) + " but forget to include an 'HH' specifier in the file path format?)");
            }

            // Also do an early check for invalid file-name characters, e.g. ':'. Note that '/' and '\' are allowed - though if a user on Linux uses "'Log'-yyyy/MM/dd/'.log'" as a format string it might not be the effect they want...
            if (formatted.IndexOfAny(Path.GetInvalidPathChars()) > -1 || formatted.IndexOf(':') >= 2) // ':' isn't included in `Path.GetInvalidPathChars()` on Windows for some reason.
                throw new ArgumentException(DefaultMessage + "The format generates file-names that contain illegal characters, such as ':' or '/'.");

            exampleFormatted = formatted;
        }

        static string CreateDirectorySearchPattern(string formatted)
        {
            // If the generated file-name extension does not contain any digits then we can assume it's a static textual extension.
            // This will break if the file-name extension contains some alphabetic DateTime format specifier, of course.

            string globPrefix = null;
            var    globSuffix = Path.GetExtension(path: formatted);

            {
                // NOTE: This breaks if there are no file-name extensions and the user is using dots to separate-out file extensions, erk!
                var exampleFileName =
                        Path.GetFileNameWithoutExtension(path: formatted); // Remove any formatted directory names that are applied before the filename format, e.g. `logDirectoryAbsolutePath + "yyyy-MM'\Log: 'yyyy-MM-dd HH-mm'.log'` --> "Log: 2019-02-22 23:59.log"

                var firstNonLetterCharIdx = -1;
                for (var i = 0; i < exampleFileName.Length; i++)
                {
                    if (!(char.IsLetter(exampleFileName[index: i]) || char.IsWhiteSpace(exampleFileName[index: i]) || exampleFileName[index: i] == '-' || exampleFileName[index: i] == '_'))
                    {
                        firstNonLetterCharIdx = i;
                        break;
                    }
                }

                if (firstNonLetterCharIdx > -1 && firstNonLetterCharIdx < exampleFileName.Length - 1)
                    globPrefix = formatted.Substring(0, length: firstNonLetterCharIdx);
            }

            return globPrefix + "*" + globSuffix;
        }

        void GetSequenceNumber(string pathWithoutExtension, string ext, out string pathWithoutSequenceSuffix, out int? sequenceNumber)
        {
            // e.g. If fileNameFormat is "yyyy-MM\'Errors-'yyyy-MM-dd'.log'" then a possible file-name is "C:\logfiles\2019-02\Errors-2019-02-22_001.log", note the "_001" sequence-number inserted right before the extension.
            var sequenceSuffixPatternMatch = _sequenceSuffixRegex.Match(input: pathWithoutExtension); // The _sequenceSuffixRegex pattern has the '$' anchor so it will only match suffixes.
            if (sequenceSuffixPatternMatch.Success && sequenceSuffixPatternMatch.Groups.Count == 2)
            {
                var wholeMatch     = sequenceSuffixPatternMatch.Groups[0].Value;
                var sequenceDigits = sequenceSuffixPatternMatch.Groups[1].Value;

                sequenceNumber = int.Parse(s: sequenceDigits, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture);

                pathWithoutSequenceSuffix = pathWithoutExtension.Substring(0, pathWithoutExtension.Length - wholeMatch.Length) + ext;

                pathWithoutSequenceSuffix = Path.Combine(path1: logDirectory, path2: pathWithoutSequenceSuffix);
            }
            else
            {
                sequenceNumber = null;

                pathWithoutSequenceSuffix = pathWithoutExtension + ext;

                pathWithoutSequenceSuffix = Path.Combine(path1: logDirectory, path2: pathWithoutSequenceSuffix);
            }
        }
    }
}
namespace Pentagon.Extensions.Logging.File {
    using Microsoft.Extensions.Logging;

    public class FileLoggerOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information; // TODO make it work with general options
        
        public string GroupDirectoryTreeFormat { get; set; } 

        public int MaxFileSize { get; set; } = 1310720;

        public string LogDirectory { get; set; }

        public bool IncludeTimeStamp { get; set; } = true;

        public bool IncludeSource { get; set; } = true;

        public bool IncludeCategory { get; set; } = true;

        public int LogFileWriteInterval { get; set; } = 1000;

        public string IndentMultiLinesFormat { get; set; } = "  >";
    }
}
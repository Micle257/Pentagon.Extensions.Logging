// -----------------------------------------------------------------------
//  <copyright file="RollingLogFile.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System;
    using System.IO;

    class RollingLogFile
    {
        public RollingLogFile(FileInfo file, DateTime? dateTime, int? sequenceNumber)
        {
            File           = file;
            DateTime       = dateTime;
            SequenceNumber = sequenceNumber;
        }

        public RollingLogFile(FileInfo file, DateTime? periodStart, int? sequenceNumber, string pathWithoutSequence)
        {
            File                = file;
            DateTime            = periodStart;
            SequenceNumber      = sequenceNumber;
            PathWithoutSequence = pathWithoutSequence;
        }

        public string PathWithoutSequence { get; }

        public FileInfo File { get; }

        public DateTime? DateTime { get; }

        public int? SequenceNumber { get; }
    }
}
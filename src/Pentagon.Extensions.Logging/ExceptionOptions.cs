// -----------------------------------------------------------------------
//  <copyright file="ExceptionOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    public struct ExceptionOptions
    {
        public static readonly ExceptionOptions Default = new ExceptionOptions
                                                          {
                                                                  CurrentIndentLevel = 0,
                                                                  IndentSpaces = 4,
                                                                  OmitNullProperties = true
                                                          };

        internal ExceptionOptions(ExceptionOptions options, int currentIndent)
        {
            CurrentIndentLevel = currentIndent;
            IndentSpaces = options.IndentSpaces;
            OmitNullProperties = options.OmitNullProperties;
        }

        public int IndentSpaces { get; set; }

        public bool OmitNullProperties { get; set; }

        internal string Indent => new string(' ', IndentSpaces * CurrentIndentLevel);

        internal int CurrentIndentLevel { get; set; }
    }
}
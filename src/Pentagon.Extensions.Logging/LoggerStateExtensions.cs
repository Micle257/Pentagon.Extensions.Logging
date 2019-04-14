// -----------------------------------------------------------------------
//  <copyright file="LoggerStateExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging
{
    using System;
    using System.Collections.Generic;

    public static class LoggerStateExtensions
    {
        public static IEnumerable<object> GetRawState(this LoggerState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            yield return state.MethodName;
            yield return state.FilePath;
            yield return state.LineNumber;
            yield return state.Message;
        }
    }
}
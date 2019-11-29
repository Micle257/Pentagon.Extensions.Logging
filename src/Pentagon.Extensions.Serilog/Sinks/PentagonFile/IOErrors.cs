// -----------------------------------------------------------------------
//  <copyright file="IOErrors.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System.IO;

    static class IOErrors
    {
#pragma warning disable CA1801 // Review unused parameters
        public static bool IsLockedFile(IOException ex)
#pragma warning restore CA1801 // Review unused parameters
        {
#if HRESULTS
            var errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
#else
            return true;
#endif
        }
    }
}
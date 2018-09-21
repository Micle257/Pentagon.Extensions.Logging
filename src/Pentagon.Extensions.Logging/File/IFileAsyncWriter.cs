// -----------------------------------------------------------------------
//  <copyright file="IFileAsyncWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.File
{
    using System;

    public interface IFileAsyncWriter : IDisposable
    {
        void AddMessage(DateTimeOffset time, string message);
    }
}
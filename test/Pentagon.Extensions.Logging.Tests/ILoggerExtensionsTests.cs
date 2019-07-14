// -----------------------------------------------------------------------
//  <copyright file="ILoggerExtensionsTests.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.Tests
{
    using System;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ILoggerExtensionsTests
    {
        [Fact]
        public void InScope_FirstArgumentIsNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ILoggerExtensions.InScope(null, Array.Empty<(string key, object value)>()));
        }

        [Fact]
        public void InScope_SecondArgumentIsNotSpecified_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Mock.Of<ILogger>().InScope());
        }

        [Fact]
        public void InScope_SecondArgumentIsNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Mock.Of<ILogger>().InScope(null));
        }
    }
}
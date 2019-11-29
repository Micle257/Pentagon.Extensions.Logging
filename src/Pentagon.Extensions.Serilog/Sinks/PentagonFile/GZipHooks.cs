// -----------------------------------------------------------------------
//  <copyright file="GZipHooks.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <inheritdoc />
    /// <summary> Compresses log output using streaming GZip </summary>
    public class GZipHooks : FileLifecycleHooks
    {
        const int DEFAULT_BUFFER_SIZE = 32 * 1024;

        readonly CompressionLevel compressionLevel;
        readonly int bufferSize;

        public GZipHooks(CompressionLevel compressionLevel = CompressionLevel.Fastest, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            this.compressionLevel = compressionLevel;
            this.bufferSize       = bufferSize;
        }

        public override Stream OnFileOpened(Stream underlyingStream, Encoding _)
        {
            var compressStream = new GZipStream(stream: underlyingStream, compressionLevel: compressionLevel);
            return new BufferedStream(stream: compressStream, bufferSize: bufferSize);
        }
    }
}
// -----------------------------------------------------------------------
//  <copyright file="FileLifecycleHooks.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.PentagonFile
{
    using System.IO;
    using System.Text;

    /// <summary> Enables hooking into log file lifecycle events. Hooks run synchronously and therefore may affect responsiveness of the application if long operations are performed. </summary>
    public abstract class FileLifecycleHooks
    {
        /// <summary> Initialize or wrap the <paramref name="underlyingStream" /> opened on the log file. This can be used to write file headers, or wrap the stream in another that adds buffering, compression, encryption, etc. The underlying file may or may not be empty when this method is called. </summary>
        /// <remarks> A value must be returned from overrides of this method. Serilog will flush and/or dispose the returned value, but will not dispose the stream initially passed in unless it is itself returned. </remarks>
        /// <param name="underlyingStream"> The underlying <see cref="Stream" /> opened on the log file. </param>
        /// <param name="encoding"> The encoding to use when reading/writing to the stream. </param>
        /// <returns> The <see cref="Stream" /> Serilog should use when writing events to the log file. </returns>
        public virtual Stream OnFileOpened(Stream underlyingStream, Encoding encoding) => underlyingStream;

        /// <summary> Called before an obsolete (rolling) log file is deleted. This can be used to copy old logs to an archive location or send to a backup server. </summary>
        /// <param name="path"> The full path to the file being deleted. </param>
        public virtual void OnFileDeleting(string path) { }
    }
}
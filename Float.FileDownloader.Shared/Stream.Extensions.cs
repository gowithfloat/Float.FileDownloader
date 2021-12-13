using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// Extensions on the Stream class.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Copies from this stream to another while reporting progress.
        /// See <a href="https://stackoverflow.com/questions/20661652/progress-bar-with-httpclient">here</a> for more info.
        /// </summary>
        /// <returns>An awaitable task for the copy process.</returns>
        /// <param name="source">The source from which to copy. Must be readable.</param>
        /// <param name="destination">The destination to which to copy. Must be writeable. Required.</param>
        /// <param name="progress">An object to receive progress updates. Optional.</param>
        /// <param name="cancellationToken">A cancellation token for the copy process. Optional.</param>
        /// <param name="bufferSize">Buffer size. Must be greater than zero. 4096 by default.</param>
        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<long> progress = null, CancellationToken cancellationToken = default(CancellationToken), int bufferSize = 4096)
        {
            if (!source.CanRead)
            {
                throw new InvalidOperationException("Cannot copy from a stream that can't be read");
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!destination.CanWrite)
            {
                throw new InvalidOperationException("Cannot copy to a stream that can't be written");
            }

            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}

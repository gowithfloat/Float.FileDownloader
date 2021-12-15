using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// Extensions on the HttpClient class.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Asynchronously downloads content while optionally reporting progress.
        /// </summary>
        /// <returns>An awaitable task for the download process.</returns>
        /// <param name="client">The client to use when performing the download.</param>
        /// <param name="requestUri">URI of the content to download.</param>
        /// <param name="destination">The destination to which to copy. Must be writeable. Required.</param>
        /// <param name="progress">An object to receive progress updates. Optional.</param>
        /// <param name="cancellationToken">A cancellation token for the copy process. Optional.</param>
        /// <param name="bufferSize">The size of the buffer for writing to disk. 4096 by default.</param>
        public static async Task<HttpResponseMessage> DownloadAsync(this HttpClient client, Uri requestUri, Stream destination, IProgress<IDownloadBytesProgress> progress = null, CancellationToken cancellationToken = default(CancellationToken), int bufferSize = 4096)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
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

            // start by getting headers so we know size of content
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                // get content length from headers
                var contentLength = response.Content.Headers.ContentLength;

                // create task to download response content
                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    // ignore progress reporting if there's no reporter or length
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return response;
                    }

                    // convert absolute progress to bytes progress
                    var relativeProgress = new Progress<long>(totalBytes =>
                    {
                        progress.Report(new DownloadBytesProgress(requestUri, totalBytes, contentLength.Value));
                    });

                    // use extension to report progress and download
                    await download.CopyToAsync(destination, relativeProgress, cancellationToken, bufferSize);

                    // report completion when done
                    progress.Report(new DownloadBytesProgress(requestUri, contentLength.Value, contentLength.Value));

                    return response;
                }
            }
        }
    }
}

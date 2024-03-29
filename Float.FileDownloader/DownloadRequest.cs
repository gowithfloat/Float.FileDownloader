using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// A convenience wrapper around HttpClient to download the content of an HTTP response directly to a file.
    /// </summary>
    public static class DownloadRequest
    {
        /// <summary>
        /// Gets or sets the default message handler to use when downloading a request.
        /// </summary>
        /// <value>An HttpMessageHandler to use for each download.</value>
        public static HttpMessageHandler DownloadMessageHandler { get; set; }

        /// <summary>
        /// Downloads the response body to the specified file.
        /// </summary>
        /// <returns>HTTP response (e.g. headers).</returns>
        /// <param name="request">The HTTP request; all authentication and headers should be set prior to downloading.</param>
        /// <param name="destination">The absolute path of the location to store the downloaded file.</param>
        /// <param name="progressReporter">Optionally, a progress reporter can be provided to receive updated on download status.</param>
        /// <param name="cancellationTokenSource">Optionally, a cancellation token can be specified to allow the caller to cancel the download.</param>
        /// <param name="bufferSize">The size of the buffer for writing to disk. 4096 by default.</param>
        public static async Task<HttpResponseMessage> Download(HttpRequestMessage request, string destination, IProgress<IDownloadBytesProgress> progressReporter = null, CancellationTokenSource cancellationTokenSource = null, int bufferSize = 4096)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException(nameof(destination));
            }

            var messageHandler = DownloadMessageHandler ?? new HttpClientHandler();
            var shouldDisposeHandler = DownloadMessageHandler == null;

            using (var client = new HttpClient(messageHandler, shouldDisposeHandler))
            {
                if (cancellationTokenSource == null)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                }

                foreach (var header in request.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                using (var writeStream = File.OpenWrite(destination))
                {
                    return await client.DownloadAsync(request.RequestUri, writeStream, progressReporter, cancellationTokenSource.Token, bufferSize);
                }
            }
        }
    }
}

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
        /// Downloads the response body to the specified file.
        /// </summary>
        /// <returns>HTTP response (e.g. headers).</returns>
        /// <param name="request">The HTTP request; all authentication and headers should be set prior to downloading.</param>
        /// <param name="destination">The absolute path of the location to store the downloaded file.</param>
        /// <param name="progressReporter">Optionally, a progress reporter can be provided to receive updated on download status.</param>
        /// <param name="cancellationTokenSource">Optionally, a cancellation token can be specified to allow the caller to cancel the download.</param>
        /// <param name="bufferSize">The size of the buffer for writing to disk. 4096 by default.</param>
        /// <param name="messageHandler">Optionally, the message handler.</param>
        public static async Task<HttpResponseMessage> Download(HttpRequestMessage request, string destination, IProgress<IDownloadBytesProgress> progressReporter = null, CancellationTokenSource cancellationTokenSource = null, int bufferSize = 4096, HttpMessageHandler messageHandler = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException(nameof(destination));
            }

            if (messageHandler == null)
            {
                messageHandler = new HttpClientHandler();
            }

            using (var client = new HttpClient(messageHandler))
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

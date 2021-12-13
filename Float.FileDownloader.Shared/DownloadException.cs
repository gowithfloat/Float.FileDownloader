using System;
using System.Net.Http;

namespace Float.FileDownloader
{
    /// <summary>
    /// Exceptions thrown during a download.
    /// </summary>
    public class DownloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="response">The HTTP response.</param>
        internal DownloadException(string message, HttpResponseMessage response) : base(message)
        {
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadException"/> class.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="innerException">The underlying exception that caused this.</param>
        internal DownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the HTTP response which may contain more information about the reason for the failure.
        /// </summary>
        /// <value>The HTTP response.</value>
        public HttpResponseMessage Response { get; }
    }
}

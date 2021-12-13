using System.Net.Http;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// Provides the instructions required for underlying libraries to retrieve
    /// a file from a remote location (e.g. a server).
    /// </summary>
    public interface IRemoteFileProvider
    {
        /// <summary>
        /// Create an HTTP request for downloading the specified file.
        /// The HTTP request should contain all information required for retriving the file,
        /// including any request authentication (e.g. Authorization headers).
        /// </summary>
        /// <remarks>
        /// Consider also specifying values for the HTTP caching headers so that files
        /// are not needlessly redownloaded when they haven't changed.
        /// </remarks>
        /// <returns>The request to download a file.</returns>
        /// <param name="file">The file for which to build an HTTP request.</param>
        Task<HttpRequestMessage> BuildRequestToDownloadFile(IRemoteFile file);
    }
}

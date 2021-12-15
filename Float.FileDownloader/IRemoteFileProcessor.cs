using System.Net.Http;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// Defines an object responsible for manipulating and processing a
    /// file immediately after it has been downloaded.
    /// This could be used to unzip files, resize images, etc.
    /// The implementer is responsible for saving the results of the file processing
    /// either on top of the original file or in a new location.
    /// </summary>
    public interface IRemoteFileProcessor
    {
        /// <summary>
        /// Process the file at the specified path.
        /// </summary>
        /// <param name="file">The file that just finished downloading.</param>
        /// <param name="downloadPath">Absolute path to downloaded content.</param>
        /// <param name="response">HTTP response received when the file was requested.</param>
        /// <returns>Reference to the task for asynchronous work.</returns>
        Task ProcessDownload(IRemoteFile file, string downloadPath, HttpResponseMessage response);
    }
}

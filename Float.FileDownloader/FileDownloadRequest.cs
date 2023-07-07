using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Float.FileDownloader
{
    /// <summary>
    /// A convenience class for downloading a remote file to the local file system.
    /// Optionally, callers may also specify a processor to manipulate the file after it has been downloaded
    /// (e.g. unzipping files or resizing images).
    /// </summary>
    public static class FileDownloadRequest
    {
        /// <summary>
        /// Downloads a remote file to the local file system.
        /// The caller may optionally provide a DownloadStatus object to recieve updates about the progress of the download.
        /// Additionally, callers may also specify an IFileProcessor object to process or manipulate the file after it has been downloaded.
        /// The file processor will have an opportunity to run before the download reports as finished.
        /// </summary>
        /// <remarks>
        /// The file processor is responsible for determining how to store the results of the processed download.
        /// Additionally, it is the responsible of the caller to handing caching headers as part of the request.
        /// It is recommended that the caller observe the response headers returned after a download completes to
        /// use that information (e.g. the ETag) to use on future requests.
        /// </remarks>
        /// <returns>The HTTP response from requesting the file.</returns>
        /// <param name="fileProvider">The IFileProvider object that can build an HTTP request for the specified file.</param>
        /// <param name="file">The file to download.</param>
        /// <param name="downloadDestination">Absolute URI of the location to save the downloaded file.</param>
        /// <param name="status">Optionally, receive status updates about the download progress of the file.</param>
        /// <param name="fileProcessor">Optionally, specify an IFileProcessor to process or manipulate the file after it downloads.</param>
        /// <param name="messageHandler">Optionally, the message handler.</param>
        public static async Task<HttpResponseMessage> DownloadFile(IRemoteFileProvider fileProvider, IRemoteFile file, Uri downloadDestination, DownloadStatus status = null, IRemoteFileProcessor fileProcessor = null, HttpMessageHandler messageHandler = null)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (downloadDestination == null)
            {
                throw new ArgumentNullException(nameof(downloadDestination));
            }

            // Validate that the download destination is reasonable
            if (!downloadDestination.IsFile || !downloadDestination.IsAbsoluteUri)
            {
                throw new ArgumentException("downloadDestination must refer to a valid file");
            }

            var downloadPath = downloadDestination.OriginalString;
            var downloadDirectory = Path.GetDirectoryName(downloadPath);
            var temporaryDownloadPath = Path.Combine(downloadDirectory, Path.GetRandomFileName());
            CreateDirectoryUnlessExists(downloadDirectory);

            IProgress<IDownloadBytesProgress> progressReporter = null;
            CancellationTokenSource token = null;

            if (status != null)
            {
                progressReporter = status.AddProgressReporter<IDownloadBytesProgress>();
                token = status.CancellationTokenSource;
            }

            var request = await fileProvider.BuildRequestToDownloadFile(file);

            if (request == null)
            {
                throw new ArgumentException($"{fileProvider} returned a null request for {file}");
            }

            HttpResponseMessage response;

            try
            {
                response = await DownloadRequest.Download(request, temporaryDownloadPath, progressReporter, token, messageHandler: messageHandler);
            }
            catch (Exception e)
            {
                progressReporter?.Report(DownloadBytesProgress.Failed(file.Url));

                string message;

                switch (e)
                {
                    case IOException _:
                        message = FileDownloaderStrings.FileDownloadFailedFullHddString;
                        break;
                    case OperationCanceledException _:
                        message = e.Message;
                        break;
                    default:
                        message = FileDownloaderStrings.FileDownloadFailedString;
                        break;
                }

                var exception = new DownloadException(message, e);
                status?.OnProcessingComplete(exception);
                File.Delete(temporaryDownloadPath);
                throw exception;
            }

            // if the remote content was not modified, clean up and return the response
            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                status?.OnProcessingComplete();
                File.Delete(temporaryDownloadPath);
                return response;
            }

            // if there was an error, throw an exception
            // note that we check for "not modified" first because that is not a "success" status code
            if (!response.IsSuccessStatusCode)
            {
                var exception = new DownloadException(response.ReasonPhrase, response);
                status?.OnProcessingComplete(exception);
                File.Delete(temporaryDownloadPath);
                throw exception;
            }

            string fileName;

            // if we were given a local file path, move the temporary file there
            // but if its a local directory, use the file URL to get a permanent file name
            if (string.IsNullOrWhiteSpace(Path.GetExtension(downloadDestination.AbsolutePath)))
            {
                fileName = file.Url.AbsoluteUri;
            }
            else
            {
                fileName = downloadDestination.AbsolutePath;
            }

            var destinationPath = Path.Combine(downloadDirectory, Uri.UnescapeDataString(Path.GetFileName(fileName)));

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(temporaryDownloadPath, destinationPath);

            try
            {
                if (fileProcessor != null)
                {
                    await fileProcessor.ProcessDownload(file, downloadPath, response);
                }

                status?.OnProcessingComplete();
            }
            catch (Exception e)
            {
                status?.OnProcessingComplete(e);
                throw new DownloadException("Processing download failed", e);
            }

            return response;
        }

        static void CreateDirectoryUnlessExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
        }
    }
}

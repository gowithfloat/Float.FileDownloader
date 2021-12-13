using System;

namespace Float.FileDownloader
{
    /// <summary>
    /// Interface for objects that store download progress information.
    /// </summary>
    public interface IDownloadBytesProgress : IPercentProgress
    {
        /// <summary>
        /// Gets the total number of bytes expected to receive for the download.
        /// </summary>
        /// <value>The total number of expected bytes.</value>
        long TotalBytes { get; }

        /// <summary>
        /// Gets the number of bytes received so far by the download.
        /// </summary>
        /// <value>The number of bytes received so far.</value>
        long BytesReceived { get; }

        /// <summary>
        /// Gets the file URL.
        /// </summary>
        /// <value>The file URL.</value>
        Uri FileUrl { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:LearningLibrary.DownloadBytesProgress"/> is finished.
        /// </summary>
        /// <value>True if this is finished, false otherwise.</value>
        bool IsFinished { get; }
    }
}

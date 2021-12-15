using System;

namespace Float.FileDownloader
{
    /// <summary>
    /// Represents the progress of a download.
    /// </summary>
    struct DownloadBytesProgress : IDownloadBytesProgress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadBytesProgress"/> struct.
        /// </summary>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="bytesReceived">Bytes received.</param>
        /// <param name="totalBytes">Total expected bytes to download.</param>
        public DownloadBytesProgress(Uri fileUrl, long bytesReceived, long totalBytes)
        {
            if (fileUrl == null)
            {
                throw new ArgumentNullException(nameof(fileUrl));
            }

            if (bytesReceived < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesReceived));
            }

            if (totalBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalBytes));
            }

            if (bytesReceived > totalBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesReceived));
            }

            FileUrl = fileUrl;
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }

        /// <inheritdoc />
        public long TotalBytes { get; }

        /// <inheritdoc />
        public long BytesReceived { get; }

        /// <inheritdoc />
        public Uri FileUrl { get; }

        /// <inheritdoc />
        public bool IsFinished => BytesReceived == TotalBytes;

        /// <inheritdoc />
        public double PercentComplete => TotalBytes == 0 ? 1 : (double)BytesReceived / TotalBytes;

        /// <summary>
        /// Convenience method to create an object representing a failed progress with no bytes received or progress.
        /// </summary>
        /// <returns>The failed progress object.</returns>
        /// <param name="fileUrl">File URL associated with the failed download.</param>
        public static IDownloadBytesProgress Failed(Uri fileUrl)
        {
            return new DownloadBytesProgress(fileUrl, 0, 0);
        }
    }
}

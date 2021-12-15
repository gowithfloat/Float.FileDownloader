using System;

namespace Float.FileDownloader
{
    /// <summary>
    /// A remote file to download locally.
    /// </summary>
    public interface IRemoteFile
    {
        /// <summary>
        /// Gets the URL where the file is located.
        /// This URL _may_ require authentication to access it's resource.
        /// </summary>
        /// <value>The URL where the file is located.</value>
        Uri Url { get; }
    }
}

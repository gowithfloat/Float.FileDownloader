using System;

namespace Float.FileDownloader.Tests
{
    class SimpleRemoteFile : IRemoteFile
    {
        internal SimpleRemoteFile(string url)
        {
            Url = new Uri(url);
        }

        public Uri Url { get; }
    }
}

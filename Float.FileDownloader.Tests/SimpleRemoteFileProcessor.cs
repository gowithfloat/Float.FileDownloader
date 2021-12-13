using System.Net.Http;
using System.Threading.Tasks;

namespace Float.FileDownloader.Tests
{
    public class SimpleRemoteFileProcessor : IRemoteFileProcessor
    {
        public IRemoteFile RemoteFile { get; set; }

        public string DownloadPath { get; set; }

        public HttpResponseMessage Message { get; set; }

        public async Task ProcessDownload(IRemoteFile file, string downloadPath, HttpResponseMessage response)
        {
            RemoteFile = file;
            DownloadPath = downloadPath;
            Message = response;
            await Task.Delay(1);
        }
    }
}

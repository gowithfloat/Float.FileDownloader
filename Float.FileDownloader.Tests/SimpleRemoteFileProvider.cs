using System.Net.Http;
using System.Threading.Tasks;

namespace Float.FileDownloader.Tests
{
    class SimpleRemoteFileProvider : IRemoteFileProvider
    {
        public Task<HttpRequestMessage> BuildRequestToDownloadFile(IRemoteFile file)
        {
            return Task.FromResult(new HttpRequestMessage(HttpMethod.Get, file.Url.AbsoluteUri));
        }
    }
}

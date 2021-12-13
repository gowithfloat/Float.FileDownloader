using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class DownloadRequestTests
    {
        [Test]
        public void TestInvalidDownload()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, " "));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, TempFilePath()));

            Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), null));
            Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), string.Empty));
            Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), " "));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), TempFilePath()));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await DownloadRequest.Download(new HttpRequestMessage(HttpMethod.Get, "/alsdkjf/asdflkj/aljsieo"), TempFilePath()));
        }

        [Test]
        public async Task TestValidDownload()
        {
            var request1 = new HttpRequestMessage(HttpMethod.Put, TestUriString());
            await DownloadRequest.Download(request1, TempFilePath());

            var request2 = new HttpRequestMessage(HttpMethod.Get, TestUri());
            await DownloadRequest.Download(request2, TempFilePath());

            await DownloadRequest.Download(request2, TempFilePath());
        }

        [Test]
        public async Task TestMethodTypes()
        {
            var request1 = new HttpRequestMessage(HttpMethod.Delete, TestUriString());
            var request2 = new HttpRequestMessage(HttpMethod.Get, TestUriString());
            var request3 = new HttpRequestMessage(HttpMethod.Head, TestUriString());
            var request4 = new HttpRequestMessage(HttpMethod.Options, TestUriString());
            //var request5 = new HttpRequestMessage(HttpMethod.Patch, TestUriString());
            var request6 = new HttpRequestMessage(HttpMethod.Post, TestUriString());
            var request7 = new HttpRequestMessage(HttpMethod.Put, TestUriString());
            var request8 = new HttpRequestMessage(HttpMethod.Trace, TestUriString());

            await DownloadRequest.Download(request1, TempFilePath());
            await DownloadRequest.Download(request2, TempFilePath());
            await DownloadRequest.Download(request3, TempFilePath());
            await DownloadRequest.Download(request4, TempFilePath());
            //await DownloadRequest.Download(request5, TempFilePath());
            await DownloadRequest.Download(request6, TempFilePath());
            await DownloadRequest.Download(request7, TempFilePath());
            await DownloadRequest.Download(request8, TempFilePath());
        }

        [Test]
        public async Task TestFileNotFound()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.httpbin.org/status/404");
            var response = await DownloadRequest.Download(request, TempFilePath());
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void TestServerNotFound()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.floatfloatfloat.float/float");
            Assert.ThrowsAsync<HttpRequestException>(async () => await DownloadRequest.Download(request, TempFilePath()));
        }
    }
}

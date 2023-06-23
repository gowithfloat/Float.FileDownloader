using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class DownloadRequestTests
    {
        [Fact]
        public async Task TestInvalidDownload()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, string.Empty));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, " "));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(null, TempFilePath()));

            await Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), null));
            await Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), string.Empty));
            await Assert.ThrowsAsync<ArgumentException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), " "));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await DownloadRequest.Download(new HttpRequestMessage(), TempFilePath()));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await DownloadRequest.Download(new HttpRequestMessage(HttpMethod.Get, "/alsdkjf/asdflkj/aljsieo"), TempFilePath()));
        }

        [Fact]
        public async Task TestValidDownload()
        {
            var request1 = new HttpRequestMessage(HttpMethod.Put, TestUriString());
            await DownloadRequest.Download(request1, TempFilePath());

            var request2 = new HttpRequestMessage(HttpMethod.Get, TestUri());
            await DownloadRequest.Download(request2, TempFilePath());

            await DownloadRequest.Download(request2, TempFilePath());
        }

        [Fact]
        public async Task TestMethodTypes()
        {
            var request1 = new HttpRequestMessage(HttpMethod.Delete, TestUriString());
            var request2 = new HttpRequestMessage(HttpMethod.Get, TestUriString());
            var request3 = new HttpRequestMessage(HttpMethod.Head, TestUriString());
            var request4 = new HttpRequestMessage(HttpMethod.Options, TestUriString());
            var request5 = new HttpRequestMessage(HttpMethod.Patch, TestUriString());
            var request6 = new HttpRequestMessage(HttpMethod.Post, TestUriString());
            var request7 = new HttpRequestMessage(HttpMethod.Put, TestUriString());
            var request8 = new HttpRequestMessage(HttpMethod.Trace, TestUriString());

            await DownloadRequest.Download(request1, TempFilePath());
            await DownloadRequest.Download(request2, TempFilePath());
            await DownloadRequest.Download(request3, TempFilePath());
            await DownloadRequest.Download(request4, TempFilePath());
            await DownloadRequest.Download(request5, TempFilePath());
            await DownloadRequest.Download(request6, TempFilePath());
            await DownloadRequest.Download(request7, TempFilePath());
            await DownloadRequest.Download(request8, TempFilePath());
        }

        [Fact]
        public async Task TestFileNotFound()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.httpbin.org/status/404");
            var response = await DownloadRequest.Download(request, TempFilePath());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TestServerNotFound()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.floatfloatfloat.float/float");
            await Assert.ThrowsAsync<HttpRequestException>(async () => await DownloadRequest.Download(request, TempFilePath()));
        }

        [Fact]
        public async Task TestEmptyHandler()
        {
            var filePath = TempFilePath();
            var request1 = new HttpRequestMessage(HttpMethod.Put, TestUriString());
            await DownloadRequest.Download(request1, filePath);
            var request2 = new HttpRequestMessage(HttpMethod.Get, TestUri());
            await DownloadRequest.Download(request2, filePath);
            Assert.True(System.IO.File.Exists(filePath));
        }
    }
}

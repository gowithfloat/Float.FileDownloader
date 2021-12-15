using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task TestDownloadAsyncNullUri()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DownloadAsync(null, writeStream, null, default, 2048));
            }
        }

        [Fact]
        public async Task TestDownloadAsyncNullDestination()
        {
            using (var client = new HttpClient())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DownloadAsync(TestUri(), null, null, default, 2048));
            }
        }

        [Fact]
        public async Task TestDownloadAsyncReadOnly()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenRead(TempFilePath()))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.DownloadAsync(TestUri(), writeStream, null, default, 2048));
            }
        }

        [Fact]
        public async Task TestDownloadAsyncInvalidBuffer()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.DownloadAsync(TestUri(), writeStream, null, default, 0));
            }
        }

        [Fact]
        public async Task TestDownloadAsync()
        {
            var progress = new Progress<IDownloadBytesProgress>(totalBytes => { });

            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await client.DownloadAsync(TestUri(), writeStream, progress, default, 2048);
            }
        }

        [Fact]
        public async Task TestDownloadAsyncWithoutProgress()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await client.DownloadAsync(TestUri(), writeStream, null, default, 2048);
            }
        }
    }
}

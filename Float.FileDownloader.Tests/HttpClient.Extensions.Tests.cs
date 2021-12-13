using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class HttpClientExtensionsTests
    {
        [Test]
        public void TestDownloadAsyncNullUri()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DownloadAsync(null, writeStream, null, default, 2048));
            }
        }

        [Test]
        public void TestDownloadAsyncNullDestination()
        {
            using (var client = new HttpClient())
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DownloadAsync(TestUri(), null, null, default, 2048));
            }
        }

        [Test]
        public void TestDownloadAsyncReadOnly()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenRead(TempFilePath()))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await client.DownloadAsync(TestUri(), writeStream, null, default, 2048));
            }
        }

        [Test]
        public void TestDownloadAsyncInvalidBuffer()
        {
            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.DownloadAsync(TestUri(), writeStream, null, default, 0));
            }
        }

        [Test]
        public async Task TestDownloadAsync()
        {
            var progress = new Progress<IDownloadBytesProgress>(totalBytes => { });

            using (var client = new HttpClient())
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await client.DownloadAsync(TestUri(), writeStream, progress, default, 2048);
            }
        }

        [Test]
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

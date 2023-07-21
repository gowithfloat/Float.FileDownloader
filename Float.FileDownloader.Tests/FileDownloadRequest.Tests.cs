using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Float.HttpServer;
using Moq;
using Moq.Protected;
using Xunit;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class FileDownloadRequestTests : IDisposable
    {
        private bool isDisposed;
        private readonly LocalHttpServer server = new LocalHttpServer("127.0.0.1", 33616);

        static class PortSelector
        {
            const ushort defaultStartPort = 61550;
            static ushort index = 0;

            public static ushort SelectForAddress(string _, ushort startPort = defaultStartPort)
            {
                index += 1;
                return (ushort)(startPort + index);
            }
        }

        SimpleRemoteFile CreateFile(string fileName = "test.html")
        {
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, fileName);
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write("Hello World");
            }

            long contentLength = 0;
            using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                contentLength = input.Length;
            }

            var port = PortSelector.SelectForAddress("127.0.0.1", 56555);
            var server = new LocalHttpServer("127.0.0.1", port);
            server.Start();
            server.SetDefaultResponder(new StaticFileResponder(directory));
            var file = new SimpleRemoteFile($"http://{server.Host}:{server.Port}/{fileName}");
            return file;
        }

        [Fact]
        public async Task TestSmallDownload()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestSmallDownloadWithProcessor()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var processor = new SimpleRemoteFileProcessor();
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination, null, processor);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(destination.AbsolutePath, processor.DownloadPath);
            Assert.Equal(file, processor.RemoteFile);
            Assert.Equal(response, processor.Message);
        }

        [Fact]
        public async Task TestSmallDownloadWithStatus()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var status = new DownloadStatus("hi");
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination, status);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestLargeDownload()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestDownloadFileWithSpaceInName()
        {
            var fileName = "spaced file name.txt";
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile(fileName);
            var destinationString = Path.Combine(Path.GetTempPath(), fileName);
            var destinationUri = new Uri(destinationString);

            if (File.Exists(destinationString))
            {
                File.Delete(destinationString);
            }

            Assert.False(File.Exists(destinationString));

            var response = await FileDownloadRequest.DownloadFile(provider, file, destinationUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(destinationString));
        }

        [Fact]
        public async Task TestDownloadFileWithSpaceInNameWithoutFileUri()
        {
            var fileName = "spaced file name.txt";
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile(fileName);
            var destination = new Uri(Path.GetTempPath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), fileName)));
        }

        [Fact]
        public async Task TestDownloadAdjacentFiles()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var folder = Path.Combine(Path.GetTempPath(), "adjacent");

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Assert.False(Directory.Exists(folder));

            var destination1 = new Uri(Path.Combine(folder, "file1.png"));
            var destination2 = new Uri(Path.Combine(folder, "file2.png"));
            _ = await FileDownloadRequest.DownloadFile(provider, file, destination1);
            Assert.True(Directory.Exists(folder));
            Assert.True(File.Exists(destination1.AbsolutePath));
            Assert.False(File.Exists(destination2.AbsolutePath));

            _ = await FileDownloadRequest.DownloadFile(provider, file, destination2);
            Assert.True(Directory.Exists(folder));
            Assert.True(File.Exists(destination1.AbsolutePath));
            Assert.True(File.Exists(destination2.AbsolutePath));
        }

        [Fact]
        public async Task TestCustomHandler()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var mock = new Mock<HttpClientHandler>();
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage()));
            await FileDownloadRequest.DownloadFile(provider, file, destination, messageHandler: mock.Object);
            mock.Protected().Verify("SendAsync", Times.AtLeastOnce(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                server.Dispose();
            }

            isDisposed = true;
        }

        ~FileDownloadRequestTests()
        {
            Dispose(false);
        }
    }
}

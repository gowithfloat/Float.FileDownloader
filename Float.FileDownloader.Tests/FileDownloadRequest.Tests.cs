using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Float.HttpServer;
using NUnit.Framework;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    [TestFixture]
    public class FileDownloadRequestTests
    {
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

        [Test]
        public async Task TestSmallDownload()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task TestSmallDownloadWithProcessor()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var processor = new SimpleRemoteFileProcessor();
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination, null, processor);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(destination.AbsolutePath, processor.DownloadPath);
            Assert.AreEqual(file, processor.RemoteFile);
            Assert.AreEqual(response, processor.Message);
        }

        [Test]
        public async Task TestSmallDownloadWithStatus()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var status = new DownloadStatus("hi");
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination, status);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task TestLargeDownload()
        {
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile();
            var destination = new Uri(TempFilePath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
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
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(destinationString));
        }

        [Test]
        public async Task TestDownloadFileWithSpaceInNameWithoutFileUri()
        {
            var fileName = "spaced file name.txt";
            var provider = new SimpleRemoteFileProvider();
            var file = CreateFile(fileName);
            var destination = new Uri(Path.GetTempPath());
            var response = await FileDownloadRequest.DownloadFile(provider, file, destination);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), fileName)));
        }

        [Test]
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
    }
}

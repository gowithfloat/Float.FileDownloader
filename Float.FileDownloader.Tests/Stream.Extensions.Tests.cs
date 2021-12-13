using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class StreamExtensionsTests
    {
        /// <summary>
        /// Verify that the current platform can handle a simple file copy.
        /// </summary>
        [Test]
        public async Task TestSanity()
        {
            var source = TempFilePath();
            var destination = TempFilePath();
            await File.WriteAllTextAsync(source, "test contents");

            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(destination))
            {
                await readStream.CopyToAsync(writeStream);
            }

            var destinationText = File.ReadAllText(destination);
            Assert.AreEqual("test contents", destinationText);
        }

        [Test]
        public void TestCopyFromWriteOnly()
        {
            using (var readStream = File.OpenWrite(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress()));
            }
        }

        [Test]
        public void TestCopyToReadOnly()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenRead(TempFilePath()))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress()));
            }
        }

        [Test]
        public void TestCopyToNull()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await readStream.CopyToAsync(null, SimpleProgress()));
            }
        }

        [Test]
        public void TestInvalidBufferSize()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress(), new CancellationToken(), -1));
            }
        }

        [Test]
        public async Task TestCopyEmptyAsync()
        {
            int progressCount = 0;
            var progress = new Progress<long>(totalBytes => progressCount++);

            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await readStream.CopyToAsync(writeStream, progress);
            }

            Assert.AreEqual(0, progressCount);
        }
    }
}

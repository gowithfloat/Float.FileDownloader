using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Float.FileDownloader.Tests.TestHelpers;

namespace Float.FileDownloader.Tests
{
    public class StreamExtensionsTests
    {
        /// <summary>
        /// Verify that the current platform can handle a simple file copy.
        /// </summary>
        [Fact]
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
            Assert.Equal("test contents", destinationText);
        }

        [Fact]
        public async Task TestCopyFromWriteOnly()
        {
            using (var readStream = File.OpenWrite(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress()));
            }
        }

        [Fact]
        public async Task TestCopyToReadOnly()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenRead(TempFilePath()))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress()));
            }
        }

        [Fact]
        public async Task TestCopyToNull()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await readStream.CopyToAsync(null, SimpleProgress()));
            }
        }

        [Fact]
        public async Task TestInvalidBufferSize()
        {
            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await readStream.CopyToAsync(writeStream, SimpleProgress(), new CancellationToken(), -1));
            }
        }

        [Fact]
        public async Task TestCopyEmptyAsync()
        {
            int progressCount = 0;
            var progress = new Progress<long>(totalBytes => progressCount++);

            using (var readStream = File.OpenRead(TempFilePath()))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await readStream.CopyToAsync(writeStream, progress);
            }

            Assert.Equal(0, progressCount);
        }

        [Fact(Skip="bokred")]
        public async Task TestCopyNonEmptyAsync()
        {
            var source = TempFilePath();
            await File.WriteAllTextAsync(source, RandomString(4096));

            int progressCount = 0;
            var progress = new Progress<long>(totalBytes => progressCount++);

            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(TempFilePath()))
            {
                await readStream.CopyToAsync(writeStream, progress, new CancellationToken(), 1024);
            }

            // this is a little flaky, not sure why
            Assert.InRange(progressCount, 1, 10);
        }
    }
}

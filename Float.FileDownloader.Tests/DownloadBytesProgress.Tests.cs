using System;
using Xunit;

namespace Float.FileDownloader.Tests
{
    public class DownloadBytesProgressTest
    {
        [Fact]
        public void TestInvalidInit()
        {
            Assert.Throws<ArgumentNullException>(() => new DownloadBytesProgress(null, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadBytesProgress(new Uri("a://b.c.d"), -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadBytesProgress(new Uri("a://b.c.d"), 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DownloadBytesProgress(new Uri("a://b.c.d"), 2, 1));
        }

        [Fact]
        public void TestDefaultInit()
        {
            var progress1 = new DownloadBytesProgress();
            Assert.Null(progress1.FileUrl);
            Assert.Equal(1, progress1.PercentComplete);
            Assert.Equal(0, progress1.BytesReceived);
            Assert.True(progress1.IsFinished);
            Assert.Equal(0, progress1.TotalBytes);
        }

        [Fact]
        public void TestValidInit()
        {
            var progress2 = new DownloadBytesProgress(new Uri("a://b.c.d"), 0, 0);
            Assert.NotNull(progress2.FileUrl);

            var progress3 = new DownloadBytesProgress(new Uri("a://b.c.d"), 1, 2);
            Assert.False(progress3.IsFinished);
            Assert.Equal(2, progress3.TotalBytes);
            Assert.Equal(1, progress3.BytesReceived);
            Assert.Equal(0.5, progress3.PercentComplete);
        }

        [Fact]
        public void TestBuiltinFailed()
        {
            var failed = DownloadBytesProgress.Failed(new Uri("a://b.c"));
            Assert.NotNull(failed.FileUrl);
            Assert.Equal(0, failed.BytesReceived);
            Assert.Equal(0, failed.TotalBytes);
        }
    }
}

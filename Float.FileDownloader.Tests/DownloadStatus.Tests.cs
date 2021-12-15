using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Float.FileDownloader.Tests
{
    public class DownloadStatusTests
    {
        [Fact]
        public void TestInit()
        {
            Assert.Throws<ArgumentException>(() => new DownloadStatus(null));
            Assert.Throws<ArgumentException>(() => new DownloadStatus(string.Empty));
            Assert.Throws<ArgumentException>(() => new DownloadStatus(" "));
        }

        [Fact]
        public async Task TestStateCancelled()
        {
            var status = new DownloadStatus("name");
            Assert.Equal(DownloadStatus.DownloadState.Waiting, status.State);

            var progress = status.AddProgressReporter<IDownloadBytesProgress>();
            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 1, 2));

            // we have to wait for events to propagate
            await Task.Delay(50);

            Assert.Equal(DownloadStatus.DownloadState.Downloading, status.State);
            Assert.Equal(0.5, status.PercentComplete);

            status.CancelDownload();
            Assert.Equal(DownloadStatus.DownloadState.Cancelled, status.State);
        }

        [Fact]
        public async Task TestStateFinished()
        {
            var status = new DownloadStatus("name");
            Assert.Equal(DownloadStatus.DownloadState.Waiting, status.State);

            var progress = status.AddProgressReporter<IDownloadBytesProgress>();
            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 1, 2));

            // we have to wait for events to propagate
            await Task.Delay(50);

            Assert.Equal(DownloadStatus.DownloadState.Downloading, status.State);
            Assert.Equal(0.5, status.PercentComplete);

            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 2, 2));

            await Task.Delay(50);

            Assert.Equal(1.0, status.PercentComplete);

            status.OnProcessingComplete();
            Assert.Equal(DownloadStatus.DownloadState.Finished, status.State);
        }

        [Fact]
        public void TestStateError()
        {
            var status = new DownloadStatus("name");
            Assert.Equal(DownloadStatus.DownloadState.Waiting, status.State);

            status.OnProcessingComplete(new Exception());
            Assert.Equal(DownloadStatus.DownloadState.Error, status.State);
        }

        [Fact]
        public async Task TestPropertyChanged()
        {
            var status = new DownloadStatus("name");
            var percentProgressChanges = 0;
            Assert.Equal(DownloadStatus.DownloadState.Waiting, status.State);

            status.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(DownloadStatus.PercentComplete))
                {
                    percentProgressChanges++;
                }
            };

            Assert.Equal(0, percentProgressChanges);

            var progress = status.AddProgressReporter<IDownloadBytesProgress>();
            await Task.Delay(50);
            Assert.Equal(0, percentProgressChanges);

            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 1, 5));
            await Task.Delay(50);
            Assert.Equal(1, percentProgressChanges);

            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 2, 5));
            await Task.Delay(50);
            Assert.Equal(2, percentProgressChanges);

            progress.Report(new DownloadBytesProgress(new Uri("a://b.c"), 201, 500));
            await Task.Delay(50);
            Assert.Equal(2, percentProgressChanges);
        }

        [Fact]
        public async Task TestProgressSum()
        {
            var status = new DownloadStatus("name");
            var progress1 = status.AddProgressReporter<IDownloadBytesProgress>();
            var progress2 = status.AddProgressReporter<IDownloadBytesProgress>();
            var progress3 = status.AddProgressReporter<IDownloadBytesProgress>();

            progress1.Report(new DownloadBytesProgress(new Uri("a://b.c"), 1000, 10000));
            progress2.Report(new DownloadBytesProgress(new Uri("a://b.d"), 3000, 10000));
            progress3.Report(new DownloadBytesProgress(new Uri("a://b.d"), 1500, 10000));
            await Task.Delay(100);

            Assert.Equal(0.18333, status.PercentComplete, 5);
        }

        [Fact]
        public async Task TestDownloadStatusConcurrentModification()
        {
            List<Task> tasks = new List<Task>();

            for (var x = 0; x < 3; x++)
            {
                var task = Task.Run(() =>
                {
                    var status = new DownloadStatus("name");

                    var progress = status.AddProgressReporter<IDownloadBytesProgress>();
                    var uri = new Uri("a://b.c");

                    var total = 100;

                    for (var i = 0; i <= total; i++)
                    {
                        progress.Report(new DownloadBytesProgress(uri, i, total));
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}

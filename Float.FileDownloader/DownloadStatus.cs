using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Float.FileDownloader
{
    /// <summary>
    /// Represents the status of one or more file downloads.
    /// </summary>
    public class DownloadStatus : INotifyPropertyChanged
    {
        /// <summary>
        /// Internal dictionary keeping track of the IPercentProgress instances
        /// for all active downloads.
        /// </summary>
        readonly IDictionary<object, IPercentProgress> progresses = new ConcurrentDictionary<object, IPercentProgress>();

        /// <summary>
        /// Internal storage for the total percent complete of all downloads.
        /// </summary>
        double percentComplete;

        /// <summary>
        /// Internal storage for the download state.
        /// </summary>
        DownloadState downloadState = DownloadState.Waiting;

        /// <summary>
        /// The last PercentComplete value sent to observers.
        /// This allows us to "rate-limit" the number of times we notify observers of updated status so that
        /// they are not overwhelmed when the percent complete changes by a hundreth.
        /// </summary>
        double previousNotifiedPercentComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadStatus"/> class.
        /// </summary>
        /// <param name="name">Display name for the download.</param>
        public DownloadStatus(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// Occurs when all downloads complete.
        /// </summary>
        public event EventHandler DownloadsCompleted;

        /// <summary>
        /// Occurs when downloads cancelled.
        /// </summary>
        public event EventHandler DownloadsCancelled;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Download state.
        /// </summary>
        public enum DownloadState
        {
            /// <summary>
            /// The download has not been started yet.
            /// </summary>
            Waiting,

            /// <summary>
            /// The download is currently in process.
            /// </summary>
            Downloading,

            /// <summary>
            /// The download is finished.
            /// </summary>
            Finished,

            /// <summary>
            /// The download experienced an error.
            /// </summary>
            Error,

            /// <summary>
            /// The download is cancelled.
            /// </summary>
            Cancelled,
        }

        /// <summary>
        /// Gets the cancellation token source.
        /// </summary>
        /// <value>The cancellation token source.</value>
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        /// <summary>
        /// Gets the display name of the download.
        /// </summary>
        /// <value>The display name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception, if one occurred.</value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the state of this download.
        /// </summary>
        /// <value>The representation of download state.</value>
        public DownloadState State
        {
            get
            {
                return downloadState;
            }

            private set
            {
                if (downloadState != value && downloadState != DownloadState.Cancelled)
                {
                    downloadState = value;
                    NotifyPropertyChanged(nameof(State));
                }
            }
        }

        /// <summary>
        /// Gets the percent complete.
        /// </summary>
        /// <value>The percent complete.</value>
        public double PercentComplete
        {
            get
            {
                return percentComplete;
            }

            private set
            {
                percentComplete = value;
                if (PropertyChanged != null && (percentComplete - previousNotifiedPercentComplete > .01 || percentComplete >= 1))
                {
                    previousNotifiedPercentComplete = percentComplete;
                    NotifyPropertyChanged(nameof(PercentComplete));
                }
            }
        }

        /// <summary>
        /// Generates a new progress reporter for attaching to a new download.
        /// DownloadStatus will observe the progress on this download and update it's own status.
        /// </summary>
        /// <returns>The progress reporter.</returns>
        /// <typeparam name="T">The type of progress being reported.</typeparam>
        public IProgress<T> AddProgressReporter<T>() where T : IPercentProgress
        {
            var progress = new Progress<T>();

            progress.ProgressChanged += (sender, e) =>
            {
                progresses[progress] = e;
                RecalculateStatus();
            };

            return progress;
        }

        /// <summary>
        /// Cancels the download.
        /// </summary>
        public void CancelDownload()
        {
            if (PercentComplete < 1 && State == DownloadState.Downloading)
            {
                percentComplete = 0;
                CancellationTokenSource.Cancel();
                State = DownloadState.Cancelled;

                DownloadsCancelled?.Invoke(this, EventArgs.Empty);

                Cleanup();
            }
        }

        /// <summary>
        /// Invoked when the processing for one of the child tasks is complete.
        /// </summary>
        /// <param name="exception">Exception, if one occurred. Null otherwise.</param>
        internal void OnProcessingComplete(Exception exception = null)
        {
            if (CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (exception != null)
            {
                Exception = exception;
                State = DownloadState.Error;
            }

            // Verify that all child processes are complete
            // The lines below are commented out because they interfered with clean up being called when there was an error. See ticket #2800
            // foreach (var progress in progresses.Values)
            // {
            //    if (progress.PercentComplete < 1)
            //    {
            //        return;
            //    }
            // }

            // If it looks like everything is complete, then this download is done!
            if (State != DownloadState.Error)
            {
                State = DownloadState.Finished;
            }

            DownloadsCompleted?.Invoke(this, EventArgs.Empty);

            Cleanup();
        }

        void Cleanup()
        {
            DownloadsCompleted = null;
            DownloadsCancelled = null;
            CancellationTokenSource.Dispose();
            progresses?.Clear();
        }

        void RecalculateStatus()
        {
            if (progresses.Values.Count == 0 || State == DownloadState.Cancelled)
            {
                return;
            }

            State = DownloadState.Downloading;
            PercentComplete = progresses.Values.TotalPercentComplete();
        }

        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

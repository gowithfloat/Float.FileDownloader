namespace Float.FileDownloader
{
    /// <summary>
    /// Reports the progress of a task as a percentage (from 0 to 1).
    /// </summary>
    public interface IPercentProgress
    {
        /// <summary>
        /// Gets the percent complete.
        /// </summary>
        /// <value>The percent complete.</value>
        double PercentComplete { get; }
    }
}

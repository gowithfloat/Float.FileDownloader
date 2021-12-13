using System.Collections.Generic;
using System.Linq;

namespace Float.FileDownloader
{
    /// <summary>
    /// Extensions on IEnumerables of IPercentProgress elements.
    /// </summary>
    static class IEnumerableExtensions
    {
        /// <summary>
        /// For an enumerable of percent progresses, calculates the total percent progress.
        /// </summary>
        /// <returns>The sum percent progress.</returns>
        /// <param name="progresses">This enumerable of percent progress elements.</param>
        internal static double PercentSum(this IEnumerable<IPercentProgress> progresses)
        {
            // note that this mitigates errors due to NaN values
            return progresses.Aggregate(0.0, (aggregate, progress) => aggregate + (!double.IsNaN(progress.PercentComplete) ? progress.PercentComplete : 0.0));
        }

        /// <summary>
        /// Calculates the percent complete for all progresses by dividing the PercentSum by the number of elements.
        /// Note that this will return NaN for an empty collection.
        /// </summary>
        /// <returns>The total percent complete.</returns>
        /// <param name="progresses">This enumerable of percent progress elements.</param>
        internal static double TotalPercentComplete(this IEnumerable<IPercentProgress> progresses)
        {
            return progresses.PercentSum() / progresses.Count();
        }
    }
}

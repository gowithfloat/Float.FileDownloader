using System.Collections.Generic;
using Xunit;

namespace Float.FileDownloader.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void TestPercentSum()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(0),
                new SimplePercentProgress(0.5),
                new SimplePercentProgress(0.75)
            };

            Assert.Equal(1.25, list.PercentSum());
        }

        [Fact]
        public void TestTotalPercentComplete()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(0.3),
                new SimplePercentProgress(0.6),
                new SimplePercentProgress(0.9)
            };

            Assert.Equal(0.6, list.TotalPercentComplete());
        }

        [Fact]
        public void TestNan()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(double.NaN),
                new SimplePercentProgress(0.3),
                new SimplePercentProgress(0)
            };

            Assert.Equal(0.3, list.PercentSum());
            Assert.Equal(0.1, list.TotalPercentComplete(), 8);
        }

        [Fact]
        public void TestEmpty()
        {
            var list = new List<IPercentProgress>();
            Assert.Equal(0.0, list.PercentSum());
            Assert.Equal(double.NaN, list.TotalPercentComplete());
        }

        class SimplePercentProgress : IPercentProgress
        {
            internal SimplePercentProgress(double percentComplete)
            {
                PercentComplete = percentComplete;
            }

            public double PercentComplete { get; }
        }
    }
}

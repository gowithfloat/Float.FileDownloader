using System.Collections.Generic;
using NUnit.Framework;

namespace Float.FileDownloader.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Test]
        public void TestPercentSum()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(0),
                new SimplePercentProgress(0.5),
                new SimplePercentProgress(0.75)
            };

            Assert.AreEqual(1.25, list.PercentSum());
        }

        [Test]
        public void TestTotalPercentComplete()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(0.3),
                new SimplePercentProgress(0.6),
                new SimplePercentProgress(0.9)
            };

            Assert.AreEqual(0.6, list.TotalPercentComplete());
        }

        [Test]
        public void TestNan()
        {
            var list = new List<IPercentProgress>
            {
                new SimplePercentProgress(double.NaN),
                new SimplePercentProgress(0.3),
                new SimplePercentProgress(0)
            };

            Assert.AreEqual(0.3, list.PercentSum());
            Assert.AreEqual(0.1, list.TotalPercentComplete(), 8);
        }

        [Test]
        public void TestEmpty()
        {
            var list = new List<IPercentProgress>();
            Assert.AreEqual(0.0, list.PercentSum());
            Assert.AreEqual(double.NaN, list.TotalPercentComplete());
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

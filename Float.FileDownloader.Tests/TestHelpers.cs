using System;
using System.IO;

namespace Float.FileDownloader.Tests
{
    public static class TestHelpers
    {
        internal static IProgress<long> SimpleProgress()
        {
            return new Progress<long>(totalBytes => Console.WriteLine($"totalBytes: {totalBytes}"));
        }

        internal static string TempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        }

        internal static string TestUriString()
        {
            return "https://www.gowithfloat.com";
        }

        internal static Uri TestUri()
        {
            return new Uri(TestUriString());
        }

        internal static string RandomString(int length)
        {
            var random = new Random();
            var randomBytes = new byte[length];
            random.NextBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}

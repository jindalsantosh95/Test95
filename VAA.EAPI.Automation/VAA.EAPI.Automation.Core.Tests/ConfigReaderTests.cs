using Xunit;
using CoreConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;

namespace VAA.EAPI.Automation.Core.Tests
{
    public sealed class ConfigReaderTests
    {
        [Fact]
        public void Get_ReadsProcessEnvironmentVariableRegardlessOfCasing()
        {
            string suffix = Guid.NewGuid().ToString("N");
            string environmentKey = $"PlatformSafe_{suffix}";
            string requestedKey = environmentKey.ToUpperInvariant();
            const string expected = "configured-value";

            try
            {
                Environment.SetEnvironmentVariable(environmentKey, expected, EnvironmentVariableTarget.Process);

                Assert.Equal(expected, CoreConfigReader.Get(requestedKey));
            }
            finally
            {
                Environment.SetEnvironmentVariable(environmentKey, null, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable(requestedKey, null, EnvironmentVariableTarget.Process);
            }
        }

        [Fact]
        public void Get_ReturnsDefaultWhenEnvironmentVariableDoesNotExist()
        {
            string key = $"Missing_{Guid.NewGuid():N}";

            Assert.Equal("fallback", CoreConfigReader.Get(key, "fallback"));
        }

        [Fact]
        public void Get_PreservesExactCasePrecedenceAfterDifferentlyCasedLookup()
        {
            if (OperatingSystem.IsWindows())
                return;

            string suffix = Guid.NewGuid().ToString("N");
            string mixedCaseKey = $"PlatformExact_{suffix}";
            string upperCaseKey = mixedCaseKey.ToUpperInvariant();

            try
            {
                Environment.SetEnvironmentVariable(mixedCaseKey, "mixed", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable(upperCaseKey, "upper", EnvironmentVariableTarget.Process);

                Assert.Equal("mixed", CoreConfigReader.Get(mixedCaseKey));
                Assert.Equal("upper", CoreConfigReader.Get(upperCaseKey));
            }
            finally
            {
                Environment.SetEnvironmentVariable(mixedCaseKey, null, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable(upperCaseKey, null, EnvironmentVariableTarget.Process);
            }
        }
    }
}

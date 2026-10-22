using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Tests
{
    public static class ConnPulseTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                { "StarlinkAuthenticationUrl", "https://starlink/auth/token" },
                { "StarlinkSandboxUrl", "https://starlink.sandbox/" },
                { "StarlinkAPIClientId", "client-id" },
                { "StarlinkClientSecret", "client-secret" },
                { "CaptivePortalAsyncCheckUrl", "https://www.virginatlantic.com/?debug=test-debug-value" },
                { "CaptivePortalExpectedBackendHeaderValue", "starlink-portal" },
                { "InflightConnUrl", "https://www.virginatlantic.com" },
                { "InflightConnectivityHealth", "inflightconn-subscription-key" },
                { "GraphQLUrl", "https://www.virginatlantic.com" },
            };

            foreach (var item in settings)
            {
                ConfigReader.Set(item.Key, item.Value);
            }
        }
    }
}

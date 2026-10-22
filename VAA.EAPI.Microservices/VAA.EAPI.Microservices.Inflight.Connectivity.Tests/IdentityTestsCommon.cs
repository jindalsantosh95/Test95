using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using Xunit;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Tests
{
    [CollectionDefinition("EnvironmentDependentTests", DisableParallelization = true)]
    public class EnvironmentDependentTestsCollection
    {
    }
    public class InflightConnectivityITestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"StarlinkAuthenticationUrl", "https://starlink/auth/token"},
                {"StarlinkSandboxUrl", "https://starlink.sandbox/"},
                {"StarlinkAPIClientId", "client-id" },
                { "StarlinkClientSecret", "client-secret" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}

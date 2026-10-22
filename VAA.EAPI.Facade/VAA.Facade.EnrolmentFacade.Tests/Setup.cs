using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.Facade.EnrolmentFacade.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            VAA.EAPI.Common.AsyncContext.Current = new()
            {
                Context = new VAA.EAPI.Core.Functions.TestFunctionContext()
            };

            Dictionary<string, string> settings = new()
            {
                {"RetrieveEnrolmentOcpKey", "test-ocp-key" },
                {"RetrieveCustomerEndpoint", "testappid" },
                {"RetrieveCustomerApiResource", "testkey" },
                {"EapiMsClientID", "test-client-id" },
                {"EapiMSSecret", "test-client-secret" },
                {"AAD:Authority", "https://login.test/" },
                {"AAD:TenantId", "tenant" },
                {"RetryAttempts", "3" }
            };

            foreach (var item in settings)
            {
                ConfigReader.Set(item.Key, item.Value);
                System.Environment.SetEnvironmentVariable(item.Key, item.Value, System.EnvironmentVariableTarget.Process);
            }
        }
    }
}

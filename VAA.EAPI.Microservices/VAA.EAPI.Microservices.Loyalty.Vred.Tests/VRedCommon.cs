using VAA.EAPI.Common.Helpers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    [CollectionDefinition("EnvironmentDependentTests", DisableParallelization = true)]
    public class EnvironmentDependentTestsCollection
    {
    }

    public static class VRedCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"AzureWebJobsStorage", "UseDevelopmentStorage=true"},
                {"FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated"},
                {"APPINSIGHTS_INSTRUMENTATIONKEY", "7cae02b5-44f8-430c-8263-889548b786e0"},
                {"MemberMetaDataSchema", "https://dummy.com"},
                {"VRedAuthUrl", "https://dummy.com/oauth"},
                {"AuthAudience", "https://dummy.com"},
                {"AuthClientId", "dummy"},
                {"AuthClientSecret", "dummy"},
                {"VRed-Ocp-Apim-Subscription-Key", "dummy"},
                {"KeyVaultToken", "EAPI-AAD-VREDTokenKeyName"},
                {"KeyVaultTokenPeriod", "EAPI-AAD-VREDTokenPeriodKeyName"},
                {"KeyVaultExpireTokenValue", "EAPI-AAD-VREDExpireTokenValueKeyName" },
                {"VredEnableSignDate", "false" },
                {"updatevredauthurl","https://dummy.com"},
                {"CancelTransactionResponseUrl", "https://dummy.com" },
                {"VredTransactionAPICallbackUrl", "https://dummy.com" },
                {"VredTransactionAPIV2", "https://dummy.com" },
                {"CreditMetaDataSchema", "https://dummy.com"},
                {"DebitMetaDataSchema", "https://dummy.com"},
                {"ResponsePollingCount", "3"},
                {"ResponseDelayTimeInMS", "300"},
                {"MicroServiceDBKey", "XXXX" },
                {"MicroServiceEventStoreTableName", "VRedEventStore" },
                {"MicroServiceResponseTableName", "VRedResponse" },
                {"EarnTransactionResponseUrl", "https://dummy.com/correlationid" },
                {"BurnTransactionResponseUrl", "https://dummy.com/correlationid" },
                {"CreateTransactionResponseUrl", "https://dummy.com/correlationid" },
                {"VRedEndpointUrl", "https://dummy.red.com/balances/"},
                {"UrlVersion", "v1"},
                { "VRedStatementEndpointUrl", "https://dummy.red.com/stmt" }
    };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

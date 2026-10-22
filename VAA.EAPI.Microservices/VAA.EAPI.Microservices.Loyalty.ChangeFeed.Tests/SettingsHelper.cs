
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Tests
{
    public class SettingsHelper
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"SCV:CustDB:CustSearchColCntnr", "CustomerCollection" },
                {"CustDB:SCVCustSearchDBEndPoint", "https://avadeuncmdbcustomer.documents.azure.com:443/" },
                {"CustDB:SCVCustSearchDBName", "CustomerDB_NonProd"},
                {"SCV:CustDB:CustColCntnr", "test" },
                {"scv:custdb:endpoint", "test" },
                {"ResponsePollingCount", "5" },
                {"ResponseDelayTimeInMS", "10" },
                {"CustDB:SCVCustSearchDBCollection", "CustomerSearch" },
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}

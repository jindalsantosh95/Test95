using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class SettingsHelper
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"Audit:AuditDBEndPoint", "https://avadeuncmdbaudit.documents.azure.com:443/" },
                {"Audit:AuditDBName", "LoyaltyAuditDB" },
                {"Audit:AuditDBCollection", "AuditCollection"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
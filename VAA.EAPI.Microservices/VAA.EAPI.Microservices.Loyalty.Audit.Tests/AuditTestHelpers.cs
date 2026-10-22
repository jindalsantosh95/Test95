using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public static class AuditTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"Audit:AuditDBCollection", "testCollection" },
                {"Audit:AuditDBEndPoint", "https://dummy.com/123/" },
                {"Audit:AuditDBName", "testDB" },
                {"AwardAuditDocType", "dummy123" },
                {"ServiceBusConnectionString", "https://dummy.com/123/" },
                { "MicroServiceDBKey", "http://localhost:7071/api/{0}/{1}"},
                { "MicroServiceEventStoreTableName", "http://localhost:7071/api/{0}s/status/{1}"},
                { "MicroServiceResponseTableName", "30"},
                { "CosmosSqlDBConnectionMode", "3"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}

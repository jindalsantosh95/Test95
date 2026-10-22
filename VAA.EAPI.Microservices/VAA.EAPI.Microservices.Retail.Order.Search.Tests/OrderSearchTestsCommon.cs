using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class OrderSearchTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ReferenceDataMicroServiceDBKey", "https://avadeunkveapis.vault.azure.net/secrets/AVADEUNAFNORDERSEARCHMICROSERV-Config-StorageAccount/"},
                {"ReferenceDataTableName", "AirportLocation"},
                {"OrderDBEndpoint", "https://avadeunscvcmdbsql.documents.azure.com:443/"},
                {"DatabaseName", "OrderDB"},
                {"OrdersCollection", "OrdersCollection"},
                {"ApplicationId", "ND"},
                {"AppChannelName", "NDC"},
                {"DebugOptions", "Y"},
                {"TestLab", "TVTH"},
                {"PoolType", "STAMPING/VAA/NEW/DIST/CAPABILITY"},
                {"OptionNumberForDisplayRequest", "1"},
                {"ProvideResidualValue", "N"},
                {"TrustedSource", "Y"},
                {"AgentCityCode", "LON"},
                {"RoleIndicator", "RESNDCSALES"},
                {"SecurityId", "D027954"},
                {"DurationFormat", "HH:MM"},
                {"AirCraftDataTableName", "AircraftInstrumentDetails" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

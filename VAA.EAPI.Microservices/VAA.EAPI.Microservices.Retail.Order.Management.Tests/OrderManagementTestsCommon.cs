using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class OrderManagementTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"DebugOptions", "Y"},
                {"TestLab", "TVTH"},
                {"PrinterData", "14450A"},
                {"MicroServiceDBKey", "DefaultEndpointsProtocol=https;AccountName=avadeunstorderapi;AccountKey=8kdWXvWCAdvQzr2B0IzePknyoxVWx21Mc3fcxnbNbhJFP+MYUtaERKCBm6GP0VcEtZ4pJkrnSH5bPApGmVdGAA==;EndpointSuffix=core.windows.net"},
                {"ReferenceDataTableName","AirportLocation" },
                {"pooltype", "test" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

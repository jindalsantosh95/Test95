using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderTestsCommon
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
                {"DlOfferAuthenticationUrl", "https://test/oauth/token"},
                {"DlApiApplicationId", "DC"},
                {"DlApiChannelId", "DCOM"},
                {"DlOrderApiUri", "https://tempuri.org/"},
                {"DlOrderClientId", "34985"},
                {"DlOrderClientSecret", "test2345"},
                {"OpenApi__Scope", "https://test/"},
                {"OpenApi__TokenUrl", "https://test.microsoft.com/"},
                {"OrderCreateResponseUrl", "http://localhost:7071/order/response/{correlationid}"},
                {"ResponsePollingCount", "50"},
                {"customerorderenrollmentservicebustopicname", "enrolCustomer"},
                {"CustomerOrderEnrollmentServiceBusConnectionString", "Endpoint=sb://avadeunsbeapicustomer.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=c+xImgTp+eWsFBcVgAScH8A0iODexiYaR7nmLul9RCQ="},
                { "ResponseDelayTimeInMS", "100"},
                { "ServiceBusConnectionString", "testConnectionString"},
                { "ServiceBusTopicName", "testOrderTopic"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

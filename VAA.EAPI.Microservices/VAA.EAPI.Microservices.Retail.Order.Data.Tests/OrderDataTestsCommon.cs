using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderDataTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"OrderCollection", "OrderCollection"},
                {"DatabaseName", "SingleOrderViewDB_Dev"},
                {"OrderDBEndpoint", "https://avadeuncmdbsingleorderview.documents.azure.com:443/"},
                {"RefundReason", "Risk Free Cancellation"},
                {"ServiceBusTopicNameTicket", "ticket"},
                {"ServiceBusTopicName","Order"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

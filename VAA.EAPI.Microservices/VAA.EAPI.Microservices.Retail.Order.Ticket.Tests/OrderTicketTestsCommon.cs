using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Tests
{
    public class OrderTicketTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ServiceBusTopicName", "Ticket"},
                {"OrderPaymentServiceBusTopicName", "Payment"},
                {"RefundWindowInHours","24"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

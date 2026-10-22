using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"MockResponseEnabled", "N"},
                {"ServiceBusTopicName", "Order"},
                {"OrderNotificationResponseUrl", "http://localhost:7071/api/order/notification/Status/{correlationid}"},
                {"OrderNotifyStatusRetryAfterValue","2" },
                {"ResponsePollingCount","2"},
                {"ResponseDelayTimeInMS","100" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

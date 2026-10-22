using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                { "ServiceBusTopicName", "Payment" },
                { "MockOrderPaymentService", "false" },
                { "OrderPaymentResponseUrl", "https://Testresources.virginatlantic.com/dev/orderpaymentmicroservice/order/payment/status/{correlationid}" },
                { "PaymentStatusRetryAfterValue","2" },
                { "EnableMultiplePayment","false" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}

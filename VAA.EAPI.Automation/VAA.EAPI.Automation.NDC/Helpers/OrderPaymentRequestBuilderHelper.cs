using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class OrderPaymentRequestBuilderHelper
    {

        public static string AutorizePaymentRequest(string countryCode, string currencyCode, string amount, string cardType, string tokenisedCardNumber)
        {
            var payload = Core.Extensions.FileExtensions.ReadFile("AuthorizePayment", "json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["pos"]["CountryCode"] = countryCode;
            payloadObject["paymentDetails"][0]["amount"]["monetaryAmount"] = amount;
            payloadObject["paymentDetails"][0]["cardMethod"]["cardType"] = cardType;
            payloadObject["paymentDetails"][0]["cardMethod"]["cardNumber"] = tokenisedCardNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CapturePaymentRequest(string countryCode, string currencyCode, string Amount, string cardType, string tokenisedCardNumber, string transactionId)
        {
            var payload = Core.Extensions.FileExtensions.ReadFile("CapturePayment", "json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["pos"]["countryCode"] = countryCode;
            payloadObject["paymentDetails"][0]["paymentTransactionId"] = transactionId;
            payloadObject["paymentDetails"][0]["amount"]["monetaryAmount"] = Amount;
            payloadObject["paymentDetails"][0]["cardMethod"]["cardType"] = cardType;
            payloadObject["paymentDetails"][0]["cardMethod"]["cardNumber"] = tokenisedCardNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RefundPaymentRequest(string countryCode, string currencyCode, string Amount, string cardType, string tokenisedCardNumber, string transactionId)
        {
            var payload = Core.Extensions.FileExtensions.ReadFile("RefundPayment", "json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["pos"]["CountryCode"] = countryCode;
            payloadObject["paymentDetails"][0]["paymentTransactionId"] = transactionId;
            payloadObject["paymentDetails"][0]["amount"]["monetaryAmount"] = Amount;
            payloadObject["paymentDetails"][0]["cardMethod"]["cardType"] = cardType;
            payloadObject["paymentDetails"][0]["cardMethod"]["CardNumber"] = tokenisedCardNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string PnrCancel(string pnr)
        {
            var payload = Core.Extensions.FileExtensions.ReadFile("OrderCancel", "json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["rloc"] = pnr;

            return JsonConvert.SerializeObject(payloadObject);
        }

    }
}


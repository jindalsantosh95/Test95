using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.Microservices.OrderPayment
{
    public class OrderPayment
    {
        [Trait("Category", "CPD")]
        [Theory]
        [InlineData("GB", "GBP", "95.8", "VI", "4444333322221111")]
        [InlineData("GB", "USD", "500.09", "DC", "361253433727425")]
        [InlineData("GB", "AED", "1000.9", "CA", "4444333322221111")]

        public async Task AuthorizePaymentValidation(string countryCode, string currencyCode, string amount, string cardType, string unTokenisedCardNumber)
        {
            string authorizePaymentRequest = OrderPaymentRequestBuilderHelper.AutorizePaymentRequest(countryCode, currencyCode, amount, cardType, unTokenisedCardNumber);

            HttpResponseMessage authorizePaymentResponse = MicroserviceEndpoints.AuthorizePayment(authorizePaymentRequest).EnsureIsSuccessStatusCode();

            JObject authorizePaymentResponsejObject = JsonConvert.DeserializeObject<JObject>(await authorizePaymentResponse.Content.ReadAsStringAsync());

            string transactionStatus = authorizePaymentResponsejObject["transactionStatus"].ToString();

            Assert.True(transactionStatus == "Payment authorized", $"Transaction Status was not Payment authorized. Transaction status returned as {transactionStatus}");
        }

        [Trait("Category", "CPD")]
        [Theory]
        [InlineData("GB", "GBP", "695.8", "VI", "4444333322221111")]
        [InlineData("GB", "EUR", "1500.09", "DC", "361253433727425")]
        public async Task CapturePaymentValidation(string countryCode, string currencyCode, string amount, string cardType, string unTokenisedCardNumber)

        {
            string transactionId = OrderPaymentMicroserviceHelper.ReturnTransactionId(countryCode, currencyCode, amount, cardType, unTokenisedCardNumber);

            string capturePaymentRequest = OrderPaymentRequestBuilderHelper.CapturePaymentRequest(countryCode, currencyCode, amount, cardType, unTokenisedCardNumber, transactionId);

            HttpResponseMessage capturePaymentResponse = MicroserviceEndpoints.CapturePayment(capturePaymentRequest).EnsureIsSuccessStatusCode();

            JObject capturePaymentResponsejObject = JsonConvert.DeserializeObject<JObject>(await capturePaymentResponse.Content.ReadAsStringAsync());

            string transactionStatus = capturePaymentResponsejObject["transactionStatus"].ToString();

            Assert.True(transactionStatus == "Payment Captured", $"Transaction Status was not Payment Captured was returned as {transactionStatus}");
        }

        [Trait("Category", "CPD")]
        [Theory]
        [InlineData("GB", "GBP", "695.8", "500", "VI", "4444333322221111")]
        [InlineData("GB", "EUR", "1500.09", "500", "DC", "361253433727425")]
        public void RefundPaymentValidation(string countryCode, string currencyCode, string captureAmount, string refundAmount, string cardType, string unTokenisedCardNumber)
        {
            string transactionId = OrderPaymentMicroserviceHelper.ReturnTransactionId(countryCode, currencyCode, captureAmount, cardType, unTokenisedCardNumber);

            string capturePaymentRequest = OrderPaymentRequestBuilderHelper.CapturePaymentRequest(countryCode, currencyCode, captureAmount, cardType, unTokenisedCardNumber, transactionId);

            HttpResponseMessage capturePaymentResponse = MicroserviceEndpoints.CapturePayment(capturePaymentRequest).EnsureIsSuccessStatusCode();

            string refundPaymentRequest = OrderPaymentRequestBuilderHelper.RefundPaymentRequest(countryCode, currencyCode, refundAmount, cardType, unTokenisedCardNumber, transactionId);

            HttpResponseMessage refundPaymentResponse = MicroserviceEndpoints.RefundPayment(refundPaymentRequest).EnsureIsSuccessStatusCode();

            Assert.True(refundPaymentResponse.IsSuccessStatusCode, $"Transaction was not Refunded with success status code was returned as {(int)refundPaymentResponse.StatusCode}");
        }


    }
}

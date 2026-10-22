using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public class OrderPaymentMicroserviceHelper
    {

        public static string ReturnTransactionId(string countryCode, string currencyCode, string amount, string cardType, string unTokenisedCardNumber)
        {

            string authorizePaymentRequest = OrderPaymentRequestBuilderHelper.AutorizePaymentRequest(countryCode, currencyCode, amount, cardType, unTokenisedCardNumber);

            HttpResponseMessage authorizePaymentResponse = MicroserviceEndpoints.AuthorizePayment(authorizePaymentRequest).EnsureIsSuccessStatusCode();

            string authorizePaymentResult = authorizePaymentResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JObject authorizePaymentResponsejObject = JsonConvert.DeserializeObject<JObject>(authorizePaymentResult);

            var transactionId = authorizePaymentResponsejObject["transactionId"].ToString();

            return transactionId;

        }
    }
}


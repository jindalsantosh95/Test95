using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.NDC.Requests.Endpoints
{
    public static class MicroserviceEndpoints
    {
        public static readonly string env = Config.Environment;

        public static HttpResponseMessage Offers(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/OfferMicroservice/Offers"), requestPayload, headers);
        }

        public static HttpResponseMessage Price(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key"));
            headers.Add("ChannelId", "NDC");

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : $"{env}")}/OfferMicroservice/Offers/price"), requestPayload, headers);
        }
        public static HttpResponseMessage Seats(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("SeatsMicroserviceSubscriptionKey"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/seatmicroservice/RetrieveSeatMap"), requestPayload, headers);
        }

        //Orderpaymentmicroservice endpoints

        public static HttpResponseMessage AuthorizePayment(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("CPDAuthorizePaymentSubscriptionKey"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/orderpaymentmicroservice/AuthorizePayment"), requestPayload, headers);              
        }

        public static HttpResponseMessage CapturePayment(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("CPDAuthorizePaymentSubscriptionKey"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/orderpaymentmicroservice/CapturePayment"), requestPayload, headers);
        }

        public static HttpResponseMessage RefundPayment(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("CPDAuthorizePaymentSubscriptionKey"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/orderpaymentmicroservice/RefundPayment"), requestPayload, headers);
        }
    }
}

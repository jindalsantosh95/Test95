using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Requests;

namespace VAA.EAPI.Automation.Book.Requests.Endpoints
{
    public static class MicroserviceEndpoint
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage GetOffer(string offerId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/{offerId}"), headers);
        }

        public static HttpResponseMessage OfferSearch(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/search"), requestPayload, headers);
        }

        public static HttpResponseMessage CreateBasket(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key"));

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/basketmicroservice/basket"), requestPayload, headers);
        }

        public static HttpResponseMessage GetBasket(string locationUrl)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(locationUrl, headers);
        }

        public static HttpResponseMessage UpdateBasketWithOfferId(string basketId, string offerId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/basketmicroservice/basket/{basketId}/offer/{offerId}"), "", headers);
        }

        public static HttpResponseMessage UpdatePeopleBasket(string basketId, string id, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/basketmicroservice/basket/{basketId}/People/{id}"), requestPayload, headers);
        }

        public static HttpResponseMessage UpdateContactBasket(string basketId, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/basketmicroservice/basket/{basketId}/Contact"), requestPayload, headers);
        }
        public static HttpResponseMessage TermsAndConditionsBasketMS(string basketId, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/basketmicroservice/basket/{basketId}/Terms"), requestPayload, headers);
        }

        public static HttpResponseMessage DeleteBasket(string basketId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBASKETMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Delete(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/basketmicroservice/basket/{basketId}"), headers);
        }

        public static HttpResponseMessage Price(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/search/price"), requestPayload, headers);
        }

        public static HttpResponseMessage GetOrderBF(string bearer, string locationUrl)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNORDERMGTMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(locationUrl, headers);
        }

        public static HttpResponseMessage GetOrderPayment(string paymentReferenceId, string formOfPayment = "Card")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "formOfPayment", formOfPayment },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNORDERMGTMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/orderpaymentmicroservice/order/payment/paymentDetails/{paymentReferenceId}"), headers);
        }

        public static HttpResponseMessage GetFareRulesForSummary(string summaryOfferId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/{summaryOfferId}/farerules"), headers);
        }

        public static HttpResponseMessage Seats(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("SeatsMicroserviceSubscriptionKey") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatmicroservice/RetrieveSeatMap"), requestPayload, headers);
        }

        public static HttpResponseMessage SeatLopa(string fleetTypeCode, string equipmentCode)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/LopaMicroservice/{fleetTypeCode}/{equipmentCode}/Lopa"), headers);
        }

        public static HttpResponseMessage Sustainability(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNSUSTAINABILITYMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/sustainmicroservice/Sustainability"), requestPayload, headers);
        }

        public static HttpResponseMessage OfferFlexiSearch(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/FlexiSearch"), requestPayload, headers);
        }

        public static HttpResponseMessage OfferCalendarSearch(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/CalendarSearch"), requestPayload, headers);
        }

        public static HttpResponseMessage BrandMS(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNBRANDMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/brandmicroservice/brand"), requestPayload, headers);
        }

        public static HttpResponseMessage ReferenceMSGet(string endPointName)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNREFERENCEMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/ReferenceMicroservice/{endPointName}"), headers);
        }

        public static HttpResponseMessage ReferenceMSPost(string requestPayload, string endPointName)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNREFERENCEMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/ReferenceMicroservice/{endPointName}"), requestPayload, headers);
        }

        public static HttpResponseMessage InsuranceQuote(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("InsuranceMicroservice-SubscriptionKey") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/Insurance/InsuranceQuote"), requestPayload, headers);
        }

        public static HttpResponseMessage DeepLinkSummary(string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("AVATEUNAFNOFFERMICROSERV-Subscription-Automation-Key") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/OfferMicroservice/Offers/search/OfferPriceDeepLink"), requestPayload, headers);
        }

        //Added VRED endpoint call
        public static HttpResponseMessage Vred(string bearer, string flynigClubCardNumber)
        {
            var apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-SubscriptionKeyVred");

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                //Need to check where we will put this APIM Key 
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };

            return WebRequest.Get(Config.VredEndPoint.GetUrl($"/members/members/v1?partnerUserId={flynigClubCardNumber}&requestId=123"), headers);
        }
    }
}
using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Trips.Requests.Endpoint
{
    public static class Microservice
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage TripsSearch(string env, string requestPayload, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : $"{env}")}/digital/trip/trips/search"), requestPayload, headers);
        }

        public static HttpResponseMessage RetrieveBookingUsingPNR(string usingSearch, string firstName, string lastName, string pnr, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/digital/trip/trips/search"), Factory.Microservice.RetrieveBookingUsingPNR(usingSearch, firstName, lastName, pnr), headers);
        }

        public static HttpResponseMessage TripsList(string env, string usingString, List<string> pnrs, List<string> firstNames, List<string> lastNames, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/digital/trip/tripsList/search"), Factory.Microservice.TripsList(usingString, firstNames, lastNames, pnrs), headers);

        }

        public static HttpResponseMessage RetrieveTravelCheckList(string tripBookingId, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/digital/trip/messages/{tripBookingId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/digital/trip/messages/{tripBookingId}"), headers);
        }

        public static HttpResponseMessage RetrieveSsrsFromFlightService(string tripBookingId, string ocpSubKeyFlightServiceMS)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKeyFlightServiceMS }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/flightservicesmicroservice/ssrs/{tripBookingId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/flightservicesmicroservice/ssrs/{tripBookingId}"), headers);
        }

        public static HttpResponseMessage RetrieveBasketFromBasketMS(string basketId, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/basketmicroservice/basket/{basketId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/basketmicroservice/basket/{basketId}"), headers);
        }

        public static HttpResponseMessage Deeplink(string requestPayload, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/digital/trip/deeplink"), requestPayload, headers);
        }

        public static HttpResponseMessage GetCachedDeeplink(string id, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}")}/digital/trip/deeplinkdetail/{id}"), headers);
        }

        public static HttpResponseMessage PlusGradeAPIForBidForUpgradeAPI(string plusGradeRequest, string pnr, string lastName, string plusgradeApikey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "x-pnr", pnr },
                { "x-api-key", plusgradeApikey }
            };

            return WebRequest.Json.Post("https://request-api.stage.plusgrade.com/v1/offer/10gM7q0DVS/create", plusGradeRequest, headers);
        }

        public static HttpResponseMessage OperationalScheduleAPI(string airlineCode, string localDateFrom, string localDateTo, string direction, string originAirportCode, string destinationAirportCode, int flightNumber, int limit, string ocpSubKey)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : string.Empty)}/enterprise/operationallogistics/managetheoperation/operationalschedule/flights?Airline_Code={airlineCode}&Local_Date_From={localDateFrom}&Local_Date_To={localDateTo}&Direction={direction}&Origin_Airport_Code={originAirportCode}&Destination_Airport_Code={destinationAirportCode}&Flight_Number={flightNumber}&limit={limit}"), headers);
        }
    }
}

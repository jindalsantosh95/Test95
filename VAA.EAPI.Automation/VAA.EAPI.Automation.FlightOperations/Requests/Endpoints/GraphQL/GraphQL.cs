using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.FlightOperations.Requests.Endpoints.GraphQL
{
    public static class GraphQL
    {
        private readonly static string _env = Config.Environment;

        public static HttpResponseMessage RetrieveBookingPnr(string bearer, string bookingRef, string firstName, string lastName, string ocpSubKey = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQl")}"), Factory.GraphQL.RetrieveBookingWithPnrDetails(bookingRef, firstName, lastName), headers);
        }

        public static HttpResponseMessage RetrieveBookingsForLoyaltyAccount(string bearer, string b2cToken, string ocpSubKey = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"{bearer}, {b2cToken}" },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQl")}"), Factory.GraphQL.RetrieveBookingsLoyaltyAccount(), headers);
        }
    }
}
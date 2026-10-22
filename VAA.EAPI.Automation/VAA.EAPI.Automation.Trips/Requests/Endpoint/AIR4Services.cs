using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Trips.Requests.Endpoint
{
    public static class AIR4Services
    {
        public static string env = Config.Environment;
        public static HttpResponseMessage RetrieveTravelReservations(string firstName, string lastName, string pnr, string channelId, string appId)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "transactionid", Guid.NewGuid().ToString() },
                { "channelId", channelId },
                {"appid", appId }
            };
            // Add the test header only if not in production environment
            if (!env.Is("prod"))
            {
                headers.Add("x-air4_mytrips_test", Vault.Default.GetSecretValue("Air4MyTripsTest-Key"));
            }
            return WebRequest.Json.Post(Config.AIR4ResourcesMyTrips.GetUrl($"/v1/mytrips/travelreservations"), Factory.AIR4Services.RetrieveTravelReservations(firstName, lastName, pnr), headers);
        }

        public static HttpResponseMessage RetrieveSSRs(string channelId, string reservationResourceId)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "transactionid", Guid.NewGuid().ToString() },
                { "x-air4_mytrips_test", Vault.Default.GetSecretValue("Air4MyTripsTest-Key") },
                { "channelId", channelId }
            };
            return WebRequest.Get(Config.AIR4ResourcesRetrieveSSRs.GetUrl($"{reservationResourceId}"), headers);
        }

        public static HttpResponseMessage RetreiveMsgCenterAPI(string channelId, string appId, string reservationResourceId)
        {
            NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "transactionid", Guid.NewGuid().ToString() },
                { "x-air4_mytrips_test", Vault.Default.GetSecretValue("Air4MyTripsTest-Key") },
                { "channelId", channelId }
            };
            return WebRequest.Get(Config.AIR4ResourcesMessageCenter.GetUrl($"{reservationResourceId}"), headers);
        }
    }
}

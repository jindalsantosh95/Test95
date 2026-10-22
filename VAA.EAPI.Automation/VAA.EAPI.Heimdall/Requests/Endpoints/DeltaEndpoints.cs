using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Heimdall.Requests.Endpoints
{
    public static class DeltaEndpoints
    {
        public static HttpResponseMessage GenerateToken(string journeyType)
        {
            if (journeyType.Is("Offer"))
            {
                string data = $"client_id:{ConfigReader.Get("awsClientId")}\r\nclient_secret:{ConfigReader.Get("awsClientSecret")}\r\ngrant_type:{ConfigReader.Get("awsGrant")}\r\nscope:{ConfigReader.Get("awsScope")}";

                return WebRequest.FormUrlEncoded.Post(ConfigReader.Get("awsEndpoint"), data);
            }
            else if(journeyType.Is("Order"))
            {
                string data = $"client_id:{ConfigReader.Get("oauthClientId")}\r\nclient_secret:{ConfigReader.Get("oauthClientSecret")}\r\ngrant_type:{ConfigReader.Get("oauthGrant")}";

                return WebRequest.FormUrlEncoded.Post(ConfigReader.Get("oauthProd"), data);
            }
            return null;
        }

        public static HttpResponseMessage PriGeneration(string bearer)
        {
            string request = File.ReadAllText(@"Requests\Body\DeltaPRI.json");

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("applicationId", "DC");
            headers.Add("channelId", "DFWEB");
            headers.Add("Authorization", $"Bearer {bearer}");

            return WebRequest.Json.Post(ConfigReader.Get("deltaPriGeneration"), request, headers);
        }
    }
}

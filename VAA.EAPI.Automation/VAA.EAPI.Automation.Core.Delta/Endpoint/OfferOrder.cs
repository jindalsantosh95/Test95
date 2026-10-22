using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Delta.Endpoint
{
    public static class OfferOrder
    {
        public static HttpResponseMessage OfferSearch(string bearer, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);
            headers.Add("ApplicationId", "DC");
            headers.Add("ChannelId", "DCOM");

            return WebRequest.Json.Post(Config.DeltaEndPoint.GetUrl("/offerManagement/v1/offers/search"), requestPayload, headers);
        }
        public static HttpResponseMessage OrderCreate(string bearer, string offerid, string offerItemId, string totalAmt)
        {

            var payload = Factory.OfferOrder.OrderCreate(offerid, offerItemId, totalAmt);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);
            headers.Add("ApplicationId", "DC");
            headers.Add("ChannelId", "DCOM");

            return WebRequest.Json.Post(Config.DeltaEndPoint.GetUrl("/orderManagement/v1/orders"), payload, headers);
        }

        public static HttpResponseMessage OfferSearchSummary(string bearer, string offerid)
        {
            var payload = Factory.OfferOrder.OfferSearchSummary(offerid);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);
            headers.Add("ApplicationId", "DC");
            headers.Add("ChannelId", "DCOM");

            return WebRequest.Json.Post(Config.DeltaEndPoint.GetUrl("/offerManagement/v1/offers/search"), payload, headers);
        }
        public static HttpResponseMessage DeltaAwsOfferSearch(string bearer, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);
            headers.Add("Content-Type", "application/json");
            headers.Add("ApplicationId", "DC");
            headers.Add("ChannelId", "DFWEB");
            headers.Add("transactionId", Guid.NewGuid().ToString());

            return WebRequest.Json.Post(Config.AwsDeltaEndPoint.GetUrl("/si/offers/search"), requestPayload, headers);
        }
        public static HttpResponseMessage DeltaOfferGraphql(string bearer, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);
            headers.Add("Content-Type", "application/json");
            headers.Add("ApplicationId", "DC");
            headers.Add("ChannelId", "DFWEB");
            headers.Add("transactionId", Guid.NewGuid().ToString());

            return WebRequest.Json.Post(Config.AwsDeltaEndPoint.GetUrl("/si/rm-offer-gql"), requestPayload, headers);
        }

        public static HttpResponseMessage DeltaOfferGraphqlProduction(string bearer, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("TransactionId", "aws-si-lt-7");
            headers.Add("applicationId", "DC");
            headers.Add("channelId", "DFWEB");
            headers.Add("Authorization", $"Bearer {bearer}");

            return WebRequest.Json.Post(ConfigReader.Get("deltaOfferGql"), requestPayload, headers);
        }
    }
}
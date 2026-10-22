using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Search.Helpers
{
    public static class DeltaOfferSearchHelper
    {
        private static Random rnd = new Random();

        public static int DeltaOffersReturnedCount(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            int count = offerSearchResult.SelectTokens("$..offersSets[*].offers[*].offerId").Count();

            return count;
        }
    }
}
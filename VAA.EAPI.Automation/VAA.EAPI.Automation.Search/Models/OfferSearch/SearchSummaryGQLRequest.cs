using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SearchSummaryGQLRequest
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string operationName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string pqId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string query { get; set; }

        public SearchSummaryGQLVariables variables { get; set; }
    }
}

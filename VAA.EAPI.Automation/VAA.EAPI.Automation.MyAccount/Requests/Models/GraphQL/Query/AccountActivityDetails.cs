using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Query
{
    public class AccountActivityDetails
    {
        [JsonProperty("page")]
        public int? Page { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }
    }
}


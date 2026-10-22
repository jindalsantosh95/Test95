using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL
{
    public class GraphQLRequest
    {
        [JsonProperty("query")]
        public required string Query { get; set; }

        [JsonProperty("variables")]
        public required dynamic Variables { get; set; }
    }
}

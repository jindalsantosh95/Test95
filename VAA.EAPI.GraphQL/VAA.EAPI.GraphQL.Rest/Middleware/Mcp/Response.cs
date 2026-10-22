using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class Response
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";
        [JsonProperty("id")]
        public JToken Id { get; set; }
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Result { get; set; }
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }
    }
}

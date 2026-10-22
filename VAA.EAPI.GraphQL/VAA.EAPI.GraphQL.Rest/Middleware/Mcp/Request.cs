using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class Request
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Params { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Id { get; set; }
    }
}

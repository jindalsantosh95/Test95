using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class Error
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public JToken Data { get; set; }
    }
}

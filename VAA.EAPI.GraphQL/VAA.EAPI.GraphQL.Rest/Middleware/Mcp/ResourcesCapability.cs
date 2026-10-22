using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class ResourcesCapability
    {
        [JsonProperty("listChanged")]
        public bool ListChanged { get; set; }
        [JsonProperty("subscribe")]
        public bool Subscribe { get; set; }
    }
}
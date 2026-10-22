using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class PromptsCapability
    {
        [JsonProperty("listChanged")]
        public bool ListChanged { get; set; }
    }
}
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class Parameter
    {
        [JsonProperty("name")]
        public string Name { get; init; } = default!;
        [JsonProperty("description")]
        public string Description { get; init; } = default!;
        [JsonProperty("type")]
        public string Type { get; init; } = default!;
    }
}

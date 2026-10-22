using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{

    public sealed class ToolAnnotations
    {
        [JsonProperty("destructiveHint")]
        public bool? DestructiveHint { get; set; }

        [JsonProperty("idempotentHint")]
        public bool? IdempotentHint { get; set; }

        [JsonProperty("openWorldHint")]
        public bool? OpenWorldHint { get; set; }

        [JsonProperty("readOnlyHint")]
        public bool? ReadOnlyHint { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }
    }
}
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{

    public sealed class ToolExecution
    {
        [JsonProperty("taskSupport")]
        public string? TaskSupport { get; set; }
    }
}
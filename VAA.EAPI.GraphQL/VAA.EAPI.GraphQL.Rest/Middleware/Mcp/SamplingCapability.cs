using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class SamplingCapability
    {
        [JsonProperty("createMessage")]
        public bool CreateMessage { get; set; }
    }
}
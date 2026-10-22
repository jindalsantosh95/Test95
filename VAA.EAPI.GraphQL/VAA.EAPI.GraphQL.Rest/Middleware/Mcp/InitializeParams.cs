
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class InitializeParams
    {
        [JsonProperty("protocolVersion")]
        public string ProtocolVersion { get; set; } = default!;
        [JsonProperty("capabilities")]
        public ServerCapabilities Capabilities { get; set; } = default!;
        [JsonProperty("clientInfo")]
        public ClientInfo ClientInfo { get; set; } = default!;
    }
}
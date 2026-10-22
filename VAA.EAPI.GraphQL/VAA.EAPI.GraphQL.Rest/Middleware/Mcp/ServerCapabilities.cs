using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class ServerCapabilities
    {
        [JsonProperty("tools")]
        public ToolsCapability Tools { get; set; }

        [JsonProperty("resources")]
        public ResourcesCapability Resources { get; set; }

        [JsonProperty("prompts")]
        public PromptsCapability Prompts { get; set; }

        [JsonProperty("logging")]
        public object Logging { get; set; } // Empty object means "supported"

        [JsonProperty("experimental")]
        public Dictionary<string, object> Experimental { get; set; }
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class ListToolsResult
    {
        public static explicit operator ListToolsResult(Tool[] source) => new() { Tools = source };

        [JsonProperty("_meta")]
        public Dictionary<string, object>? Meta { get; set; }

        [JsonProperty("nextCursor")]
        public string? NextCursor { get; set; }

        [JsonProperty("tools")]
        public IEnumerable<Tool> Tools { get; set; } = [];
    }
}
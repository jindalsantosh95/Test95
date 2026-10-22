using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class Icon
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeType { get; set; }

        [JsonProperty("sizes", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Sizes { get; set; }

        [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
        public string Theme { get; set; }
    }
}

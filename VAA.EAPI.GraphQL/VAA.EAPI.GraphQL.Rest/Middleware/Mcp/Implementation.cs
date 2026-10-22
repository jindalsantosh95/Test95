using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    internal sealed class Implementation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("websiteUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string WebsiteUrl { get; set; }

        [JsonProperty("icons", NullValueHandling = NullValueHandling.Ignore)]
        public List<Icon> Icons { get; set; }
    }
}
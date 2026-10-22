using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SearchSlice
    {
        [JsonPropertyName("origin")]
        public string origin { get; set; }
        [JsonPropertyName("destination")]
        public string destination { get; set; }
        [JsonPropertyName("departure")]
        public DateOnly departure { get; set; }

        [JsonPropertyName("connections")]
        public IEnumerable<string> connections { get; set; }
    }
}

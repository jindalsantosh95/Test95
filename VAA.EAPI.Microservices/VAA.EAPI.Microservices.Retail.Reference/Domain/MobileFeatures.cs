using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public sealed class MobileFeatures
    {
        [JsonProperty("features", Required = Required.Always)]
        public Dictionary<string, string> Features { get; set; } = new();
    }
}

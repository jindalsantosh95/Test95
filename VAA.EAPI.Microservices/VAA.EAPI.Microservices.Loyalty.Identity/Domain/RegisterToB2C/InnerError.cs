using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class InnerError
    {
        public DateTime date { get; set; }

        [JsonProperty("request-id")]
        public string RequestId { get; set; }

        [JsonProperty("client-request-id")]
        public string ClientRequestId { get; set; }
    }
}

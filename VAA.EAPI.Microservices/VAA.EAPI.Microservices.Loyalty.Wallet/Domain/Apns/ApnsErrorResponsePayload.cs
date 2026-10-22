using System;
using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns
{
    public class ApnsErrorResponsePayload
    {
        [JsonIgnore]
        public ApnsResponseReason Reason =>
            Enum.TryParse<ApnsResponseReason>(ReasonRaw, out var value)
            ? value : ApnsResponseReason.Unknown;

        [JsonProperty("reason")]
        public string ReasonRaw { get; set; }
        
    }
}

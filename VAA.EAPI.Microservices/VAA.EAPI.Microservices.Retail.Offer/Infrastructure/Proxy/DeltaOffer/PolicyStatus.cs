using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PolicyStatus
    {
        [EnumMember(Value = "IN_POLICY")]
        INPOLICY = 1,
        [EnumMember(Value = "NOT_ALLOWED")]
        NOTALLOWED = 2,
        [EnumMember(Value = "OUT_POLICY")]
        OUTPOLICY = 3
    }
}

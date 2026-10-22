using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MeasureUnit
    {
        [EnumMember(Value = "LBS")]
        LBS = 1,
        [EnumMember(Value = "KG")]
        KG = 2,
        [EnumMember(Value = "MI")]
        MI = 3,
        [EnumMember(Value = "KM")]
        KM = 4,
        [EnumMember(Value = "PCT")]
        PCT = 5,
        [EnumMember(Value = "IN")]
        IN = 6,
        [EnumMember(Value = "CMS")]
        CMS = 7
    }
}

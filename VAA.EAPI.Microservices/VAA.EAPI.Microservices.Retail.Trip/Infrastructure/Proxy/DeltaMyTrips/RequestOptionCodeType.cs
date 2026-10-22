using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestOptionCodeType
    {
        [EnumMember(Value = "CONFIRMATION")]
        CONFIRMATION = 1,
        [EnumMember(Value = "TICKET")]
        TICKET = 2,
        [EnumMember(Value = "CREDITCARD")]
        CREDITCARD = 3,
        [EnumMember(Value = "LOYALTYNUM")]
        LOYALTYNUM = 4
    }
}
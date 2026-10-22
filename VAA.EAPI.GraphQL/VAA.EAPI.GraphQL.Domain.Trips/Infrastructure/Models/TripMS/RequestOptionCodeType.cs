using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestOptionCodeType
    {
        [EnumMember(Value = "PNR")]
        PNR = 1,
        [EnumMember(Value = "TICKET")]
        TICKET = 2,
        [EnumMember(Value = "CREDITCARD")]
        CREDITCARD = 3,
        [EnumMember(Value = "LOYALTYNUM")]
        LOYALTYNUM = 4,
        [EnumMember(Value = "HOLIDAYS")]
        HOLIDAYS = 5
    }
}

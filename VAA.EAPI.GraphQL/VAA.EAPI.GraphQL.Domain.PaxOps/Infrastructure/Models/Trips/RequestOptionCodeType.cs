using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
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
        LOYALTYNUM = 4
    }
}

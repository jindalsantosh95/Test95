using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
        [EnumMember(Value = "Ticket")]
        Ticket = 100,
        [EnumMember(Value = "Seat")]
        Seat = 200
    }
}

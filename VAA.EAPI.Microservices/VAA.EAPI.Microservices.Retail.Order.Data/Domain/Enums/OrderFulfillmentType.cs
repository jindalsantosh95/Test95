using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderFulfillmentType
    {
        [EnumMember(Value = "PNR")]
        PNR = 0,
        [EnumMember(Value = "ETicket")]
        ETicket = 1
    }
}

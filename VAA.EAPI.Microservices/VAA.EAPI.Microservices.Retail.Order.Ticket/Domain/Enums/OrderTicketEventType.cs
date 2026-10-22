using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderTicketEventType
    {
        [EnumMember(Value = "VAA.Order.Ticket.GetOrderDetail")]
        OrderTicketDetail,
        [EnumMember(Value = "VAA.Order.Ticket.InitiateVoidTransaction")]
        VoidTransactionInitiate,
    }
}

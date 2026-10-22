using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChangeTypeEnum
    {
        [EnumMember(Value = "FlightDateTimeChange")]
        FlightDateTimeChange = 0,
        [EnumMember(Value = "FlightNumberChange")]
        FlightNumberChange = 1,
        [EnumMember(Value = "FlightRouteChange")]
        FlightRouteChange = 2,
        [EnumMember(Value = "FlightCancellation")]
        FlightCancellation = 3,
        [EnumMember(Value = "DocumentIssuance")]
        DocumentIssuance = 4,
        [EnumMember(Value = "DocumentVoided")]
        DocumentVoided = 5,
        [EnumMember(Value = "DocumentRefunded")]
        DocumentRefunded = 6,
        [EnumMember(Value = "DocumentExchanged")]
        DocumentExchanged = 7,
        [EnumMember(Value = "OrderItemAdded")]
        OrderItemAdded = 8
    }
}

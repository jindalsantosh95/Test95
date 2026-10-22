using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain
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
        [EnumMember(Value = "OrderItemAdded")]
        OrderItemAdded = 4,
        [EnumMember(Value = "DocumentIssuance")]
        DocumentIssuance = 5,
        [EnumMember(Value = "DocumentVoided")]
        DocumentVoided = 6,
        [EnumMember(Value = "NameCorrection")]
        NameCorrection = 7,
        [EnumMember(Value = "DocumentExchanged")]
        DocumentExchanged = 8,
        [EnumMember(Value = "IdentityDocumentDetailsUpdated")]
        IdentityDocumentDetailsUpdated = 9,
        [EnumMember(Value = "SplitPNR")]
        SplitPNR = 10,
        [EnumMember(Value = "SpecialServiceRequestAdded")]
        SpecialServiceRequestAdded = 11,
        [EnumMember(Value = "PaymentFailure")]
        PaymentFailure = 12,
        [EnumMember(Value = "DocumentRefunded")]
        DocumentRefunded = 13,
        [EnumMember(Value = "UNKNOWN")]
        Unknown
    }
}

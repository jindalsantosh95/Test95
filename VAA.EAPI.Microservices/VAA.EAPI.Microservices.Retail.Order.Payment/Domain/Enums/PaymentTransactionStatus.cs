using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentTransactionStatus
    {
        [EnumMember(Value = "Success")]
        Success = 1,
        [EnumMember(Value = "Failure")]
        Failure = 2
    }
}
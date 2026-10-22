using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionType
    {
        [EnumMember(Value = "Payment ")]
        Payment = 0,
        [EnumMember(Value = "Refund")]
        Refund = 1,
    }
}

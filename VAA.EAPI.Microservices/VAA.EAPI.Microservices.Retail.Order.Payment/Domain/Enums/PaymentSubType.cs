using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentSubType
    {
        [EnumMember(Value = "Credit")]
        Credit = 1,
        [EnumMember(Value = "Debit")]
        Debit = 2
    }
}

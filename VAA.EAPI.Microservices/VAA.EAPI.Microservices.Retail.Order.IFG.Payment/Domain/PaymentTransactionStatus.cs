using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethod
    {
        [EnumMember(Value = "DirectDebit")]
        DD = 1,
        [EnumMember(Value = "CreditDebit")]
        CD = 2,
        [EnumMember(Value = "eWallet")]
        EWallet = 3,
        [EnumMember(Value = "Cash")]
        Cash = 4,
    }
}

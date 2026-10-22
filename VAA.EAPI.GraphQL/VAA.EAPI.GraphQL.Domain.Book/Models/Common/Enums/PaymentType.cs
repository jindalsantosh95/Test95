using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentType
    {
        [EnumMember(Value = "Card")]
        Card = 1,
        [EnumMember(Value = "Voucher")]
        Voucher = 2,
        [EnumMember(Value = "Wallet")]
        Wallet = 3,
        [EnumMember(Value = "APM")]
        APM = 4,
        [EnumMember(Value = "CardToken")]
        CardToken = 5,
        [EnumMember(Value = "Virtual")]
        Virtual = 6,
        [EnumMember(Value = "OnlineBanking")]
        OnlineBanking = 7,
        [EnumMember(Value = "ApplePay")]
        ApplePay = 8,
        [EnumMember(Value = "PayPal")]
        PayPal = 9,
    }
}
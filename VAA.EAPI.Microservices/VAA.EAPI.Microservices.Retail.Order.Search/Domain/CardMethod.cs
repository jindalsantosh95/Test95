using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class CardMethod
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CardTypes
        {
            [EnumMember(Value = "Visa")]
            VisaEnum = 0,
            [EnumMember(Value = "MasterCard")]
            MasterCardEnum = 1,
            [EnumMember(Value = "Amex")]
            AmexEnum = 2,
            [EnumMember(Value = "UATP")]
            UATPEnum = 3
        }
        public CardTypes? CardType { get; set; }
        public string CardNumber { get; set; }
        public string IsTokenised { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string CVV { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public string CardBrandCode { get; set; }
    }
}

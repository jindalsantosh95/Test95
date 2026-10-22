using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class CardMethod
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CardTypes
        {
            Visa,
            MasterCard,
            Amex,
            UATP
        }
        public CardTypes? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? IsTokenised { get; set; }
        public string? IssueDate { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CardholderName { get; set; }
        public string? CVV { get; set; }
        public BillingAddress? BillingAddress { get; set; }
    }
}

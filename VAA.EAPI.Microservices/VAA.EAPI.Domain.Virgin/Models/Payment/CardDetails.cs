using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
{

    public class CardDetails
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CardTypes
        {
            VI = 0,
            CA = 1,
            AX = 2,
            TP = 3,
            DC = 4,
            DS = 5,
            TO = 6,

            Visa = VI,
            MasterCard = CA,
            AmericanExpress = AX,
            UATP = TP,
            Diners = DC,
            Discover = DS,
            Maestro = TO

        }
        public CardTypes? CardType { get; set; }
        public string CardNumber { get; set; }
        [OpenApiProperty(Description = "Is the card number provided already tokenised. This means that the airline has key to de-tokenise in order to take payment.")]
        public bool? IsTokenised { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; }
        public Address BillingAddress { get; set; }
        public PaymentCardAuthentication AuthenticationData { get; set; }
        public string CardNetworkCode { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardBrandCode { get; set; }

        public static explicit operator CardDetails(JObject source)
        {
            return new CardDetails()
            {
                CardType = source["CardType"] != null ? source["CardType"].ToString().Parse<CardTypes>() : null,
                CardNumber = source["CardNumber"]?.ToString(),
                IsTokenised = source["IsTokenised"] != null ? (bool)source["IsTokenised"] : null,
                IssueDate = source["IssueDate"]?.ToString(),
                ExpiryDate = source["ExpiryDate"]?.ToString(),
                CardholderName = source["CardholderName"]?.ToString(),
                Cvv = source["Cvv"]?.ToString(),
                BillingAddress = source["BillingAddress"] != null ? (Address)source["BillingAddress"] : null,
                AuthenticationData = source["AuthenticationData"] != null ? (PaymentCardAuthentication)source["AuthenticationData"] : null,
                CardNetworkCode = source["CardNetworkCode"]?.ToString(),
                MaskedCardNumber = source["MaskedCardNumber"]?.ToString(),
                CardBrandCode = source["CardBrandCode"]?.ToString()
            };
        }
    }
}

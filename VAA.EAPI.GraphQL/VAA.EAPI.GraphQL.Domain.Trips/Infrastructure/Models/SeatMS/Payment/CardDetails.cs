using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
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
        public bool? IsTokenised { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; }
        public Address BillingAddress { get; set; }
        public string CardNetworkCode { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardBrandCode { get; set; }


        public static explicit operator CardDetails((PaymentMS.PaymentDeails.CardDetails cardMethod, PaymentMS.PaymentDeails.PaymentPayer payer, PaymentMS.Customer.Address billingAddress) source)
        {
            var (cardMethod, Payer, billingAddress) = source;
            if (cardMethod == null) return null;

            return new CardDetails
            {
                CardType = cardMethod.CardType == null ? null : (CardTypes?)cardMethod.CardType,
                CardNumber = cardMethod.CardNumber,
                ExpiryDate = cardMethod.ExpiryDate,
                CardholderName = $"{Payer?.FirstName} {Payer?.LastName}",
                Cvv = cardMethod.Cvv,
                BillingAddress = (Address)billingAddress
            };
        }
    }
}
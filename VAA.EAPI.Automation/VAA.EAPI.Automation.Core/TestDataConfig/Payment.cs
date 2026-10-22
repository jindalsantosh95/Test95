using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;
using VAA.EAPI.Automation.Core.TestDataConfig.APMPayment;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Microservices.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Payment
    {
        public CardDetails CardDetails { get; set; }
        public APMDetails APMDetails { get; set; }
        public string PaymentType {  get; set; }

        public Payment()
        {
            PaymentType = "Card";
            CardDetails = CreateCardDetails(CardType.Visa);
            APMDetails = CreateAPMdDetails(APMType.PayPal);

        }

        public Payment(CardType cardType)
        {
            PaymentType = "Card";
            CardDetails = CreateCardDetails(cardType);
        }

        private CardDetails CreateCardDetails(CardType cardType)
        {
            return cardType switch
            {
                CardType.Visa => new Visa(),
                CardType.Mastercard => new Mastercard(),
                CardType.Discover => new Discover(),
                CardType.Diners => new Diners(),
                CardType.Visa3DS => new Visa3DS(),
                CardType.Mastercard3DS => new Mastercard3DS(),
                CardType.Discover3DS => new Discover3DS(),
                CardType.Diners3DS => new Diners3DS(),
                _ => throw new ArgumentException("Invalid card type")
            };
        }

        private APMDetails CreateAPMdDetails(APMType apmType)
        {
            return apmType switch
            {
                APMType.PayPal => new PayPal(),
                _ => throw new ArgumentException("Invalid APM type")
            };
        }
    }
}

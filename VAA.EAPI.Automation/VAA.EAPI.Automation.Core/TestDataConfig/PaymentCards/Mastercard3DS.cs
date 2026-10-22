using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Mastercard3DS : CardDetails
    {
        public Mastercard3DS()
        {
            CardNumber = "5555555555554444";
            PaymentCardType = CardType.Mastercard3DS;
            IsThreeDs = true;
        }
    }
}
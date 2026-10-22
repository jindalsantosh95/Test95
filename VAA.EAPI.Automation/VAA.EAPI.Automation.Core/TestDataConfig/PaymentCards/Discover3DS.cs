using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Discover3DS : CardDetails
    {
        public Discover3DS()
        {
            CardNumber = "6509822240414441";
            PaymentCardType = CardType.Discover3DS;
            IsThreeDs = true;
        }
    }
}
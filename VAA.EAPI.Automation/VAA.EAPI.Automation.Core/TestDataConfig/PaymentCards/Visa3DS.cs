using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Visa3DS : CardDetails
    {
        public Visa3DS()
        {
            CardNumber = "4444333322221111";
            PaymentCardType = CardType.Visa3DS;
            IsThreeDs = true;
        }
    }
}
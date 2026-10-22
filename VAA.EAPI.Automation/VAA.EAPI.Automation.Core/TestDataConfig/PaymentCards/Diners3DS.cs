using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Diners3DS : CardDetails
    {
        public Diners3DS()
        {
            CardNumber = "36125351202638";
            PaymentCardType = CardType.Diners3DS;
            IsThreeDs = true;
        }
    }
}
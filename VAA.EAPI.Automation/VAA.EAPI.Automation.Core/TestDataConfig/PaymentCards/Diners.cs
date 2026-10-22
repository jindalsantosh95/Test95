using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Diners : CardDetails
    {
        public Diners()
        {
            CardNumber = "36125361164240";
            PaymentCardType = CardType.Diners;
            IsThreeDs = false;
        }
    }
}
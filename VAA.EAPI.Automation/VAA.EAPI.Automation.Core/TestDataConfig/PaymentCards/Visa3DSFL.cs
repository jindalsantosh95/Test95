using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Visa3DSFL : CardDetails
    {
        public Visa3DSFL()
        {
            CardNumber = "4000027891380961";
            PaymentCardType = CardType.Visa3DSFL;
            IsThreeDs = true;
        }
    }
}

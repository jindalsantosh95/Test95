using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Visa : CardDetails
    {
        private static readonly string[] CardNumbers = { "4000000000002719", "4000020951595032", "4761344136141390" };

        public Visa()
        {
            CardNumber = CardNumbers[Random.Shared.Next(CardNumbers.Length)];
            PaymentCardType = CardType.Visa;
            IsThreeDs = false;
        }
    }
}
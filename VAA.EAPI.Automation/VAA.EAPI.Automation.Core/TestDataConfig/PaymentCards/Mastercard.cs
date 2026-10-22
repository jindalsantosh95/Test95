using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class Mastercard : CardDetails
    {
        private static readonly string[] CardNumbers = { "5333308664112277", "5101081046006034" };

        public Mastercard()
        {
            CardNumber = CardNumbers[Random.Shared.Next(CardNumbers.Length)];
            PaymentCardType = CardType.Mastercard;
            IsThreeDs = false;
        }
    }
}
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class CardDetails
    {
        public string NameOnCard { get; set; }

        public string CardNumber { get; set; }

        public CardType PaymentCardType { get; set; }

        public string ExpiryDate { get; set; }

        public string Cvv { get; set; }

        public string AddressLineOne { get; set; }

        public string AddressLineTwo { get; set; }

        public string TownCity { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public string Postcode { get; set; }

        public bool IsThreeDs { get; set; }

        public CardDetails()
        {
            NameOnCard = "Name onCard";
            Cvv = "123";
            ExpiryDate = "1228";
            AddressLineOne = "12 address line One";
            AddressLineTwo = "addressline Two";
            TownCity = "Town";
            Country = "United Kingdom";
            Postcode = "CV47 2TH";
        }
    }
}
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards
{
    public class NdcCardDetails
    {
        public string PaymentTypeCode { get; set; }

        public string CardNumber { get; set; }

        public string CardSecurityCode { get; set; }

        public string CardTypeText { get; set; }

        public string ExpirationDate { get; set; }

        public string AuthenticationValue { get; set; }

        public string TrxStatusText { get; set; }

        public string ElectronicCommerceInd { get; set; }

        public string DirectoryServerTrxID { get; set; }

        public string EnrollmentStatusText { get; set; }

        public string PaymentTrxChannelCode { get; set; }

        public string[] StreetText { get; set; }

        public string PostalCode { get; set; }

        public string CityName { get; set; }

        public string CountryCode { get; set; }

        public string GivenName { get; set; }

        public string Surname { get; set; }

        public NdcCardDetails()
        {
            PaymentTypeCode = "CC";
            CardNumber = "52012Y1HNJGI9736";
            CardSecurityCode = "123";
            CardTypeText = "CA";
            ExpirationDate = "1226";
            AuthenticationValue = "ODQzNjgwNjU0ZjM3N2JmYTg0NTM";
            TrxStatusText = "Y";
            ElectronicCommerceInd = "02";
            DirectoryServerTrxID = "c272b04f-6e7b-43a2-bb78-90f4fb94aa25";
            EnrollmentStatusText = "CardEnrolled3DS2.1.0";
            PaymentTrxChannelCode = "TO";
            StreetText = new string[] { "8", "Cobbles Crescent" };
            PostalCode = "RH108HA";
            CityName = "Crawley";
            CountryCode = "GB";
            GivenName = "David";
            Surname = "Smith";

        }
    }
}
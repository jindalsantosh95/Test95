namespace VAA.EAPI.Automation.Core.UI.ElementLibrary
{
    public class CpdPaymentPage
    {
        public readonly static string Card = "//span[text()='Card']";

        public readonly static string NameOnCard = "//input[@id='cardInfo_Name']";

        public readonly static string CardNumber = "//input[@id='cardInfo_cardNumber']";

        public readonly static string ExpiryDate = "//input[@id='cardInfo_expiryTo']";

        public readonly static string CVV = "//input[@id='cardInfo_cvv']";

        public readonly static string AddressLine1 = "//input[@id='billingInfo_address1']";

        public readonly static string AddressLine2 = "//input[@id='billingInfo_address2']";

        public readonly static string TownCity = "//input[@id='billingInfo_city']";

        public readonly static string CountryDropdown = "//div[@id='demo-simple-select']";

        public readonly static string CountryOption = "//div[@id='menu-']//li[@role='option' and normalize-space(.)='country']";

        public readonly static string State = "//input[@id='cardInfo_State']";

        public readonly static string Postcode = "//input[@id='billingInfo_zipcode']";

        public readonly static string PayAndConfirmBooking = "//button";
    }
}

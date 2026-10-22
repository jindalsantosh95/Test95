namespace VAA.EAPI.Automation.Core.UI.ElementLibrary
{
    public class HPPForm
    {
        public readonly static string Card = "//div[text()=' Card ']";

        public readonly static string PayPal = "//div[text()=' PayPal ']";

        public readonly static string PayWithPayPal = "//div[contains(@id,'paypal-buttons')]";

        public readonly static string NameOnCard = "//input[@formcontrolname = 'name']";

        public readonly static string CardNumber = "//input[@formcontrolname = 'paymentCardNum']";

        public readonly static string ExpiryDate = "//input[@formcontrolname = 'expiryDate']";

        public readonly static string CVV = "//input[@formcontrolname = 'cardVerificationValueNum']";

        public readonly static string AddressLine1 = "//input[@formcontrolname = 'addressLine1Text']";

        public readonly static string AddressLine2 = "//input[@formcontrolname = 'addressLine2Text']";

        public readonly static string TownCity = "//input[@formcontrolname = 'cityLocalityName']";

        public readonly static string Country = "//input[@formcontrolname = 'countryCode']";

        public readonly static string State = "//input[@formcontrolname = 'state']";

        public readonly static string Postcode = "//input[@formcontrolname = 'postalCode']";

        public readonly static string PayAndConfirmBooking = "//button";

        public readonly static string ProcessingYourSecurePaymentSpinner = "//div[@class='loaderContainer']";

        public readonly static string PayPalEmailOrMobileNumber = "//input[@id='email']";

        public readonly static string NextButton = "//button[@value='Next']";

        public readonly static string Password = "//input[@id='password']";

        public readonly static string LogIn = "//button[@value='Login']";

        public readonly static string CompletePurchase = "//button[@data-id='payment-submit-btn']";

        public readonly static string PaymentAuthorizing = "//div[@class='loaderContainer']";

        public readonly static string ErrorMessage = "//div[@class='errorMessageConatiner']";

        public readonly static string OTPIFrame2 = "//iframe[@title='third-party-bank-content']";

        public readonly static string EmbeddedPaymentIframe = "//iframe[@name='paymentFrame']";

        public readonly static string OTPIFrame3 = "//iframe[contains(@id,'cardinal-stepUpIframe')]";

        public readonly static string OTPInput = "//input[@name='challengeDataEntry']";

        public readonly static string OTPSubmitButton = "//input[@value='SUBMIT']";
    }
}

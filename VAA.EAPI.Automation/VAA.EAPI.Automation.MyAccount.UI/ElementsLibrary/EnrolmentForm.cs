namespace VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary
{
    public class EnrolmentForm
    {
        public readonly static string validationMessage = "//h2[text()='Verification successful']";

        public readonly static string firstNameField = "//input[@id='firstName']";

        public readonly static string lastNameField = "//input[@id='lastName']";

        public readonly static string dateOfBirth = "//input[@id='dateOfBirth']";

        public readonly static string gender = "//label[contains(text(),'gender')]";

        public readonly static string genderRadioButton = "//label[contains(text(),'gender')]//parent::div//input";

        public readonly static string country = "//select[@name='country']//option[contains(text(),'country')]";

        public readonly static string countryDropDown = "//select[@name='country']";

        public readonly static string houseNumber = "//input[@id='addressLine1']";

        public readonly static string postCode = "//input[@id='postcode']";

        public readonly static string findAddressButton = "//span[contains(text(),'Find address')]//parent::button";

        public readonly static string findAddressResult = "//p[contains(text(),'postCode')]";

        public readonly static string selectAddress = "//*[@class='css-1h0rqgp']/ul/p[1]/p";

        public readonly static string countryCode = "//select[@name='countryCode']";

        public readonly static string phoneNumber = "//input[@id='phoneNumber']";

        public readonly static string password = "//input[@id='password']";

        public readonly static string flyingClubNewsPreference = "//input[@id='FC']";

        public readonly static string virginRedPreference = "//input[@id='VR']";

        public readonly static string termsAndCondition = "//input[@id='TNC']";

        public readonly static string createAccountButton = "//span[text()='Create my account']//parent::button";

        public readonly static string successMessage = "//p[contains(text(),'Here’s your Flying Club number')]";
    }
}

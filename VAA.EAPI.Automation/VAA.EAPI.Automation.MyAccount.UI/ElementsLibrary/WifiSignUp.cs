namespace VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary
{
    public class WifiSignUp
    {
        // TODO: Update XPaths from the actual WiFi portal sign up page
        public static readonly string firstNameField = "//input[@name= 'firstName']";
        public static readonly string lastNameField = "//input[@name= 'lastName']";
        public static readonly string dateOfBirthField = "//input[@name= 'dateOfBirth']";
        public static readonly string nextButton = "//button[text() = 'Next']";
        public static readonly string emailField = "//input[@name = 'email']";
        public static readonly string confirmButton = "//button[text() = 'Confirm']";
        public static readonly string connectNow = "//button[text() = 'Connect now']";
        public static readonly string connectedMessage= "//h1[text() = \"You're connected\"]";

        // Validation messages
        public static readonly string firstNameInvalidError = "//*[contains(text(), 'First name contains invalid characters')]";
        public static readonly string firstNameTooLongError = "//*[contains(text(), 'First name must be less than 15 characters')]";
        public static readonly string firstNameRequiredError = "//*[contains(text(), 'First name is required')]";
        public static readonly string lastNameInvalidError = "//*[contains(text(), 'Last name contains invalid characters')]";
        public static readonly string lastNameTooLongError = "//*[contains(text(), 'Last name must be less than 20 characters')]";
        public static readonly string lastNameRequiredError = "//*[contains(text(), 'Last name is required')]";
        public static readonly string dobInvalidError = "//*[contains(text(), 'Enter a valid date')]";
        public static readonly string dobRequiredError = "//*[contains(text(), 'Date of birth is required')]";
    }
}

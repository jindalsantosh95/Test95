namespace VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary
{
    public class WifiSignIn
    {
        // TODO: Update XPaths from the actual WiFi portal page
        public static readonly string yesIAgreeButton = "//*[@id='ensAcceptAll']";
        public static readonly string emailOrLoyaltyAccountNumberField = "//input[@name='email']";
        public static readonly string passwordField = "//input[@name='password']";
        public static readonly string loginButton = "//button[text() = 'Log in']";
        public static readonly string joinNowButton = "//button[text() = 'Join now']";
        public static readonly string forgotPasswordLink = "//button[text() ='Forgot password']";
        public static readonly string connectedMessage = "//h1[text() = \"You're connected\"]";

        // Validation messages
        public static readonly string emailRequiredError = "//*[contains(text(), 'Email or Flying Club number is required')]";
        public static readonly string emailInvalidError = "//*[contains(text(), 'Email or Flying Club number contains invalid characters')]";
        public static readonly string passwordRequiredError = "//*[contains(text(), 'Password is required')]";
        public static readonly string accountNotFoundError = "//h2[contains(text(), \"We can't seem to find your account\")]";
    }
}

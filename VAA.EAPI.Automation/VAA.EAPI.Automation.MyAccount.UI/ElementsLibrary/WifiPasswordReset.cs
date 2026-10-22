namespace VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary
{
    public class WifiPasswordReset
    {
        // TODO: Update XPaths from the actual WiFi portal password reset page
        public static readonly string emailField = "//input[@name = 'identifier']";
        public static readonly string nextButton = "//button[text() = 'Next']";
        
        public static readonly string newPasswordField = "//input[@name = 'newPassword']";
        public static readonly string confirmPasswordField = "//input[@name = 'confirmPassword']";
        public static readonly string changePasswordButton = "//button[text() = 'Continue']";
        public static readonly string connectedMessage = "//h1[text() = \"You're connected\"]";

        // Password validation rules — each rule is an <li> containing a tick icon and rule text.
        // When the rule is MET, the tick span loses 'text-transparent' and gains a visible colour.
        // When UNMET, the tick span has 'text-transparent'.
        // We locate each rule by its text, then check the sibling tick's class to determine state.
        public static readonly string passwordLengthRule = "//li[.//span[contains(text(), '8-20 characters')]]";
        public static readonly string passwordNumberRule = "//li[.//span[contains(text(), '1 number')]]";
        public static readonly string passwordUppercaseRule = "//li[.//span[contains(text(), '1 upper case letter')]]";
        public static readonly string passwordLowercaseRule = "//li[.//span[contains(text(), '1 lower case letter')]]";
        public static readonly string passwordSpecialCharsRule = "//li[.//span[contains(text(), 'No more than 3 special characters')]]";
        public static readonly string passwordForbiddenCharsRule = "//li[.//span[contains(text(), '@')]]";
        public static readonly string passwordsMatchRule = "//li[.//span[contains(text(), 'Passwords must match')]]";

        // XPath suffix to find the tick icon within a rule <li>
        public static readonly string tickIcon = ".//span[contains(@class, 'rounded-full')]";
    }
}

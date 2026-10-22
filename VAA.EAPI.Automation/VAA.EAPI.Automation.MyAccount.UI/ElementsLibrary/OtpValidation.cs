namespace VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary
{
    public class OtpValidation
    {
        public readonly static string otpField = "//input[@id='otpCode']";

        public readonly static string successMessage = "//h2[text()='We’ve sent a 6 digit code to:']";

        public readonly static string verifyButton = "//button[@type='submit']";
    }
}

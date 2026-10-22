namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom.ElementLibrary
{
    public class LoyaltyLoginPageElements
    {
        public readonly static string CookieAgree = "//*[@id='ensAcceptAll']";
        public readonly static string EmailOrFlyingClubNumber = "//*[@id='signInName']";
        public readonly static string Password = "//*[@id='password']";
        public readonly static string Continue = "//*[@id='continue']";
        public readonly static string SendButton = "//*[@id='readOnlyEmail_ver_but_send']";
        public readonly static string VerificationCode = "//*[@id='readOnlyEmail_ver_input']";
        public readonly static string VerifyCodeButton = "//*[@id='readOnlyEmail_ver_but_verify']";

    }

    public static class ElementHelper
    {
        public static string GetElement(string element, int index)
        {
            return string.Format(element, index);
        }

        public static string UpdateXpath(string baseXpath, string newId)
        {
            return string.Format(baseXpath, newId);
        }
    }
}
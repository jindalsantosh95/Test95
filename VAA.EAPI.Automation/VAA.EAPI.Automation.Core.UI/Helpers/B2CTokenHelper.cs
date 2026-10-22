using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;

namespace VAA.EAPI.Automation.Core.UI.Helpers
{
    public static class B2CTokenHelper
    {
        public static string endpoint = ConfigReader.Get("AzureB2CLoginToGenerateTheToken");

        public static string GenerateLoginB2CToken(string fcNumber, string password)
        {
            if (endpoint.IsNullOrWhiteSpace())
                throw new InvalidOperationException("AzureB2CLoginToGenerateTheToken is required to generate a B2C token.");

            IWebDriver webDriver = null;

            try
            {
                webDriver = GetBrowser(BrowserType.Chrome, endpoint, headless: true);

                if (webDriver.WaitForElementToBeVisible("//*[@id='ensAcceptAll']", 5) != null)
                    webDriver.FindElementAndClick("//*[@id='ensAcceptAll']", "Yes, I Agree");

                webDriver.FindElementAndEnterText("//input[@id='signInName']", fcNumber, "Enter FC Number from test case");

                webDriver.FindElementAndEnterText("//input[@id='password']", password, "Enter Password from test case");

                webDriver.FindElementAndClick("//button[@id='continue']", "Click on continue button");

                webDriver.WaitForElementToBeVisible("//div[@id='encodedToken']/span[@class='jwtHeader']");

                string tokenHeader = webDriver.GetElementText("//div[@id='encodedToken']/span[@class='jwtHeader']");

                string tokenClaims = webDriver.GetElementText("//div[@id='encodedToken']/span[@class='jwtClaims']");

                string tokenSignature = webDriver.GetElementText("//div[@id='encodedToken']/span[@class='jwtSignature']");

                return tokenHeader + "." + tokenClaims + "." + tokenSignature;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to generate a B2C token using the configured loyalty test account.", exception);
            }
            finally
            {
                webDriver?.DriverCleanUp();
            }
        }
    }
}

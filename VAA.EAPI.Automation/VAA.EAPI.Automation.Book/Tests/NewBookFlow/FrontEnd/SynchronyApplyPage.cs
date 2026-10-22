using OpenQA.Selenium;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd
{
    public class SynchronyApplyPage : IDisposable
    {
        private IWebDriver webDriver;
        private SynchronyApplyPageObjects synchronyApplyPageObjects;

        public SynchronyApplyPage
           (string url, BrowserType browserType = BrowserType.Chrome)
        {
            bool headless = ConfigReader.Get("SynchronyApplyHeadless", true);
            webDriver = GetBrowser(browserType, url, headless, false);
            synchronyApplyPageObjects = new SynchronyApplyPageObjects(webDriver);
        }

        public void ApplySynchronyVirginRedRewardsMasterCard(TestConfig testConfig)
        {
            synchronyApplyPageObjects.WaitForAcceptAllCookiesAndClick();
            synchronyApplyPageObjects.EnterLast4DigitsSSN(testConfig.Book.CoBrand.Last4DigitsSSN);
            synchronyApplyPageObjects.EnterMobileNumber(testConfig.Book.CoBrand.MobileNumber);
            synchronyApplyPageObjects.ClickOnContinueButton();
            synchronyApplyPageObjects.ClickOnEditInfoLink();
            synchronyApplyPageObjects.EnterFirstName(testConfig.Book.CoBrand.FirstName);
            synchronyApplyPageObjects.EnterLastName(testConfig.Book.CoBrand.LastName);
            synchronyApplyPageObjects.EnterStreetAddress(testConfig.Book.CoBrand.StreetAddress);
            synchronyApplyPageObjects.EnterCityAndState(testConfig.Book.CoBrand.CityAndState);
            synchronyApplyPageObjects.EnterZipCode(testConfig.Book.CoBrand.ZipCode);
            synchronyApplyPageObjects.EnterSSNNumber(testConfig.Book.CoBrand.SsnNumber);
            synchronyApplyPageObjects.EnterDateOfBirth(testConfig.Book.CoBrand.DateOfBirth);
            synchronyApplyPageObjects.EnterNetIncome(testConfig.Book.CoBrand.NetIncome);
            synchronyApplyPageObjects.EnterPrimaryPhoneNumber(testConfig.Book.CoBrand.MobileNumber);
            synchronyApplyPageObjects.ClickOnHomePhoneRadioButton();
            synchronyApplyPageObjects.ClickOnNextContinueButton();
            synchronyApplyPageObjects.ClickOnContinueToApplyButton();
            synchronyApplyPageObjects.ClickOnTermsAndConditionsCheckBox();
            synchronyApplyPageObjects.ClickOnAgreeAndSubmitButton();
        }

        public void Dispose()
        {
            // Close browser
            webDriver.DriverCleanUp();
            webDriver.Dispose();
            webDriver = null;
        }
    }
}

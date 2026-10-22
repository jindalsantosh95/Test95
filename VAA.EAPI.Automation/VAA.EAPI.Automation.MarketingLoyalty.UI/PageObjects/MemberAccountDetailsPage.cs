using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class MemberAccountDetailsPage
    {
        private readonly IWebDriver _webDriver;

        public MemberAccountDetailsPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public void ClickOnAccountDetailsTab()
        {

            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(MemberAccountDetails.AccountDetailsTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(MemberAccountDetails.AccountDetailsTab, "AccountDetails Tab", 60);

            _webDriver.WaitForElementToBePresent(MemberAccountDetails.AccountDetailsScreenLoadStatus);
        }

        public void ClickOnRefreshBtn(string tierCode)
        {
            if (tierCode == "Red")
                _webDriver.FindElementAndClick(RedTierMemberAccountDetails.RefreshRedMemberBtn, "Refresh Btn");

            if (tierCode == "Silver")
                _webDriver.FindElementAndClick(SilverTierMemberAccountDetails.RefreshSilverMemberBtn, "Refresh Btn");

            if (tierCode == "Gold")
                _webDriver.FindElementAndClick(GoldTierMemberAccountDetails.RefreshGoldMemberBtn, "Refresh Btn");

            _webDriver.WaitForElementToBePresent(MemberAccountDetails.RefreshRibbonLink, 10);

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);

        }

    }
}

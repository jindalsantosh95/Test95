using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class AuditHistoryPage
    {
        private readonly IWebDriver _webDriver;

        public AuditHistoryPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnAuditHistoryTab(ExtentTest logger)
        {
            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(AuditHistory.AuditHistoryTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryTab, "Audit History Tab", 60);

            _webDriver.WaitForElementToBePresent(AuditHistory.AuditHistoryScreenLoadStatus, 10);

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Alert On Audit Screen", 5);

            logger.Log(Status.Info, "Clicked On Audit History Tab");
        }

        public void ClickOnAuditHistoryEmailTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryEmailTab, "Email Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

        }

        public void ClickOnAuditHistoryMemberProfileTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryMemberProfileTab, "Member Profile Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void ClickOnAuditHistoryPhoneTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryPhoneTab, "Phone Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void ClickOnAuditHistoryAddressTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryAddressTab, "Address Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void ClickOnAuditHistoryPreferencesTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryPreferencesTab, "Preferences Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void ClickOnAuditHistoryAwardTab()
        {
            _webDriver.FindElementAndClick(AuditHistory.AuditHistoryAwardTab, "Awards Tab On Audit History Page");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public string GetPopUpMessageContent() => _webDriver.GetElementText(AuditHistory.PopUpMessageContent);

    }
}

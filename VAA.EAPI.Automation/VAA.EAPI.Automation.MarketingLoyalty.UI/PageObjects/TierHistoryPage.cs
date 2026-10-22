using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Reporting;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class TierHistoryPage
    {
        private readonly IWebDriver _webDriver;

        public TierHistoryPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void NavigateToTierHistoryTab(ExtentTest logger)
        {

            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            IWebElement element = _webDriver.FindElement(By.XPath(TierHistory.TierHistoryTab));

            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(TierHistory.TierHistoryTab, "TierHistory Tab");

            _webDriver.TryFindElement(TierHistory.TierHistoryTableRows);

            _webDriver.WaitForElementToBePresent(TierHistory.TierHistoryScreenLoadStatus, 30);

            logger.Log(Status.Info, "Screenshot after clicking on TierHistory  Tab " + CaptureScreenShot.EmbedImage(_webDriver));
        }

        public void ClickOnMenuBarOnTierHistoryPage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(TierHistory.TierHistoryToMenuBarNavigation, "Menu Bar On Tier History Page");

            logger.Log(Status.Info, "Clicked on Menu Bar from Tier History Page");

        }

        public Dictionary<string, string> GetActiveTierDetails()
        {
            Dictionary<string, string> activeTierDetails = new()
            {
                {"TierCode","" },
                {"StartDate","" },
                {"EndDate","" },
                {"Active","" },
                {"IssueReason","" },
                {"UserCreated","" }

            };

            IReadOnlyCollection<IWebElement> webElements = _webDriver.FindElements(By.XPath(TierHistory.TierHistoryTableRows));

            foreach (IWebElement element in webElements)
            {
                bool isActiveTier = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value").Equals("Y");

                if (isActiveTier)
                {
                    activeTierDetails["TierCode"] = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                    activeTierDetails["StartDate"] = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                    activeTierDetails["EndDate"] = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                    activeTierDetails["IssueReason"] = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                    activeTierDetails["UserCreated"] = element.FindElement(By.XPath("td[9]/div/div/span")).GetAttribute("value");

                    activeTierDetails["Active"] = "Y";

                    break;
                }

            }

            return activeTierDetails;
        }

        public void ClickOnReviseButton() => _webDriver.FindElementAndClick(TierHistory.ReviseTierButton, "Revise Tier Button On Tier History Page");

        public void SelectNewTier(string tierCode)
        {
            _webDriver.FindElementAndClick(TierHistory.ClickOnTierListDropdown, "Tier List Dropdown On Revise Pop Up Screen");

            _webDriver.FindElementAndClick(TierHistory.SelectNewTier.Replace("TierCode", tierCode),
                "Selection of New Tier from List of Tiers Dropdown");

        }

        public void SelectTierIssueReason(string issueReason)
        {
            _webDriver.FindElementAndClick(TierHistory.ClickOnIssueReasonDropdown, "Issue Reason Dropdown On Revise Pop Up Screen");

            _webDriver.FindElementAndClick(TierHistory.SelectIssueReason.Replace("IssueReason", issueReason),
                "Selection of IssueReason from List of Tiers Dropdown");
        }

        public void ClickOnReviseMemberTierButton() => _webDriver.FindElementAndClick(TierHistory.MemberTierReviseButton,
            "Member Tier Revise Button On Revise Pop Up Screen");

        public void ClickOnCancelMemberTierButton() => _webDriver.FindElementAndClick(TierHistory.CancelMemberTierReviseButton,
            "Cancel Member Tier Revise Button On Revise Pop Up Screen");

        public void RefreshTierHistoryPage() => _webDriver.FindElementAndClick(TierHistory.RefreshTierHistoryPageButton,
            "Refresh Button on Tier History Page");

        public void ClickOnProcessTierChangeButton() => _webDriver.FindElementAndClick(TierHistory.ProcessTierChangeButton,
            "Process Tier Change Button On Tier History Page");

        public void ClickOnManualTierChangeButton() => _webDriver.FindElementAndClick(TierHistory.ManualTierChangeButton,
            "Manual Tier Change Button On Tier History Page");

        public string GetPopUpMessageContent() => _webDriver.GetElementText(TierHistory.PopUpMessageContent, 10);

        public void ClosePopUp() => _webDriver.FindElementAndClick(TierHistory.ClosePopUp, "Pop Up Close Button");

    }
}

using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class FinancialPage
    {
        private readonly IWebDriver _webDriver;

        public FinancialPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnFinancialTab(ExtentTest logger)
        {
            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(Financial.FinancialTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(Financial.FinancialTab, "Financial Tab", 60);

            _webDriver.WaitForElementToBePresent(Financial.FinancialScreenLoadStatus, 10);

            logger.Log(Status.Info, "Clicked On Financial Tab");
        }

        public void ClickOnAuditAlertPopUpIfPresent()
        {
            _webDriver.TryFindElementAndClick(Financial.AlertCloseButton, "Alert On Financial Screen", 10);
        }

        public void ClickOnAddCard()
        {
            _webDriver.FindElementAndClick(Financial.AddCardIcon, "Add Card Icon");
        }

        public void SelectCardFromDropDown(string cardType)
        {
            _webDriver.TryFindElementAndClick(Financial.AddCardDropdown, "AddCard dropdown", 5);

            _webDriver.TryFindElementAndClick(Financial.SelectCardType.Replace("cardType", cardType), $"Card Type selected is {cardType}", 5);

            _webDriver.WaitForElementToBeClickable(Financial.SubmitButton);

            _webDriver.TryFindElementAndClick(Financial.SubmitButton, "Submit Button", 10);

        }

        public void ClickOnaddCardConfirmationPopup()
        {
            _webDriver.TryFindElementAndClick(Financial.AddMemberCardPopUpCloseIcon, "Add member card PopUp");
        }

        public void ClickAuditIcon()
        {
            _webDriver.FindElementAndClick(Financial.AuditHistoryIcon, "Audit history icon on the Card Record");
        }

        public void CloseAuditHistoryWindow()
        {
            _webDriver.FindElementAndClick(Financial.CardAuditHistoryWindowCloseButton, "Audit history Window Close Button");
        }

        public void NavigateToMemberDetailsPage()
        {
            _webDriver.FindElementAndClick(Financial.FinancialPageToMenuBarNavigation, "Menu Bar [BreadCrumb] button from Financial Page");

            _webDriver.FindElementAndClick(Financial.MemberDetailPageTab, "Member Detail Tab on the List of Tabs");
        }

    }
}

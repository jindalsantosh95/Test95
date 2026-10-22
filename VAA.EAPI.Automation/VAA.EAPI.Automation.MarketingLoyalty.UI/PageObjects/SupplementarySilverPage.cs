using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Reporting;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class SupplementarySilverPage
    {
        private readonly IWebDriver _webDriver;

        private MemberSearchPage memberSearchPage;

        public SupplementarySilverPage(IWebDriver driver)
        {
            _webDriver = driver;

            memberSearchPage = new MemberSearchPage(_webDriver);
        }

        public void ClickOnMenuBarOnMemberProfilePage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar On Member Detail Page");

            logger.Log(Status.Info, "Clicked on Menu Bar From Member Details Screen");
        }

        public void ClickOnRelationShipTab(ExtentTest logger)
        {
            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(SupplementarySilver.RelationshipTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(SupplementarySilver.RelationshipTab, "RelationShip Tab");

            _webDriver.TryFindElement(SupplementarySilver.RelationShipHistoryHeader);

            logger.Log(Status.Info, "Screenshot after clicking on Relationship Tab " + CaptureScreenShot.EmbedImage(_webDriver));
        }

        public void ClickOnSupplementarySilverButton(ExtentTest logger)
        {

            _webDriver.FindElementAndClick(SupplementarySilver.SuppSilverTab, "Supplementary Silver Button on Relationship Page");

            _webDriver.WaitForElementToBePresent(SupplementarySilver.SuppSilverScreenLoadStatus);

            _webDriver.TryFindElementAndClick(SupplementarySilver.LinkButtonPopUpCloseButton, "No Audit Data Alert Pop Up", 10);

            logger.Log(Status.Info, "Screenshot after clicking on Supp Silver Button " + CaptureScreenShot.EmbedImage(_webDriver));

        }

        public bool IsLinkButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(SupplementarySilver.SuppSilverLinkButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {SupplementarySilver.SuppSilverLinkButton}.");
            }
            //returns true if button is disabled
        }

        public bool IsDeLinkButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(SupplementarySilver.SuppSilverDelinkButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {SupplementarySilver.SuppSilverDelinkButton}.");
            }
            //returns true if button is disabled
        }

        public void ClickOnLinkButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.SuppSilverLinkButton, "Supp Silver Link Button");

            logger.Log(Status.Info, "Clicked on Link Button");
        }

        public void ClickOnDeLinkButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.SuppSilverDelinkButton, "Supp Silver DeLink Button");

            logger.Log(Status.Info, "Clicked on DeLink Button");
        }

        public string GetTextFromSuppSilverHistorySection() => _webDriver.GetElementText(SupplementarySilver.SuppSilverHistoryMessage);

        public string GetTextFromLinkAndDelinkButtonPopUpScreen() => _webDriver.GetElementText(SupplementarySilver.LinkDelinkButtonPopUpScreenText);

        public bool WaitForTextToBeDisplayedAfterSuccessfulDelinking(string message)
        {
            WebElement ele = (WebElement)_webDriver.WaitForElementToBeVisible(SupplementarySilver.LinkDelinkButtonPopUpScreenText + $"[contains(text(),'{message}')]");

            return ele is not null;
        }

        public void EnterChildFCNumberAndSearch(ExtentTest logger, string childFCNumber)
        {
            _webDriver.FindElementAndEnterText(SupplementarySilver.ChildFCNumberSearchTextBox, childFCNumber, "Child FC Number Search Text Box", 10);

            _webDriver.FindElementAndClick(SupplementarySilver.ChildFCNumberSearchButton, "Child FC Details Search Button", 15);

            logger.Log(Status.Info, "Entered Child FC Number and Clicked on Search Button to fetch Child Member Details");
        }

        public bool IsValidateAndLinkButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(SupplementarySilver.ValidateAndLinkButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {SupplementarySilver.ValidateAndLinkButton}.");
            }
            //returns true if button is disabled
        }

        public void ClickOnValidateAndLinkButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.ValidateAndLinkButton, "ValidateAndLink Button");

            logger.Log(Status.Info, "Clicked on ValidateAndLink Button");
        }

        public string GetTextFromChildFCSearchPopUpScreen() => _webDriver.GetElementText(SupplementarySilver.TextMessageOnChildFCSearchScreen);

        public string GetTextFromLinkedAccountsSection(string locator)
        {
            try
            {
                return _webDriver.GetElementText(locator);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public void CloseThePopUp(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.LinkButtonPopUpCloseButton, "Supp Silver Pop Up Button");

            logger.Log(Status.Info, "Closed the Pop Up");

            _webDriver.WaitForElementToBePresent(SupplementarySilver.SuppSilverScreenLoadStatus);
        }

        public void ClickOnDeLinkYesButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.DelinkConfirmYesButton, "DeLink Yes Button");

            logger.Log(Status.Info, "Clicked on DeLink Yes Button");
        }
        public void ClickOnDeLinkNoButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(SupplementarySilver.DelinkConfirmNoButton, "DeLink No Button");

            logger.Log(Status.Info, "Clicked on DeLink No Button");
        }

        public string GetTextFromAuditSection(string locator)
        {
            try
            {
                return _webDriver.GetElementText(locator);
            }

            catch
            {
                return "";
            }
        }

        public void ClickOnRefreshButton()
        {
            _webDriver.FindElementAndClick(SupplementarySilver.RefreshButton, "Refresh Button On Supp Silver Page");

            _webDriver.WaitForElementToBePresent(SupplementarySilver.SuppSilverScreenLoadStatus);
        }

        public void ClickOnOverrideCheckbox() => _webDriver.FindElementAndClick(Household.MemberOverrideCheckBoxActive, "Override checkbox");

    }
}

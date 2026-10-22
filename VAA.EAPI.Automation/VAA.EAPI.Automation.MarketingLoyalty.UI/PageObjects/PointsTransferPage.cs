using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Reporting;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class PointsTransferPage
    {
        private readonly IWebDriver _webDriver;

        public PointsTransferPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void NavigateToPointsTransferPage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            IWebElement element = _webDriver.FindElement(By.XPath(PointsTransfer.PointsTransferTab));

            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(PointsTransfer.PointsTransferTab, "PointsTransfer Tab");

            _webDriver.TryFindElement(PointsTransfer.RecipientAccNmbrTextBox);

            logger.Log(Status.Info, "Screenshot after clicking on Points Transfer  Tab " + CaptureScreenShot.EmbedImage(_webDriver));
        }

        public void EnterRecipientMemberNumber(string recipientMemberNumber) =>
            _webDriver.FindElementAndEnterText(PointsTransfer.RecipientAccNmbrTextBox, recipientMemberNumber, "Recipient Account Number Text Box");


        public void EnterNoOfPointsToTransfer(string noOfPoints) =>
            _webDriver.FindElementAndEnterText(PointsTransfer.PointsTransferValueTxtBox, noOfPoints, "Points Value Text Box");


        public void ClickOnSearchRecipientAccountDetails() => _webDriver.FindElementAndClick(PointsTransfer.FindRecipientSrchButton, "Search Recipient Button");

        public void ClickOnApplyPointsTransferButton() => _webDriver.FindElementAndClick(PointsTransfer.ApplyPointsTransferButton, "Apply Points Transfer Button");

        public void ClickOnClearAllButton() => _webDriver.FindElementAndClick(PointsTransfer.ClearallBtn, "ClearAll Details Button");

        public string GetSuccessPopUPMessage() => _webDriver.GetElementText(PointsTransfer.SuccessPopUpContent, 60);

        public void CloseSuccessPopUp() => _webDriver.FindElementAndClick(PointsTransfer.SuccessPopUpCloseButton, "Success PopUp Close Button");

        public bool IsApplyPointsTransferButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(PointsTransfer.ApplyPointsTransferButton, "disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }
    }
}

using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Reporting;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class MyOffersPage
    {
        private readonly IWebDriver _webDriver;

        public MyOffersPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void NavigateToMyOffersPage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(MyOffers.MyOffersTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(MyOffers.MyOffersTab, "MyOffers Tab", 60);

            _webDriver.WaitForElementToBeVisible(MyOffers.VoucherAddButton);

            logger.Log(Status.Info, "Screenshot after clicking on MyOffers Tab " + CaptureScreenShot.EmbedImage(_webDriver));
        }

        public void ClickOnAddButtonOnMyOffersPage() => _webDriver.FindElementAndClick(MyOffers.VoucherAddButton, "Add Voucher Button");

        public void ClickOnUpdateButtonOnMyOffersPage() => _webDriver.FindElementAndClick(MyOffers.VoucherUpdateVoucherButton, "Update Voucher Button");

        public void ClickOnClaimButtonOnMyOffersPage() => _webDriver.FindElementAndClick(MyOffers.VoucherClaimButton, "Claim Voucher Button");

        public void ClickOnGiftButtonOnMyOffersPage() => _webDriver.FindElementAndClick(MyOffers.VoucherGiftButtonContainer, "Gift Voucher Button");

        public void ClickOnCancelGiftButtonOnMyOffersPage() => _webDriver.FindElementAndClick(MyOffers.VoucherCancelGiftButton, "Cancel Gifted Voucher Button");

        public void SelectTheVoucherFromTheDropdown(string voucherName)
        {
            _webDriver.FindElementAndClick(MyOffers.VoucherTypeDropDown, "Voucher Type Dropdown Button");

            _webDriver.FindElementAndClick(MyOffers.SelectVoucher.Replace("VoucherName", voucherName), "Selection of the Voucher from the Dropdown");
        }

        public void ClickOnAddVoucherOnAddPopUp() => _webDriver.FindElementAndClick(MyOffers.AddButtonOnPopUp, "Add Button On Voucher Pop Up");

        public string GetPopUpMessageContent() => _webDriver.GetElementText(MyOffers.PopUpMessageContent, 10);

        public void ClosePopUp() => _webDriver.FindElementAndClick(MyOffers.PopUpCloseButton, "Close Button On Pop Up", 10);

        public void NavigateBackToMyOffersPageFromAuditPage()
        {
            _webDriver.FindElementAndClick(MyOffers.NavigateBackToMyOffersPageButton, "Back Button On Audit Page", 10);

            _webDriver.WaitForElementToBeVisible(MyOffers.VoucherAddButton);

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);
        }

    }
}

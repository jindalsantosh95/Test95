using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class ManualAdjustmentPage
    {
        private readonly IWebDriver _webDriver;

        public ManualAdjustmentPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnMenuBarOnMemberProfilePage()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            _webDriver.FindElementAndClick(ManualAdjustment.ManualAdjustmentTab, "ManualAdjustment Tab");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void EnterPartnerCode(string partnerCode)
        {
            if (!string.IsNullOrEmpty(partnerCode))
            {
                _webDriver.FindElementAndClick(ManualAdjustment.PartnerCodeDropdown,
                    "Partner Code dropdown");

                _webDriver.FindElementAndClick(ManualAdjustment.PartnerCodeDropdownSelection.Replace("partnerCode", partnerCode),
                    $"partner code {partnerCode} from the dropdown list");
            }

        }

        public void EnterPoints(string points)
        {
            _webDriver.FindElementAndEnterText(ManualAdjustment.PartnerPointsTextbox, points,
                "Points Text Box");
        }

        public void EnterTierPoints(string tierPoints)
        {
            _webDriver.FindElementAndEnterText(ManualAdjustment.PartnerTierpointsTextbox, tierPoints,
                "TierPoints Text Box");

        }

        public void EnterProduct(string product)
        {
            if (!string.IsNullOrEmpty(product))
            {
                _webDriver.FindElementAndClick(ManualAdjustment.ProductDropdown,
                    "Product dropdown");

                _webDriver.FindElementAndClick(ManualAdjustment.ProductDropdownSelection.Replace("productName", product),
                    $"Product {product} from the dropdown list");
            }
        }

        public void EnterActivityDate(string activityDate)
        {
            _webDriver.FindElementAndEnterText(ManualAdjustment.ActivityDateTextbox, activityDate,
                "ActivityDate Text Box");
        }

        public void EnterComments(string comments)
        {
            _webDriver.FindElementAndEnterText(ManualAdjustment.CommentsTextbox, comments,
                "Comments Text Box");
        }


        public void ClickOnApplyManualAdjustmentButton()
        {
            _webDriver.FindElementAndClick(ManualAdjustment.ApplyManualAdjBtn,
                "Apply Manual adjustment button the screen");

        }

        public string GetPopUpMessageContent()
        {
            _webDriver.FindElement(ManualAdjustment.SuccessPopupMsg, "Manual Adjustment Pop Up Message", 120);

            return _webDriver.GetAttributeTextValue(ManualAdjustment.SuccessPopupMsg, "textContent");
        }

        public bool IsApplyManualAdjustmentBuuttonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(ManualAdjustment.ApplyManualAdjBtn, "disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }
    }
}



using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class PromotionManagementPage
    {
        private readonly IWebDriver _webDriver;

        public PromotionManagementPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnPromotionManagementLink(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(PromotionManagement.PromotionsSearchTab, "PromotionSearch Tab", 60);

            _webDriver.WaitForElementToBeVisible(PromotionManagement.PromotionNameTextBox);

            logger.Log(Status.Info, "Clicked on Promotion Management Tab on the Home Page");

        }

        public void EnterThePromotionNameIntoPromotionNameTextBox(string promotionName)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionNameTextBox, promotionName, "Promotion Name Text Box");
        }

        public void EnterAndSelectThePartnerNameFromThePartnerNameDropdown(string partnerName)
        {
            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameDropDown, "Partner Name Dropdown");

            _webDriver.FindElementAndEnterText(PromotionManagement.PartnerNameTextBox, partnerName, "PartnerName TextBox");

            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameSelect + $"[text()='{partnerName}']", "");


        }

        public void EnterStartDateFrom(string startDateFrom)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.FromStartDate, startDateFrom, "StartDateFrom TextBox");

        }

        public void EnterStartDateTo(string startDateTo)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.ToStartDate, startDateTo, "StartDateTo TextBox");

        }

        public void EnterEndDateFrom(string endDateFrom)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.FromEndDate, endDateFrom, "EndDateFrom TextBox");
        }

        public void EnterEndDateTo(string endDateTo)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.ToEndDate, endDateTo, "EndDateTo TextBox");
        }

        public void ClickOnActiveFlagCheckBox()
        {
            _webDriver.FindElementAndClick(PromotionManagement.ActiveFlagCheckBox, "Active Flag CheckBox");
        }

        public void ClickOnSearchButton()
        {
            _webDriver.FindElementAndClick(PromotionManagement.SearchButton, "Search Button");
        }

        public void ClickOnClearButton()
        {
            _webDriver.FindElementAndClick(PromotionManagement.ClearButton, "Clear Button");
        }

        public void UpdatePromotionName(string promotionName)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionNameUpdateTextBox, Keys.Clear,
                "Promotion Name Text Box on Promotion Details Page");

            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionNameUpdateTextBox, promotionName,
                "Promotion Name Text Box on Promotion Details Page");

        }

        public void UpdatePartnerName(string partnerName)
        {
            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameUpdateDropDown,
                "PartnerName Dropdown On Promotion Details Page");

            _webDriver.FindElementAndClick(PromotionManagement.PartnerSelectionRemoval,
                "Existing Partner Name Removal On Promotion Details Page");

            _webDriver.FindElementAndEnterText(PromotionManagement.PartnerNameUpdateTextBox, partnerName,
                "PartnerName Text Box on Promotion Details Page");

            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameUpdateSelect.Replace("partnerName", partnerName),
                "Selecting the New Partner Name From Dropdown");
        }

        public void UpdateStartDate(string startDate)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionStartDateUpdateTextBox, Keys.Clear,
                "Promotion StartDate Text Box on Promotion Details Page");

            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionStartDateUpdateTextBox, startDate,
                "Promotion StartDate Text Box on Promotion Details Page");

        }

        public void UpdateEndDate(string endDate)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionEndDateUpdateTextBox, Keys.Clear,
                "Promotion EndDate Text Box on Promotion Details Page");

            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionEndDateUpdateTextBox, endDate,
                "Promotion EndDate Text Box on Promotion Details Page");

        }

        public void UpdateActiveFlag()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PromotionActiveFlagUpdate,
                "Promotion Active Check Box on Promotion Details Page");

        }

        public void UpdatePromotionDetailsByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PromotionUpdateSaveButton,
                "Promotion Update Save Button on Promotion Details Page");
        }

        public string UpdatePromotionPopUpContent() =>
             _webDriver.GetElementText(PromotionManagement.PopUpMessageContentUpdate);

        public void ClosePopUpOnUpdatePromotionPage()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate, "Update Pop Up Close Button");

            _webDriver.WaitForElementToBeVisible("//td[2]", 15);

        }

        public void ClearThePromotionSearchResults()
        {
            _webDriver.FindElementAndClick(PromotionManagement.ClearButton, "Clear Button On Promotion Search Page");
        }

        public void ClickOnPromotionCreateButton()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PromotionCreateButton,
                "New Promotion Create Button on Search Promotion Page");
        }

        public void PromotionCreate_EnterPromotionName(string promotionName)
        {
            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionNameCreateTextBox, promotionName,
                " Promotion Name Text Box on Create  Promotion Page");

        }

        public void PromotionCreate_EnterPartnerName(string partnerName)
        {
            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameCreateDropDown,
                "PartnerName Dropdown On Promotion Create Page");

            _webDriver.FindElementAndEnterText(PromotionManagement.PartnerNameCreateTextBox, partnerName,
                "PartnerName Text Box on Promotion Create Page");

            _webDriver.FindElementAndClick(PromotionManagement.PartnerNameUpdateSelect.Replace("partnerName", partnerName),
                "Selecting the New Partner Name From Dropdown");

        }

        public void PromotionCreate_EnterStartDate(string startDate)
        {

            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionStartDateCreateTextBox, startDate,
                "Promotion StartDate Text Box on Promotion Create Page");

        }

        public void PromotionCreate_EnterEndDate(string endDate)
        {

            _webDriver.FindElementAndEnterText(PromotionManagement.PromotionEndDateCreateTextBox, endDate,
                "Promotion EndDate Text Box on Promotion Create Page");

        }

        public void PromotionCreate_SelectActiveFlag()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PromotionActiveFlagCreateCheckBox,
                "Promotion Active Check Box on Promotion Create Page");

        }

        public void CreatePromotionByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(PromotionManagement.CreateButton,
                "Promotion Create/Save Button On Promotion Create Page");
        }

        public string CreatePromotionPopUpContent() =>
            _webDriver.GetElementText(PromotionManagement.PopUpMessageContent);

        public void ClosePopUpOnCreatePromotionPage()
        {
            _webDriver.FindElementAndClick(PromotionManagement.PopUpCloseButtoncreate,
                "Pop Up Close Button On Promotion Create Page");
        }
    }

}

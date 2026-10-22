using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class CampaignManagementPage
    {
        private readonly IWebDriver _webDriver;

        public CampaignManagementPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnBreadCrum(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(CampaignManagement.BreadCrumIcon, "");

            logger.Log(Status.Info, "Clicked on BreadCrunb Icon ");
        }

        public void ClickOnCampaignManagementLink(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignSearchTab, "CampaignSearch Tab", 60);

            _webDriver.WaitForElementToBePresent(CampaignManagement.CampaignSearchScreenLoadedValidation);

            logger.Log(Status.Info, "Clicked on Campaign Management Tab on the Home Page");

        }

        public void EnterCampaignCode(string campaignCode)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CampaignCodeTextBox, campaignCode,
                "Campaign Code Text Box On Search Page");
        }

        public void EnterCampaignName(string campaignName)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CampaignNameTextBox, campaignName,
                "Campaign Name Text Box On Search Page");
        }

        public void EnterAndSelectTheCampaignTypeFromTheCampaignTypeDropdown(string campaignType)
        {
            _webDriver.FindElementAndClick(CampaignManagement.CamapignTypeDropDown, "Campaign Type Dropdown");

            _webDriver.FindElementAndEnterText(CampaignManagement.CampaignTypeTextBox, campaignType, "CampaignType TextBox");

            _webDriver.FindElementAndClick(CampaignManagement.CampaignTypeSelect + $"[text()='{campaignType}']", "");
        }

        public void EnterStartDateFrom(string startDateFrom)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.StartDateFromBox, startDateFrom,
                "StartDateFrom TextBox On Search Page");

        }

        public void EnterStartDateTo(string startDateTo)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.StartDateToBox, startDateTo,
                "StartDateTo TextBox On Search Page");

        }

        public void EnterEndDateFrom(string endDateFrom)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.EndDateFromBox, endDateFrom,
                "EndDateFrom TextBox On Search Page");
        }

        public void EnterEndDateTo(string endDateTo)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.EndDateToBox, endDateTo,
                "EndDateTo TextBox On Search Page");
        }

        public void ClickOnSearchButton()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignSearchIcon,
                "Click on Search Button On Search Page");
        }

        public void ClickOnClearAllButton()
        {
            _webDriver.FindElementAndClick(CampaignManagement.ClearAllBtn,
                "Click on ClearAll Button On Search Page");
        }

        public string ValidateTotalResultsRetrieved()
        {
            return _webDriver.GetAttributeTextValue(CampaignManagement.TotalResultsText, "textContent");
        }

        public string ClickAndValidateCampaignDetails(string campcode)
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignCodeLink.Replace("CAMPAIGNCODE", campcode), "Click the retrieved Campaign code on screen");

            _webDriver.WaitForElementToBePresent(CampaignManagement.CampaignUpdateScreenLoadedValidation);

            _webDriver.WaitForElementToBePresent(CampaignManagement.UpdateCampaignCodeTextBox);

            return _webDriver.GetAttributeTextValue(CampaignManagement.UpdateCampaignCodeTextBox, "value");

        }

        public void ClickOnRefreshButtonIfEnabled()
        {
            _webDriver.WaitForElementToBePresent(CampaignManagement.RefreshIcon);

            string refreshBtnStatus = _webDriver.GetAttributeTextValue(CampaignManagement.RefreshIcon, "aria-disabled");

            if (refreshBtnStatus != "true")
            {
                _webDriver.FindElementAndClick(CampaignManagement.RefreshIcon, "Refresh Campaign Search Page");
            }
        }

        public string ValidatePromotionName(string promotionName)
        {
            return _webDriver.GetAttributeTextValue(CampaignManagement.PromotionNameLink.Replace("PROMOTIONNAME", promotionName), "value");
        }

        public void ClickOnCampaignCreateButton()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignCreateButton,
                "New Campaign Create Button on Search Campaign Page");
        }

        public void CampaignCreate_EnterCampaignCode(string campaignCode)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CreateCampaignCode, campaignCode,
                "Campaign Code Text Box on Create Campaign Page", 30, true);

        }

        public void CampaignCreate_EnterCampaignName(string campaignName)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CreateCampaignName, campaignName,
                "Campaign Name Text Box on Create Campaign Page");

        }

        public void CampaignCreate_EnterCampaignType(string campaignType)
        {
            _webDriver.FindElementAndClick(CampaignManagement.CreateCampaignType,
                "CampaignType Dropdown On Campaign Create Page");

            _webDriver.FindElementAndClick(CampaignManagement.CreateCampaignTypeOption.Replace("Name", campaignType),
                "Selecting the New CampaignType From Dropdown");

        }

        public void CampaignCreate_EnterCampaignStatus(string campaignStatus)
        {
            _webDriver.FindElementAndClick(CampaignManagement.CreateCampaignStatus,
                "Campaign Status Dropdown On Campaign Create Page");

            _webDriver.FindElementAndClick(CampaignManagement.CreateCampaignStatusOption.Replace("Name", campaignStatus),
                "Selecting the New Status From Dropdown");

        }

        public void CampaignCreate_EnterStartDate(string startDate)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CreateCampaignStartDate, startDate,
                "Campaign StartDate Text Box on Campaign Create Page");
        }

        public void CampaignCreate_EnterEndDate(string endDate)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CreateCampaignEndDate, endDate,
                "Campaign EndDate Text Box on Campaign Create Page");
        }

        public void CampaignCreate_EnterDescription(string description)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.CreateCampaignDescription, description,
                "Campaign Description Text Box on Campaign Create Page");
        }

        public void CampaignCreate_SelectRegistrationRequiredFlag()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CreateCampaignRRFlag,
                "Campaign Registration Required Flag CheckBox on Campaign Create Page");
        }

        public void CreateCampaignByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignUpdateButton,
                "Campaign Create/Save Button On Campaign Create Page");
        }

        public string CreateCampaignPopUpContent() =>
           _webDriver.GetElementText(CampaignManagement.CampaignCreatePopUpMessage);

        public void ClosePopUpOnCreateCampaignPage()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignCreatePopUpMessageCloseButton,
                "Pop Up Close Button On Campaign Create Page");
        }


        public void CampaignUpdate_EnterNewCampaignCode(string campaignCode)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.UpdateCampaignCodeTextBox, campaignCode,
              "Campaign Code Text Box on Update Campaign Page");
        }

        public void CampaignUpdate_EnterNewCampaignName(string campaignName)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.UpdateCampaignNameTextBox, campaignName,
              "Campaign Name Text Box on Update Campaign Page");
        }

        public void CampaignUpdate_EnterNewCampaignStatus(string campaignStatus)
        {
            _webDriver.FindElementAndClick(CampaignManagement.UpdateCampaignStatus,
                "Campaign Status Dropdown On Update Create Page");

            _webDriver.FindElementAndClick(CampaignManagement.UpdateCampaignTypeandStatusSelect.Replace("Name", campaignStatus),
                "Selecting the New Status From Dropdown");
        }

        public void CampaignUpdate_EnterNewStartDate(string startDate)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.UpdateCampaignStartDateTextBox, startDate,
                "Campaign StartDate Text Box on Update Create Page");
        }

        public void CampaignUpdate_EnterNewEndDate(string endDate)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.UpdateCampaignEndDateTextBox, endDate,
                "Campaign EndDate Text Box on Update Create Page");
        }

        public void CampaignUpdate_EnterNewDescription(string description)
        {
            _webDriver.FindElementAndEnterText(CampaignManagement.UpdateCampaignDescriptionTextBox, description,
                "Campaign Description Text Box on Update Create Page");
        }

        public void CampaignUpdate_SelectRegistrationRequiredFlag()
        {
            _webDriver.FindElementAndClick(CampaignManagement.UpdateCampaignRegReqFlagCheckBox,
                "Campaign Registration Required Flag CheckBox on Update Create Page");
        }


        public void CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign()
        {
            _webDriver.FindElementAndClick(CampaignManagement.UpdateCapaignSaveButton,
                "Save Button on Update Campaign Page.");
        }

        public string UpdateCampaignPopUpContent() =>
          _webDriver.GetElementText(CampaignManagement.CampaignCreatePopUpMessage);

        public void ClosePopUpOnUpdateCampaignPage()
        {
            _webDriver.FindElementAndClick(CampaignManagement.CampaignCreatePopUpMessageCloseButton,
                "Pop Up Close Button On Campaign Create Page");
        }

    }
}

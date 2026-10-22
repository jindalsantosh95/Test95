namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class CampaignManagement
    {

        public readonly static string BreadCrumIcon = "//div[@data-control-name='icoWaffelHomeMenu']";

        public readonly static string CampaignSearchTab = "//div[@title='Navigate to Campaign Search']";

        public readonly static string CampaignSearchScreenLoadedValidation = "//div[@data-control-name='scrSrchCampaigns']";

        public readonly static string CampaignCodeTextBox = "//input[@appmagic-control='txtSrchCampCodetextbox']";

        public readonly static string CampaignNameTextBox = "//input[@appmagic-control='txtSrchCampNametextbox']";

        public readonly static string CamapignTypeDropDown = "//div[contains(@class,'combobox-view-chevron arrowContainer')]";

        public readonly static string CampaignTypeTextBox = "//input[@placeholder='Select type']";

        public readonly static string CampaignTypeSelect = "//ul[@role='listbox']/li/div/span";

        public readonly static string CampaignSearchIcon = "//div[@data-control-name='btnSrchCampaign']/div/div/div/div";

        public readonly static string TotalResultsText = "//div[@data-control-name='lblRecordCount']/div/div/div/div/div";

        public readonly static string CampaignCodeLink = "//div[contains(@class,'dyn-container dyn-input-container inputContainer')]/span[contains(@title,'CAMPAIGNCODE')]";

        public readonly static string CampaignUpdateScreenLoadedValidation = "//div[@data-control-name='scrUpdateCampaign']";

        public readonly static string RefreshIcon = "//div[@data-control-name='btnSrchCampaignClear']/div/div/div/div";

        public readonly static string PromotionNameLink = "//div[contains(@Class,'dyn-container dyn-input-container inputContainer')]/span[contains(@title,'PROMOTIONNAME')]";

        public readonly static string StartDateFromBox = "//input[@title='From Start Date(DD-MMM-YYYY)']";

        public readonly static string StartDateToBox = "//input[@title='From End Date(DD-MMM-YYYY)']";

        public readonly static string EndDateFromBox = "//input[@title='To Start Date(DD-MMM-YYYY)']";

        public readonly static string EndDateToBox = "//input[@title='To End Date(DD-MMM-YYYY)']";

        public readonly static string ClearAllBtn = "//div[@title='Clear All']";


        //Campaign Search Result Set fields

        public readonly static string SearchResultDataSet = "//div/table/tbody/tr";


        //Campaign Create Page Elements
        public readonly static string CampaignCreateButton = "//div[@title='Create Campaign']";

        public readonly static string CreateCampaignCode = "//input[@appmagic-control='txtCreateCampCodetextbox']";

        public readonly static string CreateCampaignName = "//input[@appmagic-control='txtCreateCampNametextbox']";

        public readonly static string CreateCampaignType = "//div[@title='Campaign type']";

        public readonly static string CreateCampaignStatus = "//div[@title='Status']";

        public readonly static string CreateCampaignTypeOption = "//*[contains(@class,'itemTemplateLabel_dqr75c') and text()= 'Name']";

        public readonly static string CreateCampaignStatusOption = "//*[contains(@class,'itemTemplateLabel_dqr75c') and text()= 'Name']";

        public readonly static string CreateCampaignStartDate = "//input[@title='Campaign start date (DD-MMM-YYYY)']";

        public readonly static string CreateCampaignEndDate = "//input[@title='Campaign end date (DD-MMM-YYYY)']";

        public readonly static string CreateCampaignRRFlag = "//div[@class='appmagic-checkbox-placeholder']";

        public readonly static string CreateCampaignDescription = "//textarea[@appmagic-control='txtCreateCampDescriptiontextarea']";

        public readonly static string CreateCampaignLinkToTemplate = "//input[@appmagic-control='txtCreateCampLinkTemplatetextbox']";

        public readonly static string CampaignUpdateButton = "//button[@title='Click to update campaign']";

        public readonly static string CampaignCreatePopUpMessage = "//div[@data-control-name='lblMessage']/div/div/div/div/div";

        public readonly static string CampaignCreatePopUpMessageCloseButton = "//div[@title='Click to close']";

        public readonly static string CampaignCreateCancelButton = "//button[@title='Click to terminate save and navigate back to Campaign Search screen']";


        //Campaign Details Update page details

        public readonly static string UpdateCampaignCodeTextBox = "//input[@appmagic-control='txtCampaignCodetextbox']";

        public readonly static string UpdateCampaignNameTextBox = "//input[@appmagic-control='txtCampaignNametextbox']";

        public readonly static string UpdateCampaignTypeTextBox = "//input[@placeholder='Select type']";

        public readonly static string UpdateCampaignStatusTextBox = "//input[@placeholder='Select status']";

        public readonly static string UpdateCampaignTypeFieldStatus = "//div[@data-control-name='cbCampType']/div/div/div/div/div/ul";

        public readonly static string UpdateCampaignType = "//div[@data-control-name='cbCampType']/div/div/div/div/div/ul/li/span";

        public readonly static string UpdateCampaignTypeandStatusSelect = "//ul/li/div/span[text()='Name']";

        public readonly static string UpdateCampaignStatus = "//div[@data-control-name='cbCampStatus']/div/div/div/div/div/ul/li/span";

        public readonly static string UpdateCampaignStartDateTextBox = "//input[@title='Campaign start date (DD-MMM-YYYY)']";

        public readonly static string UpdateCampaignEndDateTextBox = "//input[@title='Campaign end date (DD-MMM-YYYY)']";

        public readonly static string UpdateCampaignRegReqFlagCheckBox = "//input[@appmagic-control='chCampRerRequired']";

        public readonly static string UpdateCampaignDescriptionTextBox = "//textarea[@appmagic-control='txtCampDescriptiontextarea']";

        public readonly static string UpdateCampaignLinkToTemplateTextBox = "//input[@appmagic-control='txtCampLinkTemplatetextbox']";

        public readonly static string UpdateCampaignCancelButton = "//button[@title='Click to terminate save and navigate back to Campaign Search screen']";

        public readonly static string UpdateCapaignSaveButton = "//button[@title='Click to update campaign']";

        public readonly static string RemoveCampaignTypeandStatus = "//button[contains(@aria-label,'Remove')]";

    }
}

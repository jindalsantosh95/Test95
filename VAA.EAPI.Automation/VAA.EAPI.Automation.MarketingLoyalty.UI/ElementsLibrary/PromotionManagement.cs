namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class PromotionManagement
    {

        public readonly static string PromotionsSearchTab = "//div[@data-control-name='icnMenuIcon']/div/div/div/div[@title='Navigate to Promotions Search']";

        //Promotion Search Page fields
        public readonly static string PromotionNameTextBox = "//input[@appmagic-control='promotionsNameTexttextbox']";

        public readonly static string PartnerNameDropDown = "//div[@data-control-name='partnerNameDD']/div/div/div/div/div[2]";

        public readonly static string PartnerNameTextBox = "//input[@placeholder='Search partner name']";

        public readonly static string PartnerNameSelect = "//ul[@aria-label='Retro Partner items']/li/div/span";

        public readonly static string FromStartDate = "//div[@data-control-name='promotionsStartDateFrom']/div/div/div/div/input";

        public readonly static string ToStartDate = "//div[@data-control-name='promotionsStartDateTo']/div/div/div/div/input";

        public readonly static string FromEndDate = "//div[@data-control-name='promotionsEndDateFrom']/div/div/div/div/input";

        public readonly static string ToEndDate = "//div[@data-control-name='promotionsEndDateTo']/div/div/div/div/input";

        public readonly static string ActiveFlagCheckBox = "//div[@class='appmagic-checkbox-placeholder']";

        public readonly static string SearchButton = "//button[@title='Click to search promotions']";

        public readonly static string ClearButton = "//button[@title='Remove all the search filters']";

        public readonly static string NoDataFoundMsg = "//table/tbody/tr/td";

        public readonly static string PopUpMessageContent = "(//div[@data-control-name='ContPopupMessage']/div/div/div/div/div/div/div/div/div)[1]";

        public readonly static string PopUpMessageContentUpdate = "(//div[@data-control-name='ContPopupMessage']/div/div/div/div/div/div/div/div/div)[2]";

        public readonly static string PromotionMasterDataDownloadIcon = "//div[@data-control-name='btnDownloadAllPromotions']/div/div/div/div";

        //Promotion Search Result Set fields

        public readonly static string SearchResultDataSet = "//div/table/tbody/tr";


        //Promotion Update Page Fields
        public readonly static string PromotionIdUpdate = "//input[@appmagic-control='txtUpdatePromotionIDtextbox']";

        public readonly static string PromotionNameUpdateTextBox = "//input[@appmagic-control='txtUpdatePromotionNametextbox']";

        public readonly static string PromotionStartDateUpdateTextBox = "//input[@title='Promotion start date (DD-MMM-YYYY)']";

        public readonly static string PromotionEndDateUpdateTextBox = "//input[@title='Promotion end date (DD-MMM-YYYY)']";

        public readonly static string PromotionActiveFlagUpdate = "//div[@data-control-name='chUpdatePromotionActive']/div/div/div/div";

        public readonly static string PromotionActiveFlagCheckbox = "//input[@appmagic-control='chUpdatePromotionActive']";

        public readonly static string PromotionUpdateSaveButton = "//button[@title='Click to create promotion']";

        public readonly static string PromotionUpdateCancelButton = "//div[@data-control-name='btnCancelUpdatePromotion']/div/div/div/div/button[@title='Click to terminate save and navigate back to Promotion Search screen']";

        public readonly static string PartnerNameRemoval = "//button[contains(@aria-label,' from selection')]";

        public readonly static string PartnerNameUpdateDropDown = "//div[@data-control-name='cbUpdatePromotionPartnerName']/div/div/div/div/div[2]";

        public readonly static string PartnerNameUpdateTextBox = "//div[contains(@class,'searchContainer')]/div/input";

        public readonly static string PartnerNameUpdateSelect = "//ul/li/div/span[text()='partnerName']";

        public readonly static string CampaignLinked = "//input[@appmagic-control='chUpdatePromotionCampaignsLinked']";

        public readonly static string PromotionEnrolledMemberListDownloadIcon = "//div[@data-control-name='btnDownloadMembers']/div";

        public readonly static string PartnerSelectionRemoval = "//ul/li/button[contains(@aria-label,'Remove')]";

        public readonly static string CurrentPartnerName = "//div[@title='Partner name']/div/ul/li/span";

        public readonly static string PopUpCloseButtonUpdate = "(//div[@data-control-name='iconClosePopup']/div/div/div/div)[2]";

        public readonly static string InActivePartnerLabelMessage = "//div[@data-control-name='lblInactivePartnerWarning']/div/div/div/div/div";

        public readonly static string CampaignsLinkedCount = "//div[@data-control-name='lblLinkedCampaignsCount']/div/div/div/div/div";

        //Promotion Create Page Fields
        public readonly static string PromotionCreateButton = "//div[@data-control-name='iconCreatePromotion']/div/div/div/div";

        public readonly static string PromotionNameCreateTextBox = "//input[@appmagic-control='txtCreatePromotionNametextbox']";

        public readonly static string PromotionStartDateCreateTextBox = "//div[@data-control-name='calCreatePromotionStartDate']/div/div/div/div/input";

        public readonly static string PromotionEndDateCreateTextBox = "//div[@data-control-name='calCreatePromotionEndDate']/div/div/div/div/input";

        public readonly static string PromotionActiveFlagCreateCheckBox = "//div[@data-control-name='chCreatePromotionActive']/div/div/div/div";

        public readonly static string PartnerNameCreateDropDown = "//div[@data-control-name='cbCreatePromotionPartnerName']/div/div/div/div/div[2]";

        public readonly static string PartnerNameCreateTextBox = "//div[contains(@class,'searchContainer')]/div/input";

        //public readonly static string PartnerNameSelect = "//ul/li/div/span";

        public readonly static string CreateButton = "//button[@title='Click to create promotion']/div";

        public readonly static string CancelButton = "//button[@title='Click to terminate save and navigate back to Promotion Search screen']";

        public readonly static string PopUpCloseButtoncreate = "(//div[@data-control-name='iconClosePopup']/div/div/div/div)[1]";
    }
}

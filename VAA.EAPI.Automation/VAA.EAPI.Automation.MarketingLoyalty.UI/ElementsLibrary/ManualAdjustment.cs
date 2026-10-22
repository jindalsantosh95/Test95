namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class ManualAdjustment
    {

        public readonly static string ManualAdjustmentTab = "//*[@title='Navigate to Manual Adjustment']";

        public readonly static string PartnerCodeDropdown = "//*[@data-control-name='manualAdjustComboPartnerCode']/div/div/div/div/div[2]";

        public readonly static string PartnerCodeDropdownSelection = "//div[@class='powerapps-flyout-portal']/div/div/ul[@role='listbox']/li/div/span[contains(text(),'partnerCode')]";

        public readonly static string PartnerPointsTextbox = "//*[@appmagic-control='manualAdjusttxtPointstextbox']";

        public readonly static string PartnerTierpointsTextbox = "//input[@appmagic-control='manualAdjusttxtTierPointstextbox']";

        public readonly static string ProductDropdown = "//div[@data-control-name='manualAdjustddlProduct']/div/div/div/div/div/div[2]";

        public readonly static string ProductDropdownSelection = "//div[contains(@id,'appmagic-dropdownFlyoutdropdown') and text()='productName']";

        public readonly static string ActivityDateTextbox = "//input[@appmagic-control='manualAdjusttxtActivityDatetextbox']";

        public readonly static string CommentsTextbox = "//input[@appmagic-control='manualAdjusttxtCommentstextbox']";

        public readonly static string ApplyManualAdjBtn = "//div[@data-control-name='manualAdjustBtnSubmit']/div/div/div/div/button";

        public readonly static string ClearAllBtn = "//div[@data-control-name='manualAdjustBtnClear']/div/div";

        //// Success and Error Messages Pop ups

        public readonly static string SuccessPopupMsg = "//div[@data-control-name='lblAppPopupHeaderHTMLPopup']/div/div/div/div/div";

    }
}

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class TierHistory
    {

        public readonly static string TierHistoryTab = "//div[@aria-label='Navigate to Tier History']";

        public readonly static string TierHistoryTableRows = "//div[contains(@class,'dyn-gridScrollRegion')]/table/tbody/tr";

        public readonly static string TierHistoryScreenLoadStatus = "//div[@data-control-name='lblHiddenTierMgmtScreenLoaded']/div/div/div/div/div[contains(text(),'true')]";

        public readonly static string TierHistoryToMenuBarNavigation = "//div[@data-control-name='btnWaffelTierMgmt']/div/div/div/div";

        public readonly static string ReviseTierButton = "//button[@title='Click to revise tier']";

        public readonly static string ProcessTierChangeButton = "//button[@title='Click to process tier change']";

        public readonly static string ManualTierChangeButton = "//button[@title='Click to process manual tier change']";

        public readonly static string RefreshTierHistoryPageButton = "//div[@data-control-name='btnRefreshTierHistory']/div/div/div/div";

        public readonly static string ClickOnTierListDropdown = "(//div[contains(@class,'combobox-view-chevron arrowContainer')])[1]";

        public readonly static string ClickOnIssueReasonDropdown = "(//div[contains(@class,'combobox-view-chevron arrowContainer')])[3]";

        public readonly static string MemberTierReviseButton = "(//button[@title='Click to revise member tier'])[1]";

        public readonly static string CancelMemberTierReviseButton = "(//button[@title='Click to cancel'])[1]";

        public readonly static string SelectNewTier = "//div/ul/li/div/span[contains(text(),'TierCode')]";

        public readonly static string SelectIssueReason = "//div/ul/li/div/span[contains(text(),'IssueReason')]";

        public readonly static string ClosePopUp = "//div[@data-control-name='iconClosePopup']/div/div/div/div";

        public readonly static string PopUpMessageContent = "//div[@data-control-name='lblMessage']/div/div/div/div/div";

        public readonly static string TierHistoryDataTable = "//div[contains(@class,'gridScrollRegion')]/table/tbody/tr";

        public readonly static string SuppHHWarningYesButton = "//div[@data-control-name='btnYes']/div/div/div/div/button";

        public readonly static string SuppHHWarningNoButton = "//div[@data-control-name='btnNo']/div/div/div/div/button";

    }
}

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class AuditHistory
    {
        public readonly static string AuditHistoryTab = "//div[@title='Navigate to Audit History']";

        public readonly static string AuditHistoryScreenLoadStatus = "//div[@data-control-name='lblAuditScreenLoaded']/div/div/div/div[contains(text(),'true')]";

        public readonly static string AuditHistoryEmailTab = "//div[@data-control-name='btnTabHeaderAuditHistory']/div/div/div/div/button[contains(@title,'Email')]";

        public readonly static string AuditHistoryPhoneTab = "//div[@data-control-name='btnTabHeaderAuditHistory']/div/div/div/div/button[contains(@title,'Phone')]";

        public readonly static string AuditHistoryAddressTab = "//div[@data-control-name='btnTabHeaderAuditHistory']/div/div/div/div/button[contains(@title,'Address')]";

        public readonly static string AuditHistoryPreferencesTab = "//div[@data-control-name='btnTabHeaderAuditHistory']/div/div/div/div/button[contains(@title,'Preferences')]";

        public readonly static string AuditHistoryAwardTab = "//div[@data-control-name='btnTabHeaderAuditHistory']/div/div/div/div/button[contains(@title,'Status Flags')]";

        public readonly static string EmailRibbon = "//div[@data-control-name='dtAuditHistoryMemberEmail']";

        public readonly static string SearchResultPhoneAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberPhone']//table/tbody/tr";

        public readonly static string SearchResultEmailAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberEmail']//table/tbody/tr";

        public readonly static string SearchResultAddressAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberAddress']//table/tbody/tr";

        public readonly static string SearchResultPreferencesAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberPreference']//table/tbody/tr";

        public readonly static string SearchResultMemberProfileAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberProfile']//table/tbody/tr";

        public readonly static string SearchResultAwardAuditDataSet = "//div[@data-control-name='dtAuditHistoryMemberAward']//div/div/div/div/div/div/div/div/div[2]/table/tbody/tr";

        public readonly static string PopUpMessageContent = "//div[@data-control-name='lblMessage']/div/div/div/div/div";

        public readonly static string AuditHistoryMemberProfileTab = "//button[@title='Member Profile']";
    }
}

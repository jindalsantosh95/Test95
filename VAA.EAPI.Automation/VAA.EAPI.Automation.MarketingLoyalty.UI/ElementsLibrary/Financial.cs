namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class Financial
    {
        public readonly static string FinancialTab = "//div[@title='Navigate to Financial']";

        public readonly static string FinancialScreenLoadStatus = "//div[@data-control-name='lblCardLinkScreenLoaded']/div/div/div/div[contains(text(),'true')]";

        public readonly static string WaitUntilPALoads = "//div[contains(@class,'progressIndicatorPaused')]";

        public readonly static string AlertCloseButton = "//div[@title='Click to close']";

        public readonly static string SearchResultForVRedCardLinks = "//div[@data-control-name='galMyCardLinks']/div/div/div/div/div[1]/div[1]";

        public readonly static string SearchResultMemberCardLinksDataSet = "//div[contains(@class,'virtualized-gallery-item')]/div[@data-container-name='galMyCardLinks-container']";

        public readonly static string AddCardIcon = "//div[@title='Click to add card']";

        public readonly static string AddCardDropdown = "//div[@data-control-name='cbCenturionCardCode']/div/div/div/div[1]/div[2]";

        public readonly static string SelectCardType = "//span[text()='cardType']";

        public readonly static string SubmitButton = "//div[@data-control-name='btnSubmitAddCardRequest']/div/div/div/div/button";

        public readonly static string AddMemberCardPopUpMessage = "//div[@data-control-name='lblMessage']/div/div/div/div/div";

        public readonly static string AddMemberCardPopUpCloseIcon = "//div[@title='Click to close']";

        //Card Details

        public readonly static string ListOfCardLinks = "//div[@data-container-name='galMyCardLinks-container']/div";

        public readonly static string PartnerCode = "div[@data-control-name='colPartnerCode']/div/div/div/div/div";

        public readonly static string PartnerName = "div[@data-control-name='colPartnerName']/div/div/div/div/div";

        public readonly static string CardType = "div[@data-control-name='colCardType']/div/div/div/div/div";

        public readonly static string CardLinkedDate = "div[@data-control-name='colCardStartDate']/div/div/div/div/div";

        public readonly static string CardEndDate = "div[@data-control-name='colCardEndDate']/div/div/div/div/div";

        public readonly static string CardAnniversaryDate = "div[@data-control-name='colCardAnniverssary']/div/div/div/div/div";

        public readonly static string AuditHistoryIcon = "//div[@data-control-name='icoCardAuditHistory']/div";

        //Card Retrival Audit

        public readonly static string CardAuditHistoryWindow = "//div[@title='Card Audit History']";

        public readonly static string SearchResultCardAuditDataSet = "//div[@data-container-name='galCardAuditHistory-container']/div";

        public readonly static string AuditEndDate = "div[@data-control-name='lblCardAuditEndDateValue']/div/div/div/div/div";

        public readonly static string AuditUpdatedBy = "div[@data-control-name='lblCardAuditUpdatedByValue']/div/div/div/div/div";

        public readonly static string AuditUpdatedOn = "div[@data-control-name='lblCardAuditUpdatedOnValue']/div/div/div/div/div";

        public readonly static string CardAuditHistoryWindowCloseButton = "//div[@data-control-name='icoCloseCardAuditPopup']/div/div/div/div";

        public readonly static string FinancialPageToMenuBarNavigation = "//div[@data-control-name='btnWaffelMyCardLinksDetail']/div/div/div/div";

        public readonly static string MemberDetailPageTab = "(//div[@title='Navigate to Member Detail'])[1]";

    }
}

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class MemberAccountDetails
    {
        public readonly static string AccountDetailsTab = "//div[@title='Navigate to Account Details']";

        public readonly static string AccountDetailsScreenLoadStatus = "//div[@data-control-name='lblHiddenTierAccountScreenLoaded']/div/div/div/div/div[contains(text(),'true')]";

        public readonly static string RefreshRibbonLink = "//div[contains(@class,'ms-MessageBar-text')]/span/span[contains(text(),'Refreshed successfully!')]";

    }

    public static class RedTierMemberAccountDetails
    {
        //Page Elements applicable for a Red Member

        public readonly static string RefreshRedMemberBtn = "//div[@data-control-name='refreshAccDetails']";

        public readonly static string TierPointsEarnedToUpgradeToSilverTextBox = "//input[@appmagic-control='accDetTierPointEarnedSilverTexttextbox']";

        public readonly static string TierPointsEarnedToUpgradeToGoldTextBox = "//input[@appmagic-control='accDetTierPointEarnedGoldTexttextbox']";

        public readonly static string ByDate_ToSilverTextBox = "//input[@appmagic-control='accDetByDateSilverTexttextbox']";

        public readonly static string ByDate_ToGoldTextBox = "//input[@appmagic-control='accDetByDateGoldTexttextbox']";

        public readonly static string TotalTierPointsNeededToUpgradeToSilverTextBox = "//input[@appmagic-control='accDetTierPointNeededSilverTexttextbox']";

        public readonly static string TotalTierPointsNeededToUpgradeToGoldTextBox = "//input[@appmagic-control='accDetTierPointNeededGoldTexttextbox']";

        public readonly static string TotalVSFlownMilesTextBox = "//input[@appmagic-control='accDetVSFlownMilsTexttextbox']";

    }

    public static class SilverTierMemberAccountDetails
    {
        //Page Elements applicable for a Silver Member

        public readonly static string RefreshSilverMemberBtn = "//div[@data-control-name='refreshAccDetailsSilverMem']";

        public readonly static string TierPointEarnedForSilverTierRenewalTextBox = "//input[@appmagic-control='accDetTierPointEarnedSilverText_1textbox']";

        public readonly static string TierRenewalFromDateTextBox = "//input[@appmagic-control='accDetByDateSilverText_1textbox']";

        public readonly static string TierRenewalToDateTextBox = "//input[@appmagic-control='accDetByDateSilverText_2textbox']";

        public readonly static string TotalTierPointsNeededForSilverRenewalTextBox = "//input[@appmagic-control='accDetTierPointNeededSilverText_1textbox']";

        public readonly static string TotalVSFlownMilesTextBox = "//input[@appmagic-control='accDetVSFlownMilsText_1textbox']";

        public readonly static string TotalVSTierPtsInCurrentMembershipYearTextBox = "//input[@appmagic-control='accDetVSFlownMilsText_2textbox']";

        public readonly static string ByDate_ToGoldTextBox = "//input[@appmagic-control='accDetByDateGoldText_1textbox']";

        public readonly static string TotalTierPointsNeededToUpgradeToGoldTextBox = "//input[@appmagic-control='accDetTierPointNeededGoldText_1textbox']";

        public readonly static string TotalTierPointsEarnedToUpgradeToGoldTextBox = "//input[@appmagic-control='accDetTierPointEarnedGoldText_1textbox']";

    }

    public static class GoldTierMemberAccountDetails
    {
        //Page Elements applicable for a Gold Member

        public readonly static string RefreshGoldMemberBtn = "//div[@data-control-name='refreshAccDetailsGoldMem']";

        public readonly static string TierPointEarnedForGoldTierRenewalTextBox = "//input[@appmagic-control='accDetTierPointEarnedSilverText_2textbox']";

        public readonly static string TierRenewalFromDateTextBox = "//input[@appmagic-control='accDetByDateSilverText_3textbox']";

        public readonly static string TierRenewalToDateTextBox = "//input[@appmagic-control='accDetByDateSilverText_4textbox']";

        public readonly static string TotalTierPointsNeededForGoldRenewalTextBox = "//input[@appmagic-control='accDetTierPointNeededSilverText_2textbox']";

        public readonly static string NoOfConsecutiveYearsAtGold = "//input[@appmagic-control='accDetTierPointNeededGoldText_2textbox']";

        public readonly static string TotalVSTierPointsSinceBecomingGold = "//input[@appmagic-control='accDetTierPointEarnedGoldText_2textbox']";

        public readonly static string TotalVSFlownMilesTextBox = "//input[@appmagic-control='accDetVSFlownMilsText_3textbox']";

        public readonly static string TotalVSTierPtsInCurrentMembershipYearTextBox = "//input[@appmagic-control='accDetVSFlownMilsText_4textbox']";

        public readonly static string WingsStartDate = "//input[@appmagic-control='accDetWingsStartDateTxttextbox']";

        public readonly static string TotalTierPointsEarnedSinceBecomingWings = "//input[@appmagic-control='accDetPointsSinceWingsTxttextbox']";

        public readonly static string TotalVSTierPointsEarnedSinceBecomingWings = "//input[@appmagic-control='accDetVSPointsSinceWingsTxttextbox']";
    }
}

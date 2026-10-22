namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class Household
    {
        public readonly static string RelationshipTab = "//div[@title='Navigate to Relationship']";

        public readonly static string RelationShipHistoryHeader = "//div[@data-control-name='lblNoHoHMember']/div/div/div/div/div";

        public readonly static string LinkButton = "//*[@data-control-name='btnLinkMember']/div/div/div/div/button";

        public readonly static string DelinkButton = "//*[@data-control-name='btnDelinkMember']/div/div/div/div/button";

        public readonly static string HouseholdScreenLoadStatus = "//div[@data-control-name='lblHiddenHHRelnScreenLoaded']/div/div/div/div/div[contains(text(),'true')]";

        public readonly static string LinkMemberPopUp = "//div[@data-control-name='BackgroundCenterContainer']/div";

        public readonly static string LinkMemberMemberAccountNumbertextbox = "//*[@appmagic-control='txtMemberAccNumbertextbox']";

        public readonly static string LinkMemberMemberAccountSearchButton = "//div[@data-control-name='btnSrchMemberAccNumber']/div";

        public readonly static string LinkMemberMemberAccountSearchLoadStatus = "//div[@data-control-name='BackgroundCenterContainer']/div/div/div/div/div/div/div/div/div/div[contains(text(),'Link Member')]";

        public readonly static string ValidateAndLinkBtn = "//div[@data-control-name='btnValidateAndLink']/div/div/div/div/button";

        public readonly static string LinkMemberSuccessMessage = "//div[@data-control-name='ContPopupMessage']/div/div/div/div/div/div/div/div/div[contains(text(),'Household linking successful.')]";

        public readonly static string LinkMemberClosePopup = "(//div[@data-control-name='iconClosePopup']/div/div/div/div)[2]";

        public readonly static string ChildMemberNumber = "//div[@data-control-name='lblHoHMemberID']/div/div/div/div/div[contains(text(),'ChildMemberNumber')]";

        public readonly static string LinkDelinkPopUpCloseButton = "(//div[@data-control-name='iconClosePopup'])[2]";

        public readonly static string LinkDelinkPopUpMessage = "(//div[@data-control-name='lblMessage']/div/div/div/div/div)[2]";

        public readonly static string DelinkYesButton = "//div[@data-control-name='btnDelinkYes']/div/div/div/div/button";

        public readonly static string DelinkNoButton = "//div[@data-control-name='btnDelinkNo']/div/div/div/div/button";

        //Link Button Pop Up Fields

        public readonly static string SearchChildFCAccountTxtBox = "//input[@appmagic-control='txtMemberAccNumbertextbox']";

        public readonly static string SearchChildFCAccountButton = "//div[@data-control-name='btnSrchMemberAccNumber']/div/div/div/div";

        public readonly static string ClearChildFCAccountButton = "//div[@data-control-name='btnClearMemberSearch']/div/div/div/div";

        public readonly static string ValidateAndLinkButton = "//div[@data-control-name='btnValidateAndLink']/div/div/div/div/button";

        public readonly static string CancelLinkButton = "//div[@data-control-name='btnCancelLink']/div/div/div/div/button";

        public readonly static string LinkPopUpErrorMessage = "//div[@data-control-name='txtErrorDetails']/div/div/div/div/div";


        //Household Active Members Section Fields

        public readonly static string MemberIdActive = "(//div[@data-control-name='lblHoHMemberID']/div/div/div/div/div)";

        public readonly static string MemberFirstNameActive = "(//div[@data-control-name='lblHoHFirstName']/div/div/div/div/div)";

        public readonly static string MemberLastNameActive = "(//div[@data-control-name='lblHoHLastName']/div/div/div/div/div)";

        public readonly static string MemberStatusActive = "(//div[@data-control-name='lblHoHMemberStatus']/div/div/div/div/div)";

        public readonly static string MemberAssociationDateActive = "(//div[@data-control-name='lblHoHAssociationDate']/div/div/div/div/div)";

        public readonly static string MemberOverrideCheckBoxActive = "(//div[@class='appmagic-checkbox-placeholder'])";

        public readonly static string MemberRelationshipTypeActive = "(//div[@data-control-name='lblHoHRelationshipType']/div/div/div/div/div)";

        public readonly static string MemberLinkedBy = "//div[@data-control-name='lblHoHLinkedBy']/div/div/div/div/div";

        public readonly static string MemberLinkedOn = "//div[@data-control-name='lblHoHLinkedOn']/div/div/div/div/div";

        //Household Relationship History Section Fields

        public readonly static string MemberIdHistory = "//div[@data-control-name='lblRHMemberID']/div/div/div/div/div";

        public readonly static string MemberFirstNameHistory = "(//div[@data-control-name='lblRHFirstName']/div/div/div/div/div)";

        public readonly static string MemberLastNameHistory = "(//div[@data-control-name='lblRHLastName']/div/div/div/div/div)";

        public readonly static string MemberStatusHistory = "(//div[@data-control-name='lblRHMemberStatus']/div/div/div/div/div)";

        public readonly static string MemberLinkedDateHistory = "(//div[@data-control-name='lblRHStartDate']/div/div/div/div/div)";

        public readonly static string MemberDelinkedDateHistory = "(//div[@data-control-name='lblRHEndDate']/div/div/div/div/div)";

        public readonly static string RelationshipTypeHistory = "(//div[@data-control-name='lblRHRelationshipType']/div/div/div/div/div)";

        public readonly static string MemberLinkedByHistory = "//div[@data-control-name='lblRHLinkedBy']/div/div/div/div/div";

        public readonly static string MemberLinkedOnHistory = "//div[@data-control-name='lblRHDelinkedBy']/div/div/div/div/div";



        //Household delink 

        public static string DelinkLessThanTweleveMonthsPopupButton = "//div[@data-control-name='btnDelinkYes']/div/div/div/div/button";

        public static string DelinkSuccessPopup = "(//div[@data-control-name='ContPopupMessage']/div/div/div/div/div/div/div/div)[2]/div[contains(text(),'Household delinking successful')]";

        public static string DelinkCloseLink = "(//div[@data-control-name='iconClosePopup'])[2]";

        public static string DelinkHistoryMemberSection = "(//div[@data-control-name='lblRHMemberID']/div/div/div/div/div)";


    }
}

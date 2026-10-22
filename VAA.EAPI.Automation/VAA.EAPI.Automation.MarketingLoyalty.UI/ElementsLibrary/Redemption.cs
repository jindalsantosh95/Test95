namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class Redemption
    {
        public static readonly string MemberdetailMenuBarBtn = "//div[@data-control-name='btnWaffelMember']/div/div/div/div";

        public static readonly string RedemptionTabBtn = "(//div[@title='Navigate to Redemption'])[2]";

        //Redemption Page

        public static readonly string PartnerCodeDropDownBtn = "//div[@data-control-name='manualRedemptionComboPartnerCode']/div/div/div/div/div[2]";

        public static readonly string PartnerCodeSelect = "//*[contains(@id,'powerapps-flyout-react-combobox-view-')]/div/ul/li/div/span[text()='partnerCode']";

        public static readonly string ProductDropDownBtn = "(//*[contains(@id,'dropdown_siena_')]/div/div[2])[1]";

        public static readonly string ProductSelect = "//*[contains(@id,'appmagic-dropdownFlyoutdropdown_siena') and text()='productName' ]";

        public static readonly string VoucherDropDownBtn = "//div[@data-control-name='manualRedemptionddlFlightVoucher']/div/div/div/div/div/div[2]";

        public static readonly string VoucherSelect = "//*[contains(@id,'appmagic-dropdownFlyoutdropdown_siena') and contains( text(),'VoucherName' )]";

        public static readonly string VoucherpointsTxtBox = "//input[@appmagic-control='manualRedemptiontxtCappingPointstextbox']";

        public static readonly string PointsTxtBox = "//input[@appmagic-control='manualRedemptiontxtPointstextbox']";

        public static readonly string CommentsTxtBox = "//input[@appmagic-control='manualRedemptiontxtCommenttextbox']";

        public static readonly string ActiviyDateTxtBox = "//div[@data-control-name='manualRedemptiontxtActivityDate']/div/div/div/div/input[@title='DD-MMM-YYYY']";

        public static readonly string OriginAirportCodeDropdown = "//div[@data-control-name='manualRedemptionComboFlightOrig']/div/div/div/div/div[2]";

        public static readonly string OriginAiportCodeselect = "//ul/li/div/span[text()='OriginAirportCode']";

        public static readonly string DestinationAirportCodeDropdown = "//div[@data-control-name='manualRedemptionComboFlightDest']/div/div/div/div/div[2]";

        public static readonly string DestinationAiportCodeselect = "//ul/li/div/span[text()='DestinationAirportCode']";

        public static readonly string PNRTxtBox = "//input[@appmagic-control='manualRedemptiontxtFlightPNRtextbox']";

        public static readonly string BookingClassTxtBox = "//input[@appmagic-control='manualRedemptiontxtFlightBookingClasstextbox']";

        public static readonly string NoOfPasngrsTxtBox = "//input[@appmagic-control='manualRedemptiontxtFlightNoPassengerstextbox']";

        public static readonly string TicketNumberTxtBox = "//input[@appmagic-control='manualRedemptiontxtTicketNotextbox']";

        public static readonly string NoOfSegmentsTxtBox = "//input[@appmagic-control='manualRedemptiontxtFlightNoSegementstextbox']";

        public static readonly string ApplyRedemptionBtn = "//div[@data-control-name='manualRedemptionBtnSubmit']/div/div/div/div/button";

        public static readonly string ClearAllBtn = "//div[@data-control-name='manualRedemptionBtnClear']/div/div/div/div/button/div";

        public static readonly string Missingmandatoryfields = "//*[@data-control-name='ManualRedemptionlblWarnSummary']/div/div/div/div/div";

        public static readonly string Disabledbtn = "//div[@data-control-name='manualRedemptionBtnSubmit']/div/div/div/div/button";

        //Pop Up Messge

        public static readonly string PopUpMessageContent = "//div[@data-control-name='htmlPopupMessage']/div/div/div/div/div/div/div/div";

        public static readonly string PopUpCloseBtn = "//*[@data-control-name='htmlPopupCloseIcon']/div/div/div/div";

        //voucher pts credit on redemption screen validations

        public static readonly string VirginPointsToBeCreditedtextBox = "//*[@title='Manually enter the amount of points to be credited']";

        public static readonly string MyOffers_VouchersDropdown = "//div[@data-control-name='manualRedemptionddlFlightVoucher']/div/div/div/div/div/div[2]";

        public static readonly string MyOffers_VoucherName = "//div[contains(@id,'appmagic-dropdownFlyoutdropdown')]/div[1]/div[contains(text(),'VoucherNumber')]";

    }
}

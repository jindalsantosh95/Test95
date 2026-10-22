using System.ComponentModel;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class RetroPartnerClaim
    {
        public readonly static string RetroClaimTab = "//*[@title ='Navigate to Retro Partner']";

        public readonly static string SelectSkyTeamAutoRetro = "//label[@class='radio-label click' and @value='SkyTeam Auto Retro']";

        public readonly static string OperAirPartnerCodeDropdown = "//div[@data-control-name='cbSkyTeamROperatingPartnerCode']";

        public readonly static string OperAirPartnerCodeDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer_14xc3ab')]/li/div/span[contains(text(),'OPERAIRLINEPARTNERCODE')]";

        public readonly static string OperBookingClassDropdown = "//div[@data-control-name='cbSkyTeamPROperatingBookingClass']";

        public readonly static string OperBookingClassDropdownSeltFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer_14xc3ab')]/li/div/span[contains(text(),'OPERATINGBOOKINGCLASS')]";

        public readonly static string MarkBookingClassDropdown = "//div[@data-control-name='cbSkyTeamPRMarketingBookingClass']";

        public readonly static string MarkBookingClassDropdownSeltFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer_14xc3ab')]/li/div/span[contains(text(),'MARKETINGBOOKINGCLASS')]";

        public readonly static string OperFlightNumberTextbox = "//input[@appmagic-control='txtSkyTeamPROperatingFlightNumbertextbox']";

        public readonly static string OriginAirportDropdown = "//div[@data-control-name='cbSkyTeamPROriginAirport']";

        public readonly static string OriginAirportDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer')]/li/div/span[contains(text(),'ORG')]";

        public readonly static string DestinationAirportDropdown = "//div[@data-control-name='cbSkyTeamPRDestinationAirport']";

        public readonly static string DestinationAirportDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer')]/li/div/span[contains(text(),'DEST')]";

        public readonly static string CabinClassDropdown = "//div[@data-control-name='cbSkyTeamPRMarketedCabin']";

        public readonly static string CabinClassDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer_14xc3ab')]/li/div/span[contains(text(),'CABINCLASS')]";

        public readonly static string ActivityDatePicker = "(//input[@placeholder='DD-MMM-YYYY'])[1]";

        public readonly static string SeatNumberTextbox = "//input[@appmagic-control='txtSkyTeamPRSeatNumbertextbox']";

        public readonly static string TicketNumberTextbox = "//input[@appmagic-control='txtSkyTeamPRTicketNumbertextbox']";

        public readonly static string PnrNumberTextbox = "//input[@appmagic-control='txtSkyTeamPRPNRtextbox']";

        public readonly static string CreateButton = "//div[@class='appmagic-button-label no-focus-outline' and text()='Create']";

        public readonly static string RetroPopUpMessageContent = "//div[@data-control-name='ContPopupMessage']";

        public readonly static string PopUpCloseButton = "//div[@data-control-name='iconClosePopup']/div/div/div/div";
    }

    public enum RetroClaims
    {
        [Description("NZ Retro")]
        NZRetro,

        [Description("Manual Retro - Partner Flights")]
        ManualRetroPartnerFlights,

        [Description("SkyTeam Auto Retro")]
        SkyTeamAutoRetro
    }
}

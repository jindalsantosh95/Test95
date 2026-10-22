namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class FlightAccurals
    {
        public readonly static string FlightAccrualsTab = "//*[@title ='Navigate to Flight Accruals']";

        public readonly static string BookingReferenceTextbox = "//input[@appmagic-control='txtFABookingReftextbox']";

        public readonly static string BookingClassDropdown = "//div[@data-control-name='cbFABookingClass']/div/div/div/div/div[2]";

        public readonly static string BookingClassDropdownTextBox = "//input[@placeholder='Booking Class']";

        public readonly static string BookingClassDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer')]/li/div/span[contains(text(),'BOOKINGCLASS')]";

        public readonly static string TicketIssueDatePicker = "//div[@data-control-name='calFATicketDate']/div/div/div/div/input";

        public readonly static string ActivityDatePicker = "//div[@data-control-name='calFAActivityDate']/div/div/div/div/input";

        public readonly static string FlightNumberTextbox = "//input[@appmagic-control='txtFAFlightNumbertextbox']";

        public readonly static string OriginAirportDropdown = "//div[@data-control-name='cbFAOrigin']";

        public readonly static string OriginAirportDropdownTextbox = "//input[@placeholder='Origin']";

        public readonly static string OriginAirportDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer')]/li/div/span[contains(text(),'ORG')]";

        public readonly static string DestinationAirportDropdown = "//div[@data-control-name='cbFADestination']";

        public readonly static string DestinationAirportDropdownTextbox = "//input[@placeholder='Destination']";

        public readonly static string DestinationAirportDropdownSelectFirstItemFromList = "//ul[contains(@class,'appmagic-combobox-itemlist itemsContainer')]/li/div/span[contains(text(),'DEST')]";

        public readonly static string TicketnumberTextbox = "//input[@appmagic-control='txtFATicketNumbertextbox']";

        public readonly static string CommentsTextbox = "//textarea[@appmagic-control='txtFACommentstextarea']";

        public readonly static string ClearBtn = "//div[@data-control-name='btnCancelFlightAccrual']/div/div/div/div/button";

        public readonly static string CreateBtn = "//div[@data-control-name='btnCreateFlightAccrual']/div/div/div/div/button";

        // Warning and Error Messages Pop ups

        public readonly static string PopUpMessageContent = "//div[@data-control-name='ContPopupMessage']";

        public readonly static string BookingRefWrngMsg = "//div[@data-control-name='lblFABookingRefWarn']/div/div/div/div";

        public readonly static string FlightNumberWrngMsg = "//div[@data-control-name='lblFAFlightNumberWarn']/div/div/div/div";

        public readonly static string TicketNumberWrngMsg = "//div[@data-control-name='lblFATicketNumberWarn']/div/div/div/div";

        public readonly static string ToastWrngMsg = "//div[contains(@class,'ms-MessageBar-text') and @role='alert']/span/span";

        public readonly static string PopUpCloseButton = "//div[@data-control-name='iconClosePopup']/div/div/div/div";

    }
}

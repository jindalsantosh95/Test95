using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class FlightAccuralsPage
    {
        private readonly IWebDriver _webDriver;

        public FlightAccuralsPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnMenuBarOnMemberProfilePage()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            _webDriver.FindElementAndClick(FlightAccurals.FlightAccrualsTab, "Click on the Flight Accruals Tab");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void EnterBookingReferenceNumber(string bookingReference)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.BookingReferenceTextbox, bookingReference,
                "Booking reference / PNR Text Box");
        }

        public void EnterBookingClass(string bookingClass)
        {
            if (!string.IsNullOrEmpty(bookingClass))
            {
                _webDriver.FindElementAndClick(FlightAccurals.BookingClassDropdown,
                    "Booking class drop down");

                _webDriver.FindElementAndClick(FlightAccurals.BookingClassDropdownSelectFirstItemFromList.Replace("BOOKINGCLASS", bookingClass),
                    "Booking class selection from the drop down");
            }

        }

        public void EnterTicketIssueDate(string ticketIssueDate)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.TicketIssueDatePicker, ticketIssueDate,
                "Ticket Issue Date TextBox");
        }

        public void EnterActivityDate(string activityDate)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.ActivityDatePicker, activityDate,
                "Activity Date TextBox");
        }

        public void EnterFlightNumber(string flightNumber)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.FlightNumberTextbox, flightNumber,
                "Flight Number TextBox");
        }

        public void EnterOriginAiportCode(string originAirportCode)
        {
            if (!string.IsNullOrEmpty(originAirportCode))
            {
                _webDriver.FindElementAndClick(FlightAccurals.OriginAirportDropdown,
                    "Origin AirportCode drop down");

                _webDriver.FindElementAndClick(FlightAccurals.OriginAirportDropdownSelectFirstItemFromList.Replace("ORG", originAirportCode),
                    "Origin AirportCode selection from the drop down");
            }

        }

        public void EnterDestinationAiportCode(string destinationAirportCode)
        {
            if (!string.IsNullOrEmpty(destinationAirportCode))
            {
                _webDriver.FindElementAndClick(FlightAccurals.DestinationAirportDropdown,
                "Destination AirportCode drop down");

                _webDriver.FindElementAndClick(FlightAccurals.DestinationAirportDropdownSelectFirstItemFromList.Replace("DEST", destinationAirportCode),
                    "Destination Airport selection from the drop down");
            }
        }

        public void EnterTicketNumber(string ticketNumber)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.TicketnumberTextbox, ticketNumber,
                "Ticket number TextBox");

        }

        public void EnterComment(string comment)
        {
            _webDriver.FindElementAndEnterText(FlightAccurals.CommentsTextbox, comment,
                "Commment Textbox");
        }

        public void ClickOnSubmitButton()
        {
            _webDriver.FindElementAndClick(FlightAccurals.CreateBtn, "Flight Accrual Submit Button");
        }


        public string[] GetMessageFromThePopUpScreen()
        {
            _webDriver.FindElement(FlightAccurals.PopUpMessageContent, "POP UP Message", 120);

            return _webDriver.GetAttributeTextValue(FlightAccurals.PopUpMessageContent, "textContent").Split(": ");
        }

        public void CloseThePopUp()
        {
            _webDriver.FindElementAndClick(FlightAccurals.PopUpCloseButton, "Pop Up Close Button");

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Alert Pop Up", 10);

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);
        }

        public bool IsCreateButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(FlightAccurals.CreateBtn, "disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public string GetErrorMessage(string key)
        {
            try
            {
                return _webDriver.GetAttributeTextValue(key, "title");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

    }
}

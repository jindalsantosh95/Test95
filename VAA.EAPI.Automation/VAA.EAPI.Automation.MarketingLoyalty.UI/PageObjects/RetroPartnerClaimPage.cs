using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class RetroPartnerClaimPage
    {
        private readonly IWebDriver _webDriver;

        public RetroPartnerClaimPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnMenuBarOnMemberProfilePage()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            _webDriver.FindElementAndClick(RetroPartnerClaim.RetroClaimTab, "Click on the Retro Partner Tab");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void SelectRetroClaims(ExtentTest logger, string retroClaims)
        {
            if (!retroClaims.IsNullOrEmpty())
            {
                _webDriver.WaitForElementToBePresent(RetroPartnerClaim.SelectSkyTeamAutoRetro, 10);

                _webDriver.FindElement(By.XPath(RetroPartnerClaim.SelectSkyTeamAutoRetro)).Click();
            }

            logger.Log(Status.Info, "Selected the Retro Claims");
        }

        public void EnterOperAirPartnerCode(string operAirlinePartnerCode)
        {
            if (!operAirlinePartnerCode.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.OperAirPartnerCodeDropdown,
                    "Operating Airline Partner code drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.OperAirPartnerCodeDropdownSelectFirstItemFromList.Replace("OPERAIRLINEPARTNERCODE", operAirlinePartnerCode),
                    "Operating Airline Partner code selection from the drop down");
            }
        }

        public void EnterOperBookingClass(string operBookingClass)
        {
            if (!operBookingClass.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.OperBookingClassDropdown,
                    "Operating Booking class drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.OperBookingClassDropdownSeltFirstItemFromList.Replace("OPERATINGBOOKINGCLASS", operBookingClass),
                    "Operating Booking class selection from the drop down");
            }
        }

        public void EnterMarkBookingClass(string markBookingClass)
        {
            if (!markBookingClass.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.MarkBookingClassDropdown,
                    "Marketing Booking class drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.MarkBookingClassDropdownSeltFirstItemFromList.Replace("MARKETINGBOOKINGCLASS", markBookingClass),
                    "Marketing Booking class selection from the drop down");
            }
        }

        public void EnterOperFlightNumber(string operFlightNumber)
        {
            _webDriver.FindElementAndEnterText(RetroPartnerClaim.OperFlightNumberTextbox, operFlightNumber,
                "Operating Flight Number TextBox");
        }

        public void EnterOriginAiport(string originAirport)
        {
            if (!originAirport.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.OriginAirportDropdown,
                    "Origin AirportCode drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.OriginAirportDropdownSelectFirstItemFromList.Replace("ORG", originAirport),
                    "Origin AirportCode selection from the drop down");
            }

        }

        public void EnterDestinationAiport(string destinationAirport)
        {
            if (!destinationAirport.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.DestinationAirportDropdown,
                "Destination AirportCode drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.DestinationAirportDropdownSelectFirstItemFromList.Replace("DEST", destinationAirport),
                    "Destination Airport selection from the drop down");
            }
        }

        public void EnterCabinClass(string cabinClass)
        {
            if (!cabinClass.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(RetroPartnerClaim.CabinClassDropdown,
                    "Cabin class drop down");

                _webDriver.FindElementAndClick(RetroPartnerClaim.CabinClassDropdownSelectFirstItemFromList.Replace("CABINCLASS", cabinClass),
                    "Cabin class selection from the drop down");
            }
        }

        public void EnterActivityDate(string activityDate)
        {
            _webDriver.FindElementAndEnterText(RetroPartnerClaim.ActivityDatePicker, activityDate,
                "Activity Date TextBox");
        }

        public void EnterSeatNumber(string seatNumber)
        {
            _webDriver.FindElementAndEnterText(RetroPartnerClaim.SeatNumberTextbox, seatNumber,
                "Seat Number TextBox");
        }

        public void EnterTicketNumber(string ticketNumber)
        {
            _webDriver.FindElementAndEnterText(RetroPartnerClaim.TicketNumberTextbox, ticketNumber,
                "Ticket Number TextBox");
        }

        public void EnterPnrNumber(string pnrNumber)
        {
            _webDriver.FindElementAndEnterText(RetroPartnerClaim.PnrNumberTextbox, pnrNumber,
                "Pnr Number TextBox");
        }

        public void ClickOnCreateButton()
        {
            _webDriver.FindElementAndClick(RetroPartnerClaim.CreateButton, "Retro Partner Submit Button");
        }

        public string[] GetMessageFromThePopUpScreen()
        {
            _webDriver.FindElement(RetroPartnerClaim.RetroPopUpMessageContent, "POP UP Message", 120);

            return _webDriver.GetAttributeTextValue(RetroPartnerClaim.RetroPopUpMessageContent, "textContent").Split(": ");
        }

        public void CloseThePopUp()
        {
            _webDriver.FindElementAndClick(RetroPartnerClaim.PopUpCloseButton, "Pop Up Close Button");

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Alert Pop Up", 10);

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);
        }
    }
}

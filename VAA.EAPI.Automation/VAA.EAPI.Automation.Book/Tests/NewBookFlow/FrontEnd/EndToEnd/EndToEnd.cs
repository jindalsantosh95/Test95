using OpenQA.Selenium;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects;
using VAA.EAPI.Automation.Core.UI.PageObjects.DotCom;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.EndToEnd
{
    [Collection("EndToEnd")]
    public class EndToEnd : IDisposable
    {
        private IWebDriver webDriver;
        private FlightSearchPanelPageObjects flightSearchPanel;
        private FareSelectionPageObjects flightFareSelection;
        private SummaryPageObjects summaryPage;
        private PassengerDetailsPageObjects passengerDetails;
        private SeatsPageObjects seats;
        private HppForm hpp;

        public EndToEnd()
        {
            webDriver = GetBrowser(BrowserType.Chrome, baseUrl);

            flightSearchPanel = new FlightSearchPanelPageObjects(webDriver);
            flightFareSelection = new FareSelectionPageObjects(webDriver);
            summaryPage = new SummaryPageObjects(webDriver);
            passengerDetails = new PassengerDetailsPageObjects(webDriver);
            seats = new SeatsPageObjects(webDriver);
            hpp = new HppForm(webDriver);
        }

        public static string baseUrl = "https://stg.virginatlantic.com/";

        public readonly string depatureDate = OfferSearchHelper.DepartureDate();

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public void HappyPathBusinessClassWithSeats()
        {
            flightSearchPanel.ClickOnAgreeToCookies();

            flightSearchPanel.EnterFlyingFrom("LHR");

            flightSearchPanel.ClickOnPopOverSelector("LHR");

            flightSearchPanel.EnterFlyingTo("JFK");

            flightSearchPanel.ClickOnPopOverSelector("JFK");

            flightSearchPanel.SelectDepartureAndReturnDate(5, 12);

            flightSearchPanel.ClickOnSearch();

            flightFareSelection.FindAndClickFareCard("Upper Class");

            flightFareSelection.FlexibilityModal("Continue with standard");

            flightFareSelection.FindAndClickFareCard("Upper Class");

            summaryPage.ClickOnContinue();

            var passengerDetailsCount = passengerDetails.ReturnCountOfPassengerDetails();

            Assert.True(passengerDetailsCount > 0);

            passengerDetails.EnterFirstName("firstName");

            passengerDetails.EnterLastName("lastName");

            passengerDetails.SelectDateOfBirth("10", "06", "1982");

            passengerDetails.SelectGender("MALE");

            passengerDetails.EnterEmailAddress("testautomaion@testautomation.com");

            passengerDetails.SelectCountryCode("44");

            passengerDetails.EnterTelephoneNumber("7777777777");

            passengerDetails.SelectNoInsurance();

            passengerDetails.SelectTncsConfirmation();

            passengerDetails.ClickChooseYourSeat();

            seats.FindAndClickAvailableSeat();

            seats.ClickNextFlightButton();

            seats.FindAndClickAvailableSeat();

            seats.ClickConfirmAndPayButton();

            Assert.True(hpp.CheckCardDisplayed(out var result), "Error found. Error: " + string.Join(", ", result.Message));

            Assert.True(hpp.CheckPayPalDisplayed(out result), "Error found. Error: " + string.Join(", ", result.Message));
        }

        public void Dispose()
        {
            // Clean up
            webDriver.DriverCleanUp();
        }
    }
}
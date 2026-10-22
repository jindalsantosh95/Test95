using OpenQA.Selenium;
using System.Diagnostics;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class FlightSearchPanelPageObjects
    {
        private readonly IWebDriver _webDriver;

        public FlightSearchPanelPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnAgreeToCookies()
        {
            if (_webDriver.WaitForElementToBeVisible(FrontEndPageElements.RejectCookies, 5) != null)
                _webDriver.FindElementAndClick(FrontEndPageElements.RejectCookies, "Accept Cookies");
        }

        public void EnterFlyingFrom(string AirportCode)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.FlyingFrom);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.FlyingFrom, AirportCode, "Flying From");
        }

        public void ClickOnPopOverSelector(string AirportCode)
        {
            var popOverSelector = ElementHelper.UpdateXpath(FrontEndPageElements.PopoverSelector, AirportCode);

            Debug.WriteLine(popOverSelector.ToString());

            _webDriver.WaitForElementToBeVisible(popOverSelector);

            _webDriver.FindElementAndClick(popOverSelector, "PopOver Selector");
        }

        public void EnterFlyingTo(string AirportCode)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.FlyingTo);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.FlyingTo, AirportCode, "Flying To");
        }

        public void SelectDepartureAndReturnDate(int DepartureDay, int ReturnDay)
        {
            _webDriver.WaitForElementsToBeVisible(FrontEndPageElements.DepartingDate);

            var departureDay = ElementHelper.GetElement(FrontEndPageElements.CalendarNextMonth, DepartureDay);

            _webDriver.TryFindElementAndClick(departureDay, "Departing Day");

            var returnDay = ElementHelper.GetElement(FrontEndPageElements.CalendarNextMonth, ReturnDay);

            _webDriver.TryFindElementAndClick(returnDay, "Departing Day");
        }

        public void ClickOnSearch()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.SearchButton);

            _webDriver.FindElementAndClick(FrontEndPageElements.SearchButton, "Search Button");
        }
    }
}

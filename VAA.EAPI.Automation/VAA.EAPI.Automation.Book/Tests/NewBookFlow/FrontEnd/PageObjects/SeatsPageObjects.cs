using Microsoft.Graph.Models;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class SeatsPageObjects
    {
        private readonly IWebDriver _webDriver;

        public SeatsPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void FindAndClickAvailableSeat()
        {
            WebDriverExtensions.HumanWait(20000);

            _webDriver.WaitForElementsToBeVisible(FrontEndPageElements.SeatsAvailableForSelection);

            var seats = _webDriver.FindElements(By.XPath(FrontEndPageElements.SeatsAvailableForSelection));

            var seatCodes = seats
                .Select(b => b.GetAttribute("aria-label").Replace("Select seat ", "").Trim())
                .ToList();

            Random rand = new Random();
            string randomSeat = seatCodes[rand.Next(seatCodes.Count)];

            _webDriver.FindElementAndClick($"//button[@aria-label='Select seat {randomSeat}']", "");
        }

        public void ClickNextFlightButton()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.NextFlightButton);

            _webDriver.FindElementAndClick(FrontEndPageElements.NextFlightButton, "Next flight button");
        }

        public void ClickConfirmAndPayButton()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.ConfirmAndPayButton);

            _webDriver.FindElementAndClick(FrontEndPageElements.ConfirmAndPayButton, "Confirm and pay button");
        }
    }
}

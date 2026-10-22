using OpenQA.Selenium;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class FareSelectionPageObjects
    {
        private readonly IWebDriver _webDriver;

        public FareSelectionPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void FindAndClickFareCard(string fare)
        {
            Thread.Sleep(20000);

            var fareCard = ElementHelper.UpdateXpath(FrontEndPageElements.FareCard, fare);

            _webDriver.WaitForElementToBeClickable(fareCard);

            _webDriver.FindElementAndClick(fareCard, "Selecting Fare Card");

            var selectFare = ElementHelper.UpdateXpath(FrontEndPageElements.FareSelection, fare);

            _webDriver.WaitForElementToBeClickable(fareCard);

            _webDriver.FindElementAndClick(selectFare, "Selecting the Fare / Cabin");
        }

        public void FlexibilityModal(string option)
        {
            var buttonOption = ElementHelper.UpdateXpath(FrontEndPageElements.FlexModalSelection, option);

            _webDriver.TryFindElement(buttonOption);

            _webDriver.TryFindElementAndClick(buttonOption, $"Selecting the option {option}");
        }
    }
}

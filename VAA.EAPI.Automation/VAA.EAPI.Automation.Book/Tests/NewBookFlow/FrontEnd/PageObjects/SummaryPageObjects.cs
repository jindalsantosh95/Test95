using Microsoft.Graph.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;
using System.Xml.Linq;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class SummaryPageObjects
    {
        private readonly IWebDriver _webDriver;

        public SummaryPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnContinue()
        {
            _webDriver.WaitForElementsToBeVisible(FrontEndPageElements.PassengerDetailsButton);
            
            _webDriver.FindElementAndClick(FrontEndPageElements.PassengerDetailsButton, "");
        }
    }
}

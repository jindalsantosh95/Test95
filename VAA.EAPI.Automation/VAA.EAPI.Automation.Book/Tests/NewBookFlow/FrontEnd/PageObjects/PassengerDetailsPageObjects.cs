using Microsoft.Graph.Models;
using Microsoft.OData.UriParser;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class PassengerDetailsPageObjects
    {
        private readonly IWebDriver _webDriver;

        public PassengerDetailsPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public int ReturnCountOfPassengerDetails()
        {
            _webDriver.WaitForElementsToBeVisible(FrontEndPageElements.PassengerDetailsInputBlock);

            IReadOnlyCollection<IWebElement> elements = _webDriver.FindElements(By.XPath(FrontEndPageElements.PassengerDetailsInputBlock));

            return elements.Count;
        }

        public void EnterFirstName(string firstName)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.FirstName);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.FirstName, firstName, "FirstName");

            WebDriverExtensions.HumanWait(1000);
        }

        public void EnterLastName(string lastName)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.LastName);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.LastName, lastName, "LastName");

            WebDriverExtensions.HumanWait(1000);
        }

        public void SelectDateOfBirth(string day, string month, string year)
        {
            //Entering day
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.DateOfBirthDay);

            _webDriver.FindElementAndClick(FrontEndPageElements.DateOfBirthDay, "Day dropdown");

            var dayValue = ElementHelper.UpdateXpath(FrontEndPageElements.DayOption, day);

            _webDriver.TryFindElementAndClick(dayValue, "Day");


            //Entering month
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.DateOfBirthMonth);

            _webDriver.FindElementAndClick(FrontEndPageElements.DateOfBirthMonth, "Month dropdown");

            var monthValue = ElementHelper.UpdateXpath(FrontEndPageElements.MonthOption, month);

            _webDriver.TryFindElementAndClick(monthValue, "Month");


            //Entering year
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.DateOfBirthYear);

            _webDriver.FindElementAndClick(FrontEndPageElements.DateOfBirthYear, "Year dropdown");

            var yearValue = ElementHelper.UpdateXpath(FrontEndPageElements.YearOption, year);

            _webDriver.TryFindElementAndClick(yearValue, "Year");
        }

        public void SelectGender(string gender)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.GenderDropdown);

            _webDriver.FindElementAndClick(FrontEndPageElements.GenderDropdown, "Gender dropdown");

            var genderValue = ElementHelper.UpdateXpath(FrontEndPageElements.SelectGenderOption, gender);

            _webDriver.TryFindElementAndClick(genderValue, "Gender");
        }

        public void SelectCountryCode(string countryCode)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.CountryCodeDropDown);

            _webDriver.FindElementAndClick(FrontEndPageElements.CountryCodeDropDown, "Country Code");

            var countryCodeValue = ElementHelper.UpdateXpath(FrontEndPageElements.CountryCodeOption, countryCode);

            _webDriver.TryFindElementAndClick(countryCodeValue, "Country Code");
        }

        public void EnterEmailAddress(string emailAddress)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.EmailAddress);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.EmailAddress, emailAddress, "EmailAddress");

            WebDriverExtensions.HumanWait(1000);
        }

        public void EnterTelephoneNumber(string telephoneNumber)
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.TelephoneNumber);

            _webDriver.FindElementAndEnterText(FrontEndPageElements.TelephoneNumber, telephoneNumber, "Telephone number");

            WebDriverExtensions.HumanWait(1000);
        }

        public void SelectTncsConfirmation()
        {
            WebDriverExtensions.HumanWait(1000);

            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.SelectTncsConfirmation);

            _webDriver.FindElementAndClick(FrontEndPageElements.SelectTncsConfirmation, "");

            WebDriverExtensions.HumanWait(1000);

            bool selected = _webDriver.FindElement(FrontEndPageElements.CheckTncSelected, "").Selected;

            if(!selected)
                _webDriver.FindElementAndClick(FrontEndPageElements.SelectTncsConfirmation, "");
        }

        public void SelectNoInsurance()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.NoInsurance);

            _webDriver.FindElementAndClick(FrontEndPageElements.NoInsurance, "");

            WebDriverExtensions.HumanWait(1000);
        }

        public void SelectYesInsurance()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.YesInsurance);

            _webDriver.FindElementAndClick(FrontEndPageElements.YesInsurance, "");

            WebDriverExtensions.HumanWait(1000);
        }

        public void ClickChooseYourSeat()
        {
            _webDriver.WaitForElementToBeVisible(FrontEndPageElements.ChooseYourSeatButton);

            _webDriver.FindElementAndClick(FrontEndPageElements.ChooseYourSeatButton, "Choose Your Seat Button");
        }
    }
}

using OpenQA.Selenium;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.PageObjects
{
    public class SynchronyApplyPageObjects
    {
        private readonly IWebDriver _webDriver;

        public SynchronyApplyPageObjects(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void WaitForAcceptAllCookiesAndClick()
        {
            // The banner is session/region dependent and is not part of the application flow.
            _webDriver.TryFindElementAndClick(SynchronyApplyPageElements.AcceptAllCookies, "ACCEPT ALL COOKIES", 5);
        }
        public void EnterLast4DigitsSSN(string last4DigitsSSN)
        {
            if (_webDriver.TryFindElement(SynchronyApplyPageElements.Last4DigitsSSN, 15) is null)
            {
                throw new InvalidOperationException(BuildLandingDiagnostic());
            }

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.Last4DigitsSSN, last4DigitsSSN, "Last 4 Digits SSN");
        }

        private string BuildLandingDiagnostic()
        {
            string location = "unavailable";
            if (Uri.TryCreate(_webDriver.Url, UriKind.Absolute, out Uri uri))
            {
                location = $"{uri.Host}{uri.AbsolutePath}";
            }

            string pageText = _webDriver.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? "";
            pageText = string.Join(" ", pageText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            if (pageText.Length > 500)
            {
                pageText = pageText[..500];
            }

            return $"Synchrony landing page did not display the Last 4 Digits SSN control. "
                + $"Location={location}, Title={_webDriver.Title}, PageText={pageText}";
        }

        public void EnterMobileNumber(string mobileNumber)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.MobileNumber);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.MobileNumber, mobileNumber, "Mobile Number");
        }

        public void ClickOnContinueButton()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.ContinueButton);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.ContinueButton, "Continue Button");
        }

        public void ClickOnEditInfoLink()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.EditInfoLink);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.EditInfoLink, "Edit Info Link");
        }

        public void EnterFirstName(string firstName)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.FirstName);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.FirstName, firstName, "First Name");
        }

        public void EnterLastName(string lastName)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.LastName);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.LastName, lastName, "Last Name");
        }

        public void EnterStreetAddress(string streetAddress)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.StreetAddress);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.StreetAddress, streetAddress, "Street Address");
        }

        public void EnterCityAndState(string cityAndState)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.CityAndState);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.CityAndState, cityAndState, "City And State");
        }

        public void EnterZipCode(string zipCode)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.ZipCode);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.ZipCode, zipCode, "Zip Code");
        }

        public void EnterPrimaryPhoneNumber(string primaryPhoneNumber)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.PrimaryPhoneNumber);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.PrimaryPhoneNumber, primaryPhoneNumber, "Primary Phone Number");
        }

        public void EnterSSNNumber(string ssnNumber)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.SSN);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.SSN, ssnNumber, "SSN Number");
        }

        public void EnterDateOfBirth(string dateOfBirth)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.DateOfBirth);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.DateOfBirth, dateOfBirth, "Date Of Birth");
        }

        public void EnterNetIncome(string netIncome)
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.NetIncome);

            _webDriver.FindElementAndEnterText(SynchronyApplyPageElements.NetIncome, netIncome, "NetIncome");
        }
        public void ClickOnHomePhoneRadioButton()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.HomePhone);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.HomePhone, "HomePhone Radio");
        }

        public void ClickOnNextContinueButton()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.NextContinueButton);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.NextContinueButton, "Next Continue Button");
        }

        public void ClickOnContinueToApplyButton()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.ContinueToApplyButton);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.ContinueToApplyButton, "Continue To Apply Button");
        }

        public void ClickOnTermsAndConditionsCheckBox()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.TermsAndConditionsCheckBox);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.TermsAndConditionsCheckBox, "Terms And Conditions CheckBox");
        }

        public void ClickOnAgreeAndSubmitButton()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.AgreeAndSubmitButton);

            _webDriver.FindElementAndClick(SynchronyApplyPageElements.AgreeAndSubmitButton, "Agree And Submit Button");
        }

        public void CheckReturnToVirginRedButtonDisplayed()
        {
            _webDriver.WaitForElementToBeVisible(SynchronyApplyPageElements.ReturnToVirginRedButton);
            _webDriver.FindElement(SynchronyApplyPageElements.ReturnToVirginRedButton, "Return To Virgin Red Button");
        }
    }
}

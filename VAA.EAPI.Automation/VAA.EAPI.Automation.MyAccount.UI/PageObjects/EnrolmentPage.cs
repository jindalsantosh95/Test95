using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;
using Status = AventStack.ExtentReports.Status;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class EnrolmentPage
    {
        private IWebDriver webDriver;

        public EnrolmentPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public string GetVerificationMessage(ExtentTest logger)
        {
            logger.Log(Status.Info, "Successfully retrieved the validation message");

            webDriver.WaitForElementToBeVisible(EnrolmentForm.validationMessage);

            return webDriver.GetElementText(EnrolmentForm.validationMessage);
        }

        public void EnterMemberDetails(ExtentTest logger, string firstName, string lastName, string dateOfBirth)
        {
            webDriver.FindElementAndEnterText(EnrolmentForm.firstNameField, firstName, "FirstName Text Box");

            webDriver.FindElementAndEnterText(EnrolmentForm.lastNameField, lastName, "LastName Text Box");

            webDriver.FindElementAndEnterText(EnrolmentForm.dateOfBirth, dateOfBirth, "Date of Birth Text Box");

            logger.Log(Status.Info, "Entered the Member First name, Last name & Date of Birth");

        }

        public void SelectGender(ExtentTest logger, string gender)
        {
            webDriver.FindElementAndClick(EnrolmentForm.genderRadioButton.Replace("gender", gender), "Gender radio button");

            logger.Log(Status.Info, "Selected the provided gender");

        }

        public void AddAddress(ExtentTest logger, string country, string houseNo, string postCode)
        {
            webDriver.WaitForElementToBeVisible(EnrolmentForm.countryDropDown, 60);

            webDriver.FindElementAndSelectFromDropdown(EnrolmentForm.countryDropDown, country, "Select the country");

            webDriver.FindElementAndClick(EnrolmentForm.postCode, "Click on Post code field");

            webDriver.FindElementAndEnterText(EnrolmentForm.postCode, postCode, "Post Code Text Box");

            webDriver.FindElementAndEnterText(EnrolmentForm.houseNumber, houseNo, "House No Text Box");

            webDriver.FindElementAndClick(EnrolmentForm.findAddressButton, "Find Address Button");

            webDriver.FindElementAndClick(EnrolmentForm.findAddressResult.Replace("postCode", postCode), "Select the address");

            logger.Log(Status.Info, "Selected the address from address look up");

        }

        public void SelectCountryCode(ExtentTest logger, string countryCode)
        {
            var element = webDriver.FindElement(By.XPath(EnrolmentForm.phoneNumber));

            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;

            js.ExecuteScript("arguments[0].scrollIntoView();", element);

            SelectElement dropdown = new SelectElement(webDriver.FindElement(By.XPath(EnrolmentForm.countryCode)));

            dropdown.SelectByValue(countryCode);

            logger.Log(Status.Info, "Selected the Country Code");
        }

        public void EnterPhoneNumber(ExtentTest logger, string phoneNo)
        {
            webDriver.FindElementAndEnterText(EnrolmentForm.phoneNumber, phoneNo, "Phone No Text Box");

            logger.Log(Status.Info, "Phone No is entered");
        }

        public void EnterPassword(ExtentTest logger, string password)
        {
            webDriver.FindElementAndEnterText(EnrolmentForm.password, password, "Password Box");

            logger.Log(Status.Info, "Password is entered");
        }

        public void CheckboxForFlyingClub(ExtentTest logger)
        {
            webDriver.FindElementAndClick(EnrolmentForm.flyingClubNewsPreference, "Check the Flying Club News checkbox");

            logger.Log(Status.Info, "Clicked on Flying Club Prefrence");
        }

        public void CheckboxForVirginRed(ExtentTest logger)
        {
            webDriver.FindElementAndClick(EnrolmentForm.virginRedPreference, "Check the Virgin Red checkbox");

            logger.Log(Status.Info, "Clicked on Virgin Red Prefrence");
        }

        public void SelectTermsAndCondition(ExtentTest logger)
        {
            webDriver.FindElementAndClick(EnrolmentForm.termsAndCondition, "Check the Terms & Condition checkbox");

            logger.Log(Status.Info, "Clicked on Terms & Condition checkbox");
        }

        public void CreateMyAccountEnrolment(ExtentTest logger)
        {
            webDriver.FindElementAndClick(EnrolmentForm.createAccountButton, "Create My Account Button");

            logger.Log(Status.Info, "Clicked on Create My Account Button");
        }

        public string GetFcNumberFromUi(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(EnrolmentForm.successMessage);

            logger.Log(Status.Info, "Get FC No success message");

            return webDriver.GetElementText(EnrolmentForm.successMessage);
        }
    }
}

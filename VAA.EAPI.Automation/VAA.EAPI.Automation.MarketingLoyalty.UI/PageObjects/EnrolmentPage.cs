using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class EnrolmentPage
    {
        private readonly IWebDriver _webDriver;

        public EnrolmentPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnEnrolButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolAddEnrolmentButton, "");

            logger.Log(Status.Info, "Clicked on Enrolment (+) Button ");
        }


        public void EnterMemberDetails(ExtentTest logger, string title, string gender, string firstName, string lastName, string dateOfBirth)
        {
            if (!string.IsNullOrEmpty(title))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolTitleDD, "Title Dropdown");

                _webDriver.FindElementAndClick(Enrolment.EnrolTitleSelect.Replace("Title", title), "Title Select");
            }

            _webDriver.FindElementAndEnterText(Enrolment.EnrolFirstName, firstName, "FirstName Text Box");

            _webDriver.FindElementAndEnterText(Enrolment.EnrolLastName, lastName, "LastName Text Box");

            if (!string.IsNullOrEmpty(gender))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolGenderDD, "Gender Dropdown");

                _webDriver.FindElementAndClick(Enrolment.EnrolOption.Replace("NAME", gender), "");
            }

            _webDriver.FindElementAndEnterText(Enrolment.EnrolDOBTextField, dateOfBirth, "Date Of Birth Text Box");

            logger.Log(Status.Info, "Entered the Member Personal Details");

        }

        public void EnterParentalConsentDetails(string parentName, string parentEmailId, string parentContactCountry, string parentContactNumber)
        {
            _webDriver.FindElementAndEnterText(Enrolment.EnrolGuardianName, parentName, "Guardian/Parent Name");

            _webDriver.FindElementAndEnterText(Enrolment.EnrolGuardianEmail, parentEmailId, "Guardian/Parent EmailId");

            if (!string.IsNullOrEmpty(parentContactNumber))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolGuardianCTCDD,
                    "Guardian/Parent Contact Country Dropdown");

                _webDriver.FindElementAndClick(Enrolment.EnrolGuardianCTCSelect.Replace("Country", parentContactCountry),
                    "Guardian/Parent Contact Country Selection");

                _webDriver.FindElementAndEnterText(Enrolment.EnrolGuardianCTC, parentContactNumber,
                    "Guardian/Parent Contact Country TextField");
            }
        }

        public void EnterMemberAddressDetails(ExtentTest logger, string addressType, string addressLine1, string zipCode, string city, string country)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolAddEnrolmentAddressButton, "Add Address Button");

            if (!string.IsNullOrEmpty(addressType))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolAddressTypeDD, "AddressType Drop down.");

                _webDriver.FindElementAndClick(Enrolment.EnrolOption.Replace("NAME", addressType), "");
            }

            _webDriver.FindElementAndEnterText(Enrolment.EnrolAddressLine1, addressLine1, "Address Line 1 Text Box");

            _webDriver.FindElementAndEnterText(Enrolment.EnrolPostcode, zipCode, "Postcode Text Box");

            _webDriver.FindElementAndEnterText(Enrolment.EnrolTown, city, "City Text Box");

            if (!string.IsNullOrEmpty(country))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolAddressCountryDD, "Country DropDown.");

                _webDriver.FindElementAndClick(Enrolment.EnrolAddressCountrySelect.Replace("Country", country), "Country Selection");
            }

            logger.Log(Status.Info, "Entered the Member Address Details");
        }

        public void EnterAddressUsingQASSearch(ExtentTest logger, string qasCountry, string addressType, string postcode)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolAddEnrolmentAddressButton, "Add Address Button");

            _webDriver.FindElementAndClick(Enrolment.EnrolAddressTypeDD, "AddressType Drop down.");

            _webDriver.FindElementAndClick(Enrolment.EnrolOption.Replace("NAME", addressType), "");

            _webDriver.FindElementAndClick(Enrolment.EnrolQASCountryDD, "QAS Country Dropdown");

            _webDriver.FindElementAndClick(Enrolment.EnrolQASCountrySelect.Replace("Country", qasCountry),
                "QAS Country Select");

            _webDriver.FindElementAndEnterText(Enrolment.EnrolQASPostcodeTextBox, postcode.Replace(" ", ""),
                "QAS Postcode Text Box");

            _webDriver.FindElementAndClick(Enrolment.EnrolQASSearchButton, "QAS Search Button");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            _webDriver.FindElementAndClick(Enrolment.QASResultDropdown, "QAS Result Dropdown");

            _webDriver.FindElementAndClick(Enrolment.QASAddressSelect.Replace("PostCode", postcode), "QAS Address Select");

            _webDriver.FindElementAndClick(Enrolment.EnrolAddressCountryDD, "Country DropDown.");

            _webDriver.FindElementAndClick(Enrolment.EnrolAddressCountrySelect.Replace("Country", qasCountry), "Country Selection");

            logger.Log(Status.Info, "Entered the Member Address Details Using QAS Search");

        }

        public void SaveTheEnteredAddressDetails(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolSaveAddressButton, "Address Save Button");

            logger.Log(Status.Info, "Clicked on Save Button to save entered address Details");
        }

        public void EnterMemberEmailDetails(ExtentTest logger, string emailType, string emailAddress)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolAddEnrolmentEmailButton, "Add Email Button.");

            if (!string.IsNullOrEmpty(emailType))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolEmailUseTypeDD, "Email Use Type Drop down.");

                _webDriver.FindElementAndClick(Enrolment.EnrolOption.Replace("NAME", emailType), "");
            }

            _webDriver.FindElementAndEnterText(Enrolment.EnrolEmail, emailAddress, "Email Address Text Box");

            logger.Log(Status.Info, "Entered the Member Email Details");
        }

        public void SaveTheEnteredEmailDetails(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolSaveEmailButton, "Email Save Button");

            logger.Log(Status.Info, "Clicked on Save Button to save entered Email Details");
        }

        public void EnterMemberPhoneDetails(ExtentTest logger, string phoneUseType, string countryCode, string phoneNumber)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolAddEnrolmentPhoneButton, "Add Phone Button.");

            if (!string.IsNullOrEmpty(phoneUseType))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolPhoneUseTypeDD, "Phone Use Type Drop down.");

                _webDriver.FindElementAndClick(Enrolment.EnrolOption.Replace("NAME", phoneUseType), "");
            }

            if (!string.IsNullOrEmpty(countryCode))
            {
                _webDriver.FindElementAndClick(Enrolment.EnrolPhoneCountryCodeDD, "Phone Country Code Drop down");

                _webDriver.FindElementAndClick(Enrolment.EnrolPhoneCountryCodeSelect.Replace("CountryCode", countryCode), "CountryCode Select");
            }

            _webDriver.FindElementAndEnterText(Enrolment.EnrolPhone, phoneNumber.ToString(), "PhoneNumber Text Box");

            logger.Log(Status.Info, "Entered the Member Phone Details");
        }

        public void SaveTheEnteredPhoneDetails(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(Enrolment.EnrolPhoneSaveButton, "Phone Save Button");

            logger.Log(Status.Info, "Clicked on Save Button to save entered Phone Details");
        }

        public void EnterReasonForTravel(ExtentTest logger, string reasonForTravel)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_webDriver;

            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

            _webDriver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + Keys.End);

            if (!string.IsNullOrEmpty(reasonForTravel))
            {
                _webDriver.WaitForElementToBePresent(Enrolment.EnrolReasonForTravel.Replace("NAME", reasonForTravel), 10);

                _webDriver.FindElement(By.XPath(Enrolment.EnrolReasonForTravel.Replace("NAME", reasonForTravel))).Click();
            }

            logger.Log(Status.Info, "Entered the Reason For Travel");
        }

        public void AcceptFCTermsAndConditions(ExtentTest logger)
        {
            _webDriver.TryFindElementAndClick(Enrolment.EnrolTermAndConditions,
                "TermsAndCondtions CheckBox");

            logger.Log(Status.Info, "Accepted the Flying Club Program Terms and Conditions");
        }

        public string[] EnrolTheMemberIntoFlyingClubProgram()
        {

            _webDriver.TryFindElementAndClick(Enrolment.EnrolButton, "Enrol Submit Button");

            _webDriver.FindElement(Enrolment.EnrolMessage, "EnrolMessage");

            return _webDriver.GetAttributeTextValue(Enrolment.EnrolMessage, "textContent").Split(": ");
        }

        public bool IsEnrolButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Enrolment.EnrolButton, "disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public bool IsAddressDetailsSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Enrolment.EnrolSaveAddressButton, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public bool IsEmailDetailsSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Enrolment.EnrolSaveEmailButton, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public bool IsPhoneDetailsSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Enrolment.EnrolPhoneSaveButton, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }
    }
}

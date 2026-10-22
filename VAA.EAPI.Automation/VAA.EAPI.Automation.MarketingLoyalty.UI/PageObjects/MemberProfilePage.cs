
using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class MemberProfilePage
    {
        private readonly IWebDriver _webDriver;

        public MemberProfilePage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileEditButton, "Member Profile Edit Button On Member Details Page");

            logger.Log(Status.Info, "Clicked on Member Profile Edit Button to Navigate to Profile Update Page");
        }

        public string GetMemberCurrentTitle()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.CurrentMemberTitle, 10);
            }
            catch
            {
                return null;
            }
        }

        public string GetMemberCurrentStatus()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.CurrentMemberStatus, 10);
            }
            catch
            {
                return null;
            }
        }

        public string GetMemberCurrentGender()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.CurrentMemberGender, 10);
            }
            catch
            {
                return null;
            }
        }

        public void ClickOnMenuBarOnMemberProfilePage(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);
        }

        public void SelectNewTitle(ExtentTest logger, string newTitle)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateTitleDropdown, "Member Title Dropdown Button");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateTitleList.Replace("Title", newTitle),
                "Selecting New Status from Dropdown List");

            logger.Log(Status.Info, $"Selected new Member Title {newTitle} from dropdown ");

        }

        public void SelectNewMemberStatus(ExtentTest logger, string newStatus)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateStatusDropdown, "Member Status Dropdown Button");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateStatusList.Replace("Status", newStatus),
                "Selecting New Status from Dropdown List");

            logger.Log(Status.Info, $"Selected new Member status {newStatus} from dropdown ");

        }

        public void SelectNewGenderValue(ExtentTest logger, string gender)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateGenderDropdown, "Member Gender Dropdown Button");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateGenderList.Replace("Gender", gender),
                "Selecting New Gender Value from Dropdown List");

            logger.Log(Status.Info, $"Selected new Member Gender Value {gender} from dropdown ");

        }

        public bool IsProfileUpdateSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(MemberProfile.MemberProfileUpdatePIDetailsSaveButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {MemberProfile.MemberProfileUpdatePIDetailsSaveButton}.");
            }
            //returns true if button is disabled
        }

        public void ClickOnMemberPIDetailsSaveButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePIDetailsSaveButton,
                "Member PI Details Save Button");

            logger.Log(Status.Info, "Clicked on Member Profile PI Details Save Button ");

        }

        public void UpdateMemberFirstName(string firstName)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateFirstNameTextBox, Keys.Clear,
                    "Member FirstName Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateFirstNameTextBox, firstName,
                "Member FirstName Text Box");
        }

        public void UpdateMemberLastName(string lastName)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateLastNameTextBox, Keys.Clear,
                "Member LastName Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateLastNameTextBox, lastName,
                "Member LastName Text Box");
        }

        public void UpdateMemberPrefCardName(string prefCardName)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePrefCardNameTextBox, Keys.Clear,
                "Member Prefered Card Name Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePrefCardNameTextBox, prefCardName,
                "Member Prefered Card Name Text Box");
        }

        public void EnterNameChangeReason(string reason)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.NameChangeReasonTextBox, reason,
                "Name Change Reason Text Box");
        }

        public void UpdateMemberDateOfBirth(string dateOfBirth)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateDOBTextBox, Keys.Clear,
                "Member DateOfBirth Card Name Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateDOBTextBox, dateOfBirth,
                "Member DateOfBirth Card Name Text Box");
        }

        public string GetTextFromAlertPopUpScreen() => _webDriver.GetElementText(MemberProfile.MemberProfileUpdateAlertPopUp);

        public string GetTextFromPopUpScreen() => _webDriver.GetElementText(MemberProfile.MemberProfileUpdatePopUpContent);

        public void ClickOnConfirmYesButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.StatusUpdateConfirmYesButton, "Alert Message Yes Button");
        }

        public void CloseThePopUpScreen(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePopUpCloseButton, "Pop Up Close Button");

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Member Profile Alert Message Pop Up", 15);

            logger.Log(Status.Info, "Closed the Pop Up Message After Updating the Details");
        }

        public void HideMemberProfileDetails()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileDetailsHideButton,
                "Hide Member Profile Details Button");
        }

        public void ClickOnEditAddressButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEditAddressButton,
                "Address Edit Button");
        }

        public void UpdateAddressLine1(string addressLine1)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateAddressLine1, Keys.Clear,
                                "Member AddressLine1 Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateAddressLine1, addressLine1,
                                "Member AddressLine1 Text Box");
        }

        public void UpdateAddressLine2(string addressLine2)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateAddressLine2, Keys.Clear,
                                "Member AddressLine2 Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateAddressLine2, addressLine2,
                                "Member AddressLine2 Text Box");
        }

        public void UpdateCity(string city)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateCity, Keys.Clear,
                                "Member Address City Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateCity, city,
                                "Member Address City Text Box");
        }

        public void UpdatePostalCode(string postCode)
        {
            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePincode, Keys.Clear,
                                "Member Address Pincode Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePincode, postCode,
                                "Member Address Pincode Text Box");
        }

        public void UpdateCountry(string country)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateCountryDropDrown,
                                "Member Address Country Dropdown");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateCountryList.Replace("Country", country),
                                "Member Address Country List");
        }

        public string GetMemberCurrentCountry()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.MemberCurrentCountry, 10);
            }
            catch
            {
                return null;
            }
        }

        public void SaveTheAddressByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateSaveAddressButtonContainer,
                               "Member Address Save Button");
        }

        public void ClickOnEditPhoneDetailsButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEditPhoneDetailsButton,
                "Phone Details Edit Button");
        }

        public string GetMemberCurrentPhoneType()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.MemberCurrentPhoneType, 10);
            }
            catch
            {
                return null;
            }
        }

        public string GetMemberCurrentPhoneCountryCode()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.MemberCurrentPhoneCountryCode, 10);
            }
            catch
            {
                return null;
            }
        }

        public void UpdatePhoneType(string phoneType)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePhoneTypeDropDown,
                                "Member Phone Type Dropdown");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePhoneTypeList.Replace("PhoneType", phoneType),
                                "Member Phone Type List");
        }

        public void UpdatePhoneCountryCode(string phoneCountryCode)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePhoneCountryCodeDropDown,
                                "Member Phone Country Code Dropdown");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePhoneCountryCodeList.Replace("CountryCode", phoneCountryCode),
                                "Member Phone Country Code List");
        }

        public bool IsPhoneValidFlagChecked()
        {
            try
            {
                return _webDriver.FindElement(By.XPath(MemberProfile.MemberProfileUpdatePhoneValidCheckBox)).GetAttribute("checked").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void UpdatePhoneNumber(string phoneNumber)
        {

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePhoneTextBox, Keys.Clear,
                "Member Phone Number Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdatePhoneTextBox, phoneNumber,
                "Member Phone Number Text Box");

            if (!IsPhoneValidFlagChecked())
                _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdatePhoneValidCheckBox, "Phone Valid CheckBox");

        }

        public bool IsPhoneDetailsUpdateSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(MemberProfile.MemberProfileUpdateSavePhoneButtonContainer, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public void SaveThePhoneDetailsByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateSavePhoneButtonContainer,
                               "Member Phone Details Save Button");
        }

        public void ClickOnEditEmailDetailsButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEditEmailButton,
                "Phone Details Edit Button");
        }

        public string GetMemberCurrentEmailUseType()
        {
            try
            {
                return _webDriver.GetElementText(MemberProfile.MemberCurrentEmailUseType, 10);
            }
            catch
            {
                return null;
            }
        }

        public void UpdateEmailUseType(string emailUseType)
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEmailUseTypeDropDown,
                                "Member Email Use Type Dropdown");

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEmailUseTypeList.Replace("EmailUseType", emailUseType),
                                "Member Email Use Type List");
        }

        public bool IsEmailDetailsUpdateSaveButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(MemberProfile.MemberProfileUpdateSaveEmailButtonContainer, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public bool IsAddressDetailsUpdateButtonDisabled()
        {

            _webDriver.FindElementAndClick(MemberProfile.MemberProfileDetailsHideButton, "Member Profile Section Hide Button On Profile Update Page");

            try
            {
                return _webDriver.GetAttributeTextValue(MemberProfile.MemberProfileUpdateEditAddressButton, "aria-disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }


        public bool IsEmailValidFlagChecked()
        {

            try
            {
                return _webDriver.FindElement(By.XPath(MemberProfile.MemberProfileUpdateEmailValidCheckBox)).GetAttribute("checked").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }

        }

        public void UpdateEmailAddress(string emailAddress)
        {

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateEmailTextBox, Keys.Clear,
                "Member Email Number Text Box");

            _webDriver.FindElementAndEnterText(MemberProfile.MemberProfileUpdateEmailTextBox, emailAddress,
                "Member Email Number Text Box");

            if (!IsEmailValidFlagChecked())
                _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateEmailValidCheckBox, "Email Valid CheckBox");

        }

        public void SaveTheEmailDetailsByClickingOnSaveButton()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberProfileUpdateSaveEmailButtonContainer,
                               "Member Email Details Save Button");
        }

        public void ClickOnMemberDetailTab()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDtlsButton, "Member Detail Tab");

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton + "[2]", "Telephone Password Alert ", 5);

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Member Profile Alert Message Pop Up", 10);

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);
        }
    }
}

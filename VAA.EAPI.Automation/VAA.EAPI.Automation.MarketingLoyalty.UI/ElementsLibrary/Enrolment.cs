namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{

    public static class Enrolment
    {
        //Member Enrolment Fields

        public readonly static string EnrolAddEnrolmentButton = "//div[@title='Click to navigate to member enrolment screen']";

        public readonly static string EnrolAddEnrolmentAddressButton = "(//div[@title='Add member address'])[2]";

        public readonly static string EnrolAddEnrolmentEmailButton = "(//div[@title='Add member email'])[2]";

        public readonly static string EnrolAddEnrolmentPhoneButton = "(//div[@title='Add member phone'])[2]";

        public readonly static string EnrolTitleDD = "//*[@id='react-combobox-view-4']";

        public readonly static string EnrolTitleSelect = "//ul/li/div/span[text()='Title']";

        public readonly static string EnrolFirstName = "//input[@appmagic-control='txtMemFirstnametextbox']";

        public readonly static string EnrolLastName = "//input[@appmagic-control='txtMemLastnametextbox']";

        public readonly static string EnrolGenderDD = "//*[@id='react-combobox-view-5']";

        public readonly static string EnrolOption = "//*[contains(@id,'powerapps-flyout-react-combobox-view')]/div/ul/li/div/span[text()='NAME']";

        public readonly static string EnrolDOBTextField = "//*[@data-control-name='calMemDoB']/div/div/div/div/input";

        public readonly static string EnrolCalenderButton = "(//div[@aria-label='Open calendar to select a date'])[1]";

        public readonly static string EnrolCalenderYearDD = "//select[@aria-label='Year']";

        public readonly static string EnrolCalenderYearDDOptions = "//select[@aria-label='Year']/option";

        public readonly static string EnrolCalenderMonthDD = "//select[@aria-label='Month']";

        public readonly static string EnrolCalenderMonthDDOptions = "//select[@aria-label='Month']/option";

        public readonly static string EnrolCalenderDateDD = "//button[@data-pika-day='NAME']";

        public readonly static string EnrolCalenderOkButton = "//button[text()='Ok']";

        public readonly static string EnrolEmailUseTypeDD = "(//*[contains(@id,'react-combobox-view-')]/div[2])[9]";

        public readonly static string EnrolEmail = "//input[@appmagic-control='txtMemNewEmailAddresstextbox']";

        public readonly static string EnrolSaveEmailButton = "//div[@title='Save member email']";

        public readonly static string EnrolSaveEmailButtonContainer = "//div[@data-control-name='btnMemAddEmailSave']/div";

        public readonly static string EnrolPhoneUseTypeDD = "(//*[contains(@id,'react-combobox-view-')]/div[2])[10]";

        public readonly static string EnrolPhoneCountryCodeDD = "(//*[contains(@id,'react-combobox-view-')]/div[2])[11]";

        public readonly static string EnrolPhoneCountryCodeSelect = "//ul/li/div/span[text()='CountryCode']";

        public readonly static string EnrolPhone = "//input[@appmagic-control='txtMemNewPhoneNumbertextbox']";

        public readonly static string EnrolPhoneSaveButton = "//div[@title='Save member phone']";

        public readonly static string EnrolFlyingClubComm = "//input[@appmagic-control='chMemFCComms']/parent::label/div[1]";

        public readonly static string EnrolReasonForTravel = "//input[@type='radio' and @value='NAME']";

        public readonly static string EnrolTermAndConditions = "//input[@appmagic-control='chMemEnrolmentTerms']/parent::label/div[1]";

        public readonly static string EnrolAddressTypeDD = "//div[@data-control-name='cbAddressType']/div/div/div/div/div[2]";

        public readonly static string EnrolAddressCountryDD = "(//*[contains(@id,'react-combobox-view-')]/div[2])[8]";

        public readonly static string EnrolAddressCountrySelect = "//ul/li/div/span[text()='Country']";

        public readonly static string EnrolPostcode = "//input[@appmagic-control='txtAddressPostCodetextbox']";

        public readonly static string EnrolTown = "//input[@appmagic-control='txtAddressCitytextbox']";

        public readonly static string EnrolAddressLine1 = "//input[@appmagic-control='txtAddressLine1textbox']";

        public readonly static string EnrolAddressLine2 = "//input[@appmagic-control='txtAddressLine2textbox']";

        public readonly static string EnrolSaveAddressButton = "//div[@title='Save member address']";

        public readonly static string EnrolSaveAddressButtonContainer = "//div[@data-control-name='btnMemAddAddressSave']/div";

        public readonly static string EnrolSavePhoneButtonContainer = "//div[@data-control-name='btnMemAddPhoneSave']/div";

        public readonly static string EnrolButton = "//button[@title='Click to enrol the member']";

        public readonly static string EnrolMessage = "//div[@data-control-name='ContPopupMessageHTMLPopup']/div/div/div/div/div/div/div/div/div/div/div/div";

        public readonly static string EnrolGeneratedAccountNumber = "//div[@data-control-name='ContPopupMessageHTMLPopup']/div/div/div/div/div/div/div/div/div/div/div/div/b[1]";

        public readonly static string EnrolMemberStatus = "//div[@data-control-name='ContPopupMessageHTMLPopup']/div/div/div/div/div/div/div/div/div/div/div/div/b[2]";

        public readonly static string EnrolQASCountryDD = "//div[@data-control-name='cbQASCountry']/div/div/div/div/div[2]";

        public readonly static string EnrolQASCountrySelect = "//ul/li/div/span[text()='Country']";

        public readonly static string EnrolQASPostcodeTextBox = "//*[@appmagic-control = 'txtQASPostCodetextbox']";

        public readonly static string EnrolQASSearchButton = "//*[@title= 'Search addresses in QAS']";

        public readonly static string QASResultDropdown = "//div[@data-control-name='cbQASResults']/div/div/div/div/div[2]";

        public readonly static string QASAddressSelect = "(//ul/li/div/span[contains(text(),'PostCode')])[1]";

        public readonly static string EnrolGuardianEmail = "//input[@appmagic-control='txtMemParentEmailtextbox']";

        public readonly static string EnrolGuardianName = "//input[@appmagic-control='txtMemParentNametextbox']";

        public readonly static string EnrolBtnContainer = "//button[@title='Click to enrol the member']/div";

        public readonly static string EnrolGuardianCTCDD = "(//*[contains(@id,'react-combobox-view-')]/div[2])[4]";

        public readonly static string EnrolGuardianCTCSelect = "//ul/li/div/span[text()='Country']";

        public readonly static string EnrolGuardianCTC = "//input[@appmagic-control='txtMemParentCTCtextbox']";

        public readonly static string EnrolPopUpCloseButton = "//div[@data-control-name='htmlPopupCloseIcon']/div/div/div/div";

        public readonly static string UndisclosedOption = "//*[@id='powerapps-flyout-react-combobox-view-5']/div/ul/li[4]/div/span";

        public readonly static string UnspecifiedOption = "//*[@id='powerapps-flyout-react-combobox-view-5']/div/ul/li[5]/div/span";

    }

    public enum ReasonForTravel
    {
        Business,
        Leisure,
        Both
    }
}

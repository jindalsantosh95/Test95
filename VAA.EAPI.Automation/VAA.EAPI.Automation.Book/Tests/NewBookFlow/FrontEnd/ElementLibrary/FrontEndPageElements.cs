using System.Security.Policy;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd.ElementLibrary
{
    public class FrontEndPageElements
    {
        public readonly static string RejectCookies = "//button[@id='ensRejectAll']";
        public readonly static string FlyingFrom = "//*[@id='flights_from']";

        public readonly static string FlyingTo = "//*[@id='flights_to']";
        public readonly static string PopoverSelector = "//button[@role='option'][contains(normalize-space(.), '({0})')]";
        public readonly static string DepartingDate = "//*[@id='flights_departing']";

        public readonly static string CalendarNextMonth = "//*[@id='popover-when']/section/div/section/section/div[2]/div[3]/div/button[{0}]";

        public readonly static string SearchButton = "//button[@data-cy='submit-button']";

        public readonly static string FareCard = "//p[@data-cy='fare-label' and normalize-space()='{0}']";
        public readonly static string FareSelection = "//h2[normalize-space()='{0}']/ancestor::article[1]//footer//button[normalize-space()='Select']";

        public readonly static string FlexModalSelection = "//*[@data-testid='modal-component']//button[@aria-label='{0}' or normalize-space()='{0}']";

        public readonly static string PassengerDetailsButton = "//button[normalize-space()='Continue to passenger details']";
        

        public readonly static string PassengerDetailsInputBlock = "//*[@class='block-component passenger-component block-invalid']";
        public readonly static string FirstName = "//*[@id='passenger-0-first-name']";
        public readonly static string LastName = "//*[@id='passenger-0-last-name']";
        public readonly static string DateOfBirthDay = "//*[@id='day-select']";
        public readonly static string DayOption = "//*[@id='day-select']/option[@value='{0}']";
        public readonly static string DateOfBirthMonth = "//*[@id='month-select']";
        public readonly static string MonthOption = "//*[@id='month-select']/option[@value='{0}']";
        public readonly static string DateOfBirthYear = "//*[@id='year-select']";
        public readonly static string YearOption = "//*[@id='year-select']/option[@value='{0}']";
        public readonly static string GenderDropdown = "//*[@id='passenger-0-gender']";
        public readonly static string SelectGenderOption = "//*[@id='passenger-0-gender']/option[@value='{0}']";
        public readonly static string CountryCodeDropDown = "//*[@id='countryCode']";
        public readonly static string TelephoneNumber = "//*[@id='telephone']";
        public readonly static string CountryCodeOption = "//*[@id='countryCode']/option[@value='{0}']";
        public readonly static string EmailAddress = "//*[@id='email']";

        public readonly static string NoInsurance = "//label[@for='noProtectTrip']";
        public readonly static string YesInsurance = "//span[contains(normalize-space(.), 'Yes, protect my trip')]";
        public readonly static string SelectTncsConfirmation = "//label[@for='acceptTerms' and contains(normalize-space(.), 'I confirm that I have read')]";
        public readonly static string CheckTncSelected = "//input[@id='acceptTerms']";


        public readonly static string ChooseYourSeatButton = "//button[@data-testid='button-component' and contains(.,'Choose your seat')]";
        public readonly static string SeatsAvailableForSelection = "//div[@data-seat-map='true']//button[starts-with(@aria-label,'Select seat ')]";

        public readonly static string NextFlightButton = "//a[@data-testid='button-component' and normalize-space()='Next flight']";

        public readonly static string ConfirmAndPayButton = "//button[@data-testid='button-component' and normalize-space()='Confirm and pay']";
    }

    public static class ElementHelper
    {
        public static string GetElement(string element, int index)
        {
            return string.Format(element, index);
        }

        public static string UpdateXpath(string baseXpath, string newId)
        {
            return string.Format(baseXpath, newId);
        }
    }
}
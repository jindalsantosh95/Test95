using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using Status = AventStack.ExtentReports.Status;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class MemberSearchPage
    {
        private readonly IWebDriver _webDriver;

        public MemberSearchPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void SwitchToPowerAppIFrame()
        {
            try
            {
                _webDriver.WaitForElementToBeClickable(MemberSearch.SignInBtn, 5);
                _webDriver.FindElementAndClick(MemberSearch.SignInBtn, "The SSO SignIn Button", 5);
            }
            catch (Exception)
            {
                //Ignore
            }

            _webDriver.SwitchToAFrame(MemberSearch.SearchPageIframeName);
        }


        public void WaitForPowerAppsToBeLoadedCompletely(ExtentTest logger)
        {
            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            logger.Log(Status.Info, "PowerApps Member Search Page Loaded");
        }

        public void EnterFCMemberNumberAndSearch(ExtentTest logger, string memberNumber)
        {

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemAcctNum, memberNumber, "MemberNumber Text Box on search page");

            _webDriver.FindElementAndClick(MemberSearch.SrchMemSrchBtn, "Search Button on Member Search Page");

            logger.Log(Status.Info, "Entered the FC Number and Clicked on Search Button");

        }

        public void ClickOnMemberNumberToNavigateToMemberProfile(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberSearchResultTableDetails.SrAcctNum, "MemberNumber from Search Results");

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton + "[2]", "Telephone Password Alert ", 10);

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Member Profile Alert Message Pop Up", 20);

            _webDriver.SwitchTo().DefaultContent();

            SwitchToPowerAppIFrame();

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);

            logger.Log(Status.Info, "Clicked on MemberNumber and Navigated to Member Profile Page.");
        }

        public void ClickOnVACMemberNumberToNavigateToVACMemberProfile(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberSearchResultTableDetails.SrAccName, "MemberName from Search Results");

            _webDriver.SwitchTo().DefaultContent();

            SwitchToPowerAppIFrame();

            _webDriver.WaitForElementToBePresent(MemberDetail.VacMemberDetailsScreenLoadStatus);

            logger.Log(Status.Info, "Clicked on MemberNumber and Navigated to Member Profile Page.");
        }

        public string GetFCNumberFromSearchResult() => _webDriver.GetElementText(MemberSearchResultTableDetails.SrAcctNum);

        public void ClickOnPowerAppsHomeButton(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberSearch.HomeButton, "Home Button on PowerApps");

            logger.Log(Status.Info, "Clicked on PowerApps Home Button.");
        }

        public void ClearTheSearchParametersAndResults(ExtentTest logger)
        {
            _webDriver.FindElementAndClick(MemberSearch.SrchMemClrBtn, "Clear Button on Member Search Page");

            logger.Log(Status.Info, "Clicked on Clear Button on Member Search Page.");
        }

        public void MemberSearchUsingFCNumber(string memberNumber)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemAcctNum, memberNumber,
                "FC Number Text Box on Member Search Page");
        }

        public void MemberSearchUsingEmailAddress(string emailAddress)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemEmail, emailAddress,
                "EmailAddress Text Box on Member Search Page");
        }


        public void MemberSearchUsingFirstNameAndSurName(string firstName, string surName)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemFName, firstName,
                "FirstName Text Box on Member Search Page");

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemSName, surName,
                "SurName Text Box on Member Search Page");
        }

        public void MemberSearchUsingFirstNameAndDateOfBirth(string firstName, string dateOfBirth)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemFName, firstName,
                "FirstName Text Box on Member Search Page");

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemDOB, dateOfBirth,
                "DateOfBirth Text Box on Member Search Page");
        }


        public void MemberSearchUsingSurNameAndDateOfBirth(string surName, string dateOfBirth)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemSName, surName,
                 "SurName Text Box on Member Search Page");

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemDOB, dateOfBirth,
                "DateOfBirth Text Box on Member Search Page");
        }

        public void MemberSearchUsingSurNameAndPostCode(string surName, string postCode)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemSName, surName,
                 "SurName Text Box on Member Search Page");

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemPstCde, postCode,
                "PostCode Text Box on Member Search Page");
        }


        public void MemberSearchUsingSurNameAndDateOfBirthAndTier(string surName, string dateOfBirth, string tier)
        {
            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemSName, surName,
                 "SurName Text Box on Member Search Page");

            _webDriver.FindElementAndEnterText(MemberSearch.SrchMemDOB, dateOfBirth,
                 "DateOfBirth Text Box on Member Search Page");

            _webDriver.FindElementAndClick(MemberSearch.TierDropdown,
                "Tier Dropdown on Member Search Page");

            _webDriver.FindElementAndClick(MemberSearch.SelectTier.Replace("TierCode", tier),
                "Tier Selection from Dropdown on Member Search Page");
        }

        public void ClickOnSearchButton()
        {
            _webDriver.FindElementAndClick(MemberSearch.SrchMemSrchBtn,
               " Search Button on Member Search Page");
        }
    }
}

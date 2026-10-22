using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{

    [Collection("MemberProfileUpdateTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberProfileUpdateTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private MemberProfilePage memberProfilePage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public MemberProfileUpdateTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);
            memberProfilePage = new MemberProfilePage(webDriver);

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_MemberFirstNameUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_MemberFirstNameUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string dateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.MemberDOB, "value");

            Assert.True(!string.IsNullOrEmpty(dateOfBirth), "Date of birth value is empty");

            int age = DateTime.UtcNow.Subtract(DateTime.Parse(dateOfBirth)).Days;

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            if (string.IsNullOrEmpty(memberProfilePage.GetMemberCurrentTitle()))

                memberProfilePage.SelectNewTitle(logger, "Mr");

            string firstName = DataGenerator.RandomFirstName();

            memberProfilePage.UpdateMemberFirstName(firstName);

            memberProfilePage.EnterNameChangeReason("Test");

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {
                string lastName = DataGenerator.RandomLastName();

                memberProfilePage.UpdateMemberLastName(lastName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = age > 6575 ? "Member details updated successfully!" :
             "Member and Parental consent  updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualFirstName = webDriver.GetAttributeTextValue(MemberDetail.MemberFirstName, "value");

            Assert.True(firstName.Equals(actualFirstName),
                $"Expected the FirstName after the Update to be {firstName} but found {actualFirstName}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_MemberTitleUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_MemberTitleUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string dateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.MemberDOB, "value");

            Assert.True(!string.IsNullOrEmpty(dateOfBirth), "Date of birth value is empty");

            int age = DateTime.UtcNow.Subtract(DateTime.Parse(dateOfBirth)).Days;

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            string currentTitle = memberProfilePage.GetMemberCurrentTitle() ?? "";

            //List of few Valid Titles
            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };

            Random random = new();

            string newTitle = titles[random.Next(titles.Length)];

            while (currentTitle == newTitle)
            {
                newTitle = titles[random.Next(titles.Length)];
            }

            memberProfilePage.SelectNewTitle(logger, newTitle);

            string firstName = DataGenerator.RandomFirstName();

            string lastName = DataGenerator.RandomLastName();


            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {
                memberProfilePage.UpdateMemberFirstName(firstName);

                memberProfilePage.UpdateMemberLastName(lastName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);

                memberProfilePage.EnterNameChangeReason("Test");
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = age > 6575 ? "Member details updated successfully!" :
                "Member and Parental consent  updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualTitle = webDriver.GetAttributeTextValue(MemberDetail.MemberTitle, "value");

            Assert.True(newTitle.Equals(actualTitle),
                $"Expected the Title after the Update to be {newTitle} but found {actualTitle}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_MemberLastNameUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC03_MemberLastNameUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string dateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.MemberDOB, "value");

            Assert.True(!string.IsNullOrEmpty(dateOfBirth), "Date of birth value is empty");

            int age = DateTime.UtcNow.Subtract(DateTime.Parse(dateOfBirth)).Days;

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            if (string.IsNullOrEmpty(memberProfilePage.GetMemberCurrentTitle()))

                memberProfilePage.SelectNewTitle(logger, "Mr");

            string lastName = DataGenerator.RandomLastName();

            memberProfilePage.UpdateMemberLastName(lastName);

            memberProfilePage.EnterNameChangeReason("Test");

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {
                string firstName = DataGenerator.RandomFirstName();

                memberProfilePage.UpdateMemberFirstName(firstName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = age > 6575 ? "Member details updated successfully!" :
                "Member and Parental consent  updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualLastName = webDriver.GetAttributeTextValue(MemberDetail.MemberLastName, "value");

            Assert.True(lastName.Equals(actualLastName),
                $"Expected the LastName after the Update to be {lastName} but found {actualLastName}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_MemberDateOfBirthUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC04_MemberDateOfBirthUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            if (string.IsNullOrEmpty(memberProfilePage.GetMemberCurrentTitle()))

                memberProfilePage.SelectNewTitle(logger, "Mr");

            Random random = new();

            DateTime startDate = DateTime.Now.AddYears(-18);

            DateTime endDate = startDate.AddYears(-20);

            int range = (startDate - endDate).Days;

            string dateOfBirth = endDate.AddDays(random.Next(range)).ToString("dd-MMM-yyyy");

            memberProfilePage.UpdateMemberDateOfBirth(dateOfBirth);

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {

                string firstName = DataGenerator.RandomFirstName();

                string lastName = DataGenerator.RandomLastName();

                memberProfilePage.UpdateMemberFirstName(firstName);

                memberProfilePage.UpdateMemberLastName(lastName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);

                memberProfilePage.EnterNameChangeReason("Test");
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = "Member details updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualDateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.MemberDOB, "value");

            Assert.True(dateOfBirth.Equals(actualDateOfBirth),
                $"Expected the Date Of Birth after the Update to be {dateOfBirth} but found {actualDateOfBirth}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_MemberStatusUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC05_MemberStatusUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            if (string.IsNullOrEmpty(memberProfilePage.GetMemberCurrentTitle()))

                memberProfilePage.SelectNewTitle(logger, "Mr");

            string currentStatus = memberProfilePage.GetMemberCurrentStatus() ?? "";

            //List of few Valid Status Values
            string[] statuses = new string[] { "Active", "Active Pending", "Inactive", "Frozen", "Suspended" };

            Random random = new();

            string newStatus = statuses[random.Next(statuses.Length)];

            while (currentStatus == newStatus)
            {
                newStatus = statuses[random.Next(statuses.Length)];
            }

            memberProfilePage.SelectNewMemberStatus(logger, newStatus);

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {

                string firstName = DataGenerator.RandomFirstName();

                string lastName = DataGenerator.RandomLastName();

                memberProfilePage.UpdateMemberFirstName(firstName);

                memberProfilePage.UpdateMemberLastName(lastName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);

                memberProfilePage.EnterNameChangeReason("Test");
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = "Member details updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualStatus = webDriver.GetAttributeTextValue(MemberDetail.MemberStatus, "value");

            Assert.True(newStatus.Equals(actualStatus),
                $"Expected the Status after the Update to be {newStatus} but found {actualStatus}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_MemberGenderUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC06_MemberGenderUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            if (string.IsNullOrEmpty(memberProfilePage.GetMemberCurrentTitle()))

                memberProfilePage.SelectNewTitle(logger, "Mr");

            string currentGender = memberProfilePage.GetMemberCurrentGender() ?? "";

            //List of valid Gender Values
            string[] genderValues = new string[] { "Male", "Female", "Undisclosed", "Unspecified" };

            Random random = new();

            string gender = genderValues[random.Next(genderValues.Length)];

            while (currentGender == gender)
            {
                gender = genderValues[random.Next(genderValues.Length)];
            }

            memberProfilePage.SelectNewGenderValue(logger, gender);

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {

                string firstName = DataGenerator.RandomFirstName();

                string lastName = DataGenerator.RandomLastName();

                memberProfilePage.UpdateMemberFirstName(firstName);

                memberProfilePage.UpdateMemberLastName(lastName);

                memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);

                memberProfilePage.EnterNameChangeReason("Test");
            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = "Member details updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(7)]
        public void TC07_MemberAddressLine1Update()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC07_MemberAddressLine1Update");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditAddressButton();

            string addressLine1 = "TestAddressLine1" + DataGenerator.RandomString(5).ToLower();

            memberProfilePage.UpdateAddressLine1(addressLine1);

            memberProfilePage.SaveTheAddressByClickingOnSaveButton();

            string expectedMessage = "Member address updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualAddressLine1 = webDriver.GetAttributeTextValue(MemberDetail.MemberAddressLine1, "value");

            Assert.True(addressLine1.Equals(actualAddressLine1),
                $"Expected the AddressLine1 after the Update to be {addressLine1} but found {actualAddressLine1}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(8)]
        public void TC08_MemberAddressLine2Update()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_MemberAddressLine2Update");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditAddressButton();

            string addressLine2 = "TestAddressLine2" + DataGenerator.RandomString(5).ToLower();

            memberProfilePage.UpdateAddressLine2(addressLine2);

            memberProfilePage.SaveTheAddressByClickingOnSaveButton();

            string expectedMessage = "Member address updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualAddressLine2 = webDriver.GetAttributeTextValue(MemberDetail.MemberAddressLine2, "value");

            Assert.True(actualAddressLine2.Contains(addressLine2),
                $"Expected the AddressLine2 {addressLine2} after the Update to be part of {actualAddressLine2}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(9)]
        public void TC09_MemberAddressCityUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC09_MemberAddressCityUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditAddressButton();

            string city = "TestCity" + DataGenerator.RandomString(4).ToLower();

            memberProfilePage.UpdateCity(city);

            memberProfilePage.SaveTheAddressByClickingOnSaveButton();

            string expectedMessage = "Member address updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualCity = webDriver.GetAttributeTextValue(MemberDetail.MemberCity, "value");

            Assert.True(city.Equals(actualCity),
                $"Expected the City after the Update to be {city} but found {actualCity}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(10)]
        public void TC10_MemberAddressPostalCodeUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC10_MemberAddressPostalCodeUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditAddressButton();

            string postalCode = DataGenerator.RandomString(2).ToUpper() + DataGenerator.RandomNumber(10, 99, true) + " " + DataGenerator.RandomString(3, true).ToUpper();

            memberProfilePage.UpdatePostalCode(postalCode);

            memberProfilePage.SaveTheAddressByClickingOnSaveButton();

            string expectedMessage = "Member address updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualPostCode = webDriver.GetAttributeTextValue(MemberDetail.MemberPostalCode, "value");

            Assert.True(postalCode.Equals(actualPostCode),
                $"Expected the PostCode after the Update to be {postalCode} but found {actualPostCode}.");

        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(11)]
        public void TC11_MemberAddressCountryUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC11_MemberAddressCountryUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditAddressButton();

            string currentCountry = memberProfilePage.GetMemberCurrentCountry();

            string newCountry = currentCountry.Equals("United Kingdom") ? "United States Of America" : "United Kingdom";

            memberProfilePage.UpdateCountry(newCountry);

            memberProfilePage.SaveTheAddressByClickingOnSaveButton();

            string expectedMessage = "Member address updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            string actualCountry = webDriver.GetAttributeTextValue(MemberDetail.MemberCountry, "value");

            Assert.True(newCountry.Equals(actualCountry),
                $"Expected the Country after the Update to be {newCountry} but found {actualCountry}.");

        }


        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(12)]
        public void TC12_MemberPhoneNumberUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC12_MemberPhoneNumberUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditPhoneDetailsButton();

            string phoneNumber = DataGenerator.RandomNumber(000000000, 99999999);

            memberProfilePage.UpdatePhoneNumber(phoneNumber);

            if (memberProfilePage.IsPhoneDetailsUpdateSaveButtonDisabled())
            {
                string phoneType = memberProfilePage.GetMemberCurrentPhoneType();

                if (string.IsNullOrEmpty(phoneType))
                    memberProfilePage.UpdatePhoneType("Mobile");

                string phoneCountryCode = memberProfilePage.GetMemberCurrentPhoneCountryCode();

                if (string.IsNullOrEmpty(phoneCountryCode))
                    memberProfilePage.UpdatePhoneCountryCode("United Kingdom (+44)");

            }

            memberProfilePage.SaveThePhoneDetailsByClickingOnSaveButton();

            string expectedMessage = "Member phone updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");


        }

        [Trait("Category", Traits.MemberProfileUpdateUIRegression)]
        [Fact, RunOrder(13)]
        public void TC13_MemberEmailUpdate()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC13_MemberEmailUpdate");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            memberProfilePage.HideMemberProfileDetails();

            memberProfilePage.ClickOnEditEmailDetailsButton();

            string emailAddress = "Test" + DataGenerator.RandomString(10) + "@gmail.co.in";

            memberProfilePage.UpdateEmailAddress(emailAddress);

            if (memberProfilePage.IsEmailDetailsUpdateSaveButtonDisabled())
            {
                string emailUseType = memberProfilePage.GetMemberCurrentEmailUseType();

                if (string.IsNullOrEmpty(emailUseType))
                    memberProfilePage.UpdateEmailUseType("Business");

            }

            memberProfilePage.SaveTheEmailDetailsByClickingOnSaveButton();

            string expectedMessage = "Member email updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            memberProfilePage.CloseThePopUpScreen(logger);

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            string actualEmailAddress = webDriver.GetAttributeTextValue(MemberDetail.MemberEmail, "value");

            Assert.True(emailAddress.Equals(actualEmailAddress, StringComparison.InvariantCultureIgnoreCase),
                $"Expected the Email Address after the Update to be {emailAddress} but found {actualEmailAddress}.");


        }


        public string GetValidFCNumber()
        {
            string queryForFCMDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.MemberType = @memberType and c.Status = @status and c.SSAMemberNumber = '' and c.HeadOfGroupMemberNumber = '' order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@memberType", "Individual").WithParameter("@status", "Active");

            IEnumerable<Core.DB_Model.Entities.FlyingClubMember> fcmDocs = cosmosFixture.CosmosQueryItems<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            return fcmDocs.FirstOrDefault(x => !(x.RetainGoldCounter.Contains(".") && x.RetainSilvercounter.Contains(".")) && string.IsNullOrEmpty(x.TelephonePassword)).MemberNumber;
        }

        public void Dispose()
        {
            // Flush the report
            reportFixture.Current.Flush();
            // Close browser
            webDriver.CloseBrowser();
        }
    }
}

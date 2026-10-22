using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("EnrolmentTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class EnrolmentTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {

        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private EnrolmentPage enrolmentPage;
        private MemberSearchPage memberSearchPage;
        private readonly Random random;

        private readonly string customerCosmosDb;

        public string env = Config.Environment;

        public EnrolmentTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            enrolmentPage = new EnrolmentPage(webDriver);

            memberSearchPage = new MemberSearchPage(webDriver);

            random = new();

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }


        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("minor")]
        [InlineData("major")]
        public void EnrolmentSuccess(string memberAge)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"EnrolmentFor{memberAge}Success");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            int minorDateRange = Convert.ToInt32(DateTime.UtcNow.Subtract(enddate).TotalDays);

            string dob = memberAge.Equals("major") ? startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy") :
                enddate.AddDays(random.Next(minorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@test.noreply.inc.com";

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", DataGenerator.RandomFirstName(), DataGenerator.RandomLastName(), dob);

            if (memberAge.Equals("minor"))
                enrolmentPage.EnterParentalConsentDetails("Test Parent", emailAddress.Replace("test", "testparent"), "United Kingdom (+44)", DataGenerator.RandomNumber(10).ToString());

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "4424240090");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, ReasonForTravel.Both.ToString());

            enrolmentPage.AcceptFCTermsAndConditions(logger);

            string[] text = enrolmentPage.EnrolTheMemberIntoFlyingClubProgram();

            string expectedMessage = "Congratulations, member enrolment was successful.";

            Assert.True(text[0].Replace("Membership Number ", "").Contains(expectedMessage),
                "Message for Member enrolment as " + expectedMessage + " is not Visible on page");

            Assert.True(text[2].Equals("Active"),
                "Member with status as " + expectedMessage + " is not Visible on page");

        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Fact, RunOrder(2)]
        public void EnrolmentSuccessUsingQASSearch()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("EnrolmentUIRegression");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@test.noreply.inc.com";

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterAddressUsingQASSearch(logger, "United Kingdom", "Home", "RH11 7DU");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "442424");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, ReasonForTravel.Both.ToString());

            enrolmentPage.AcceptFCTermsAndConditions(logger);

            string[] text = enrolmentPage.EnrolTheMemberIntoFlyingClubProgram();

            string expectedMessage = "Congratulations, member enrolment was successful.";

            Assert.True(text[0].Replace("Membership Number ", "").Contains(expectedMessage),
                "Message for Member enrolment as " + expectedMessage + " is not Visible on page");

            Assert.True(text[2].Equals("Active"),
                "Member with status as " + expectedMessage + " is not Visible on page");

        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(3)]
        [InlineData("title")]
        [InlineData("firstName")]
        [InlineData("lastName")]
        [InlineData("gender")]
        [InlineData("dateOfBirth")]
        [InlineData("reasonForTravel")]
        public void ValidateEnrolButtonWhenMandatoryFieldValuePassedAsNullorEmpty(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"ValidateEnrolButtonWhenMandatoryField-{mandatoryField}-ValuePassedAsNullorEmpty");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int range = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(range)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15).ToLower() + "@test.noreply.inc.com";

            string title = "Mr";

            string firstName = "TestFirstName";

            string lastName = "TestLastName";

            string gender = "Male";

            string reasonForTravel = ReasonForTravel.Leisure.ToString();

            switch (mandatoryField)
            {
                case "title":
                    title = "";
                    break;

                case "firstName":
                    firstName = "";
                    break;

                case "lastName":
                    lastName = "";
                    break;

                case "gender":
                    gender = "";
                    break;

                case "dateOfBirth":
                    dob = "";
                    break;

                case "reasonForTravel":
                    reasonForTravel = "";
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, title, gender, firstName, lastName, dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "442424");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, reasonForTravel);

            enrolmentPage.AcceptFCTermsAndConditions(logger);

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
                "Expected the Enrol Member Button to be in Disabled State but found Enabled");

        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(4)]
        [InlineData("phoneUseType")]
        [InlineData("phoneCountryCode")]
        [InlineData("phone")]
        public void ValidatePhoneDetailsSaveButtonWhenMandatoryFieldValuePassedAsNullorEmpty(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"ValidatePhoneDetailsSaveButtonWhenMandatoryField-{mandatoryField}-ValuePassedAsNullorEmpty");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@test.noreply.inc.com";

            string phoneUseType = "Mobile";

            string phoneCountryCode = "United Kingdom (+44)";

            string phoneNumber = DataGenerator.RandomNumber(10).ToString();

            switch (mandatoryField)
            {
                case "phoneUseType":
                    phoneUseType = "";
                    break;

                case "phoneCountryCode":
                    phoneCountryCode = "";
                    break;

                case "phone":
                    phoneNumber = "";
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, phoneUseType, phoneCountryCode, phoneNumber);

            Assert.True(enrolmentPage.IsPhoneDetailsSaveButtonDisabled(),
                $"Expected the Phone Details Save Button to be disabled when Mandatory field {mandatoryField} id null or empty");

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
               "Expected the Enrol Member Button to be in Disabled State but found Enabled");
        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(5)]
        [InlineData("emailUseType")]
        [InlineData("email")]
        public void ValidateEmailDetailsSaveButtonWhenMandatoryFieldValuePassedAsNullorEmpty(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"ValidateEmailDetailsSaveButtonWhenMandatoryField-{mandatoryField}-ValuePassedAsNullorEmpty");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@test.noreply.inc.com";

            string emailUseType = "Personal";

            switch (mandatoryField)
            {
                case "emailUseType":
                    emailUseType = "";
                    break;

                case "email":
                    emailAddress = "";
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, emailUseType, emailAddress);

            Assert.True(enrolmentPage.IsEmailDetailsSaveButtonDisabled(),
                $"Expected the Email Details Save Button to be disabled when Mandatory field {mandatoryField} id null or empty");

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
               "Expected the Enrol Member Button to be in Disabled State but found Enabled");
        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(6)]
        [InlineData("addressType")]
        [InlineData("addressLine1")]
        [InlineData("postCode")]
        [InlineData("city")]
        [InlineData("country")]
        public void ValidateAddressDetailsSaveButtonWhenMandatoryFieldValuePassedAsNullorEmpty(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"ValidateAddressDetailsSaveButtonWhenMandatoryField-{mandatoryField}-ValuePassedAsNullorEmpty");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string addressType = "Home";

            string addressLine1 = "Test Street 101";

            string postCode = "RH11 7DU";

            string city = "Crawley";

            string country = "United Kingdom";

            switch (mandatoryField)
            {
                case "addressType":
                    addressType = "";
                    break;

                case "addressLine1":
                    addressLine1 = "";
                    break;

                case "postCode":
                    postCode = "";
                    break;

                case "city":
                    city = "";
                    break;

                case "country":
                    country = "";
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, addressType, addressLine1, postCode, city, country);

            Assert.True(enrolmentPage.IsAddressDetailsSaveButtonDisabled(),
                $"Expected the Address Details Save Button to be disabled when Mandatory field {mandatoryField} id null or empty");

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
               "Expected the Enrol Member Button to be in Disabled State but found Enabled");
        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Fact, RunOrder(7)]
        public void ValidationOfErrorMessageWhenPassedExistingEmailAddressForEnrolment()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("ValidationOfErrorMessageWhenPassedExistingEmailAddressForEnrolment");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string queryForCustomerSearchDocument = "SELECT TOP 1 * FROM  c where c.DocType='CustomerSearch' order by c.CreatedOn desc";

            CustomerSearch customerSearchDocument = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDocument);

            string emailAddress = customerSearchDocument.EmailAddress;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "442424");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, ReasonForTravel.Both.ToString());

            enrolmentPage.AcceptFCTermsAndConditions(logger);

            string[] text = enrolmentPage.EnrolTheMemberIntoFlyingClubProgram();

            string expectedMessage = $"An account with that email address already exists";

            Assert.True(text[1].Contains(expectedMessage),
                $"Expected {expectedMessage} in the response but found {text[1]}.");

            Assert.True(text[2].Contains(customerSearchDocument.LoyaltyMemberNumber),
                $"Expected {customerSearchDocument.LoyaltyMemberNumber} in the response but found {text[2]}.");

        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Fact, RunOrder(8)]
        public void ValidateEmailDetailsSaveButtonWhenEmailAddressFormatIsInvalid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("ValidateEmailDetailsSaveButtonWhenEmailAddressFormatIsInvalid");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "test.noreply.inc.com";

            string emailUseType = "Personal";

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, emailUseType, emailAddress);

            Assert.True(enrolmentPage.IsEmailDetailsSaveButtonDisabled(),
                $"Expected the Email Details Save Button to be disabled when Email Address {emailAddress} is in invalid format.");

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
               "Expected the Enrol Member Button to be in Disabled State but found Enabled");
        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Theory, RunOrder(9)]
        [InlineData("parentName")]
        [InlineData("parentEmailAddress")]
        [InlineData("parentPhone")]
        public void ValidateEnrolButtonWhenMandatoryParentalDetailsFieldValuePassedAsNullorEmptyForMinorEnrolment(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"ValidateEnrolButtonWhenMandatoryParentalDetailsField-{mandatoryField}-ValuePassedAsNullorEmptyForMinorEnrolment");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int range = Convert.ToInt32(DateTime.UtcNow.Subtract(enddate).TotalDays);

            string dob = enddate.AddDays(random.Next(range)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15).ToLower() + "@test.noreply.inc.com";

            string parentName = "TestParent";

            string parentEmailAddress = DataGenerator.RandomString(15).ToLower() + "@testparent.noreply.inc.com";

            string parentPhone = "9646464353";

            string reasonForTravel = ReasonForTravel.Leisure.ToString();

            switch (mandatoryField)
            {
                case "parentName":
                    parentName = "";
                    break;

                case "parentEmailAddress":
                    parentEmailAddress = "";
                    break;

                case "parentPhone":
                    parentPhone = "";
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterParentalConsentDetails(parentName, parentEmailAddress, "United Kingdom (+44)", parentPhone);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "442424");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, reasonForTravel);

            enrolmentPage.AcceptFCTermsAndConditions(logger);

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
                "Expected the Enrol Member Button to be in Disabled State but found Enabled");

        }

        [Trait("Category", Traits.EnrolmentUIRegression)]
        [Fact, RunOrder(10)]
        public void ValidateEnrolButtonWhenVAATermsAndConditionsAreNotAccepted()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("ValidateEnrolButtonWhenVAATermsAndConditionsAreNotAccepted");

            DateTime startdate = DateTime.Now.AddYears(-50);

            DateTime enddate = DateTime.Now.AddYears(-18);

            int majorDateRange = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(majorDateRange)).ToString("dd-MMM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@test.noreply.inc.com";

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            enrolmentPage.ClickOnEnrolButton(logger);

            enrolmentPage.EnterMemberDetails(logger, "Mr", "Male", "Test", "Member", dob);

            enrolmentPage.EnterMemberAddressDetails(logger, "Home", "Test Street", "RH11 7DU", "Crawley", "United Kingdom");

            enrolmentPage.SaveTheEnteredAddressDetails(logger);

            enrolmentPage.EnterMemberEmailDetails(logger, "Personal", emailAddress);

            enrolmentPage.SaveTheEnteredEmailDetails(logger);

            enrolmentPage.EnterMemberPhoneDetails(logger, "Mobile", "United Kingdom (+44)", "442424");

            enrolmentPage.SaveTheEnteredPhoneDetails(logger);

            enrolmentPage.EnterReasonForTravel(logger, ReasonForTravel.Both.ToString());

            Assert.True(enrolmentPage.IsEnrolButtonDisabled(),
                "Expected the Enrol Member Button to be in Disabled State but found Enabled");

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

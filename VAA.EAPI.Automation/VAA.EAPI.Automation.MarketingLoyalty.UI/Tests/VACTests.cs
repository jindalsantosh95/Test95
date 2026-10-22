using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
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

    [Collection("VACTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class VACTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private MemberProfilePage memberProfilePage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public VACTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
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
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);
            memberProfilePage = new MemberProfilePage(webDriver);

        }

        [Trait("Category", Traits.VACUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_VACRetrieval()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_VACRetrieval");

            (string memberNumber, string customerId) = GetValidVACMember();

            (string memberFirstName, string memberLastName, string memberPhoneNumber, string memberEmailAddress, string memberDoB) = GetVACMemberDetails(customerId);

            string memberPreference = GetVACMemberPreference(customerId);

            Assert.True(!(string.IsNullOrEmpty(memberNumber) || string.IsNullOrEmpty(customerId)),
                "No Valid member found for the test Execution.");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnVACMemberNumberToNavigateToVACMemberProfile(logger);

            string vacFirstName = webDriver.GetAttributeTextValue(MemberDetail.VacFirstName, "value");

            // Verify First Name
            Assert.True(!string.IsNullOrEmpty(vacFirstName), "VAC First Name is empty");

            Assert.True(memberFirstName.Equals(vacFirstName), $"First Name mismatch. Expected: {memberFirstName}, Actual: {vacFirstName}");

            string vacLastName = webDriver.GetAttributeTextValue(MemberDetail.VacLastName, "value");

            Assert.True(memberLastName.Equals(vacLastName), $"Last Name mismatch. Expected: {memberLastName}, Actual: {vacLastName}");

            string vacEmailAddress = webDriver.GetAttributeTextValue(MemberDetail.VacEmailAddress, "value");

            Assert.True(memberEmailAddress.Equals(vacEmailAddress), $"Email Address mismatch. Expected: {memberEmailAddress}, Actual: {vacEmailAddress}");

            string vacDateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.VacDateOfBirth, "value");

            string vacPhoneNumber = webDriver.GetAttributeTextValue(MemberDetail.VacPhoneNumber, "value");

            Assert.True(memberPhoneNumber.Equals(vacPhoneNumber), $"Phone Number mismatch. Expected: {memberPhoneNumber}, Actual: {vacPhoneNumber}");

            string dateOfBirth = webDriver.GetAttributeTextValue(MemberDetail.VacDateOfBirth, "value");

            Assert.True(dateOfBirth.Equals(vacDateOfBirth), $"DoB  mismatch. Expected: {dateOfBirth}, Actual: {vacDateOfBirth}");

            string memberPreferenceFlag = webDriver.FindElement(By.XPath(MemberDetail.VacMemberPreference)).GetAttribute("checked");

            memberPreferenceFlag = string.IsNullOrEmpty(memberPreferenceFlag) ? "Y" : memberPreferenceFlag.ToLower().Equals("true") ? "N" : "Y";

            Assert.True(memberPreference.Equals(memberPreferenceFlag), $"Member Preference expected on the screen is {memberPreference} , Actual : {memberPreferenceFlag}");



        }

        public (string MemberNumber, string CustomerId) GetValidVACMember()
        {
            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode = @tierCode and c.MemberType = @memberType and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "VAC").WithParameter("@memberType", "Individual").WithParameter("@status", "Active");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            return (fcmDoc.MemberNumber, fcmDoc.CustomerId);
        }

        public (string memberFirstName, string memberLastName, string memberPhoneNumber, string memberEmailAddress, string memberDoB) GetVACMemberDetails(string customerId)
        {
            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            string phoneNumber = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone").ContactValue;

            string emailAddress = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email").ContactValue;

            string dateOfBirth = customerDoc.DateOfBirth?.ToString("dd-MMM-yyyy");

            return (customerDoc.FirstName, customerDoc.LastName, phoneNumber, emailAddress, dateOfBirth);


        }

        public string GetVACMemberPreference(string customerId)
        {
            string queryForCustomerProfileDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerProfileDoc).WithParameter("@docType", "CustomerProfile").WithParameter("@customerId", customerId);

            CustomerProfile customerProfileDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.CustomerProfile>(parameterizedQuery);

            string memberPreference = customerProfileDoc.Preferences.FirstOrDefault(x => x.PreferenceType == "VAAMarketingPermissions").PreferenceValue;

            return memberPreference;
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

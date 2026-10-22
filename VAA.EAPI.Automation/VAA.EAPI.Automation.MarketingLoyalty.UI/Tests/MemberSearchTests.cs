using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("MemberSearchTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberSearchTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private readonly TokenFixture _tokenFixture;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public MemberSearchTests(ReportFixture reportFixture, CosmosFixture cosmosFixture, TokenFixture _tokenFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this._tokenFixture = _tokenFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this._tokenFixture.AddTokenForVRedFromKeyVault("vred");
            memberSearchPage = new MemberSearchPage(webDriver);

        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_MemberSearchUsingFCMemberNumber()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_MemberSearchUsingFCMemberNumber");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingFCNumber(fcMemberNumber);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() == 1,
                $"Expected only one Search Result for FC Number Search {fcMemberNumber} but the count is {result.Count()}");

            string memberNumber = result.FirstOrDefault().FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            Assert.True(fcMemberNumber.Equals(memberNumber),
                $"Expected the Member Number in the Search Result to be {fcMemberNumber} but found {memberNumber}.");


        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_MemberSearchUsingEmailAddress()
        {
            // Create a new test in the report
            ExtentTest logger = reportFixture.Current.CreateTest("TC02_MemberSearchUsingEmailAddress");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string emailAddress = customerDoc.ContactDetails.FirstOrDefault(c => c.ContactType == "Email").ContactValue;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingEmailAddress(emailAddress);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() == 1,
                $"Expected only one Search Result for Email Address Search {emailAddress} but the count is {result.Count()}");

            string emailId = result.FirstOrDefault().FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            Assert.True(emailAddress.Equals(emailId),
                $"Expected the Email Address in the Search Result to be {emailAddress} but found {emailId}.");


        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_MemberSearchUsingFirstNameAndSurName()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC03_MemberSearchUsingFirstNameAndSurName");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string firstName = customerDoc.FirstName;

            string surName = customerDoc.LastName;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingFirstNameAndSurName(firstName, surName);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() > 0,
                $"Expected one or more Search Result for firstname {firstName} and lastname {surName} Search but the count was {result.Count()}");

            foreach (IWebElement element in result)
            {
                string memberFullName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                Assert.True(memberFullName.Equals(firstName + ", " + surName),
                    $"Expected the Member Full Name in the Search Result to be {firstName + ", " + surName} but found {memberFullName}.");
            }

        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_MemberSearchUsingFirstNameAndDateOfBirth()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC04_MemberSearchUsingFirstNameAndDateOfBirth");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string dateOfBirth = customerDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string firstName = customerDoc.FirstName;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingFirstNameAndDateOfBirth(firstName, dateOfBirth);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() > 0,
                $"Expected one or more Search Result for FirstName {firstName} and DateOfBirth {dateOfBirth} Search but the count was {result.Count()}");

            foreach (IWebElement element in result)
            {
                string dob = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string fullName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                Assert.True(dateOfBirth.Equals(dob),
                    $"Expected the Date Of Birth in the Search Result to be {dateOfBirth} but found {dob}.");

                Assert.True(fullName.Has(firstName),
                    $"Expected the FirstName {firstName} in the FullName field of Search Result  but found {fullName}.");
            }

        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_MemberSearchUsingSurNameAndDateOfBirth()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC05_MemberSearchUsingSurNameAndDateOfBirth");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string dateOfBirth = customerDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string surName = customerDoc.LastName;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingSurNameAndDateOfBirth(surName, dateOfBirth);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() > 0, $"Expected one or more Search Result for SurName {surName} and DateOfBirth {dateOfBirth} Search but the count was {result.Count()}");

            foreach (IWebElement element in result)
            {
                string dob = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string fullName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                Assert.True(dateOfBirth.Equals(dob),
                    $"Expected the Date Of Birth in the Search Result to be {dateOfBirth} but found {dob}.");

                Assert.True(fullName.Has(surName),
                    $"Expected the LastName {surName} in the FullName field of Search Result but found {fullName}.");
            }

        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_MemberSearchUsingSurNameAndPostCode()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC06_MemberSearchUsingSurNameAndPostCode");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string postCode = customerDoc.AddressDetails.FirstOrDefault().PostCode;

            string surName = customerDoc.LastName;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingSurNameAndPostCode(surName, postCode);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() > 0,
                $"Expected one or more Search Result for SurName {surName} and postcode {postCode} Search but the count was {result.Count()}");

            foreach (IWebElement element in result)
            {
                string fullName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                Assert.True(fullName.Has(surName),
                    $"Expected the LastName {surName} in the FullName field of Search Result but found {fullName}.");
            }

        }


        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(7)]
        public void TC07_MemberSearchUsingSurNameAndDateOfBirthAndTier()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC07_MemberSearchUsingSurNameAndDateOfBirthAndTier");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            string dateOfBirth = customerDoc.DateOfBirth.Value.ToString("dd-MMM-yyyy");

            string surName = customerDoc.LastName;

            string tierCode = fcmDoc.TierCode;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.MemberSearchUsingSurNameAndDateOfBirthAndTier(surName, dateOfBirth, tierCode);

            memberSearchPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MemberSearchResultTableDetails.MemberSearchResultSet));

            Assert.True(result.Count() > 0,
                $"Expected one or more Search Result for DateOfBirth {dateOfBirth} ,SurName {surName} ,TierCode {tierCode} Search but the count was {result.Count()}");

            foreach (IWebElement element in result)
            {
                string dob = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string fullName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string tier = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                Assert.True(dateOfBirth.Equals(dob),
                    $"Expected the Date Of Birth in the Search Result to be {dateOfBirth} but found {dob}.");

                Assert.True(fullName.Has(surName),
                    $"Expected the LastName {surName} in the FullName field of Search Result but found {fullName}.");

                Assert.True(tierCode.Equals(tier),
                    $"Expected the Tiercode in the Search Result to be {tierCode} but found {tier}.");
            }
        }

        [Trait("Category", Traits.MemberSearchUIRegression)]
        [Fact, RunOrder(8)]
        public void TC08_MemberSearchAndVredFlagAttributeValidation()
        {
            #region Test Data 

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_MemberSearchAndVredFlagAttributeValidation");

            string fcMemberNumber = GetValidFCNumberWithVREDFlagAttribute();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string vredFlagValue;

            try
            {
                webDriver.WaitForElementToBePresent(MemberDetail.VredMemberFlag, 10);

                vredFlagValue = webDriver.FindElement(By.XPath(MemberDetail.VredMemberFlag)).GetAttribute("checked");
            }

            catch (Exception ex)
            {
                vredFlagValue = ex.Message;
            }

            Assert.True(vredFlagValue.Equals("true"), $"Vred Flag Attribute is missing on the PowerApps screen for member number :{fcMemberNumber} and the error message is: {vredFlagValue} ");

        }


        public string GetValidFCNumberWithVREDFlagAttribute()
        {
            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            string fcMemberNumber = fcmDoc.MemberNumber;

            HttpResponseMessage vredResponse = LoopsFrontEndAPIEndpoints.VRedMemberStatusAPI(_tokenFixture.GetToken("vred"), fcMemberNumber).EnsureIsSuccessStatusCode();

            dynamic? vredResult = JsonConvert.DeserializeObject<dynamic>(vredResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isRedUserIdExists = !string.IsNullOrEmpty(vredResult.redUserId.ToString());

            Assert.True(isRedUserIdExists, $"The member {fcMemberNumber} selected is not a linked member at VRED end");

            HttpResponseMessage responseFeAPI = LoopsFrontEndAPIEndpoints.GetMember(fcMemberNumber).EnsureIsSuccessStatusCode();

            dynamic? resultFeAPI = JsonConvert.DeserializeObject<dynamic>(responseFeAPI.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            GetMember getMember = JsonConvert.DeserializeObject<GetMember>(resultFeAPI.ToString());

            bool vredMemberFlag = getMember.vredMember;

            if (isRedUserIdExists)
            {
                Assert.True(vredMemberFlag, $"Expected VRED Member Flag atttribute for member id: {fcMemberNumber} is true but retrieved in Get member response as {vredMemberFlag}");
            }

            return fcMemberNumber;
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

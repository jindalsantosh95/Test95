using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Customer = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests.AuditHistory
{
    [Collection("AuditManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class EmailAndPhoneAuditHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly MemberSearchPage memberSearchPage;
        private readonly AuditHistoryPage auditHistoryPage;
        private readonly IWebDriver webDriver;
        private readonly MemberProfilePage memberProfilePage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;


        public EmailAndPhoneAuditHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<ContactDetails>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<ContactHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);

        }


        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(5)]
        public void TC01_RetrieveEmailAuditHistoryForMemberSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveEmailAuditHistoryForMemberSuccessScenario");

            #region test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.ContactType = @contactType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "ContactHistory").WithParameter("@contactType", "Email");

            ContactHistory contactHistoryDoc = cosmosFixture.CosmosQueryItem<ContactHistory>(parameterizedQuery);

            Assert.True(contactHistoryDoc is not null, "Failed to retrieve the Test Data");

            string customerId = contactHistoryDoc.CustomerId;

            string queryForEmailHistoryDocs = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ContactType = @contactType ";

            QueryDefinition query = new QueryDefinition(query: queryForEmailHistoryDocs).WithParameter("@docType", "ContactHistory").WithParameter("@partitionKey", customerId).WithParameter("@contactType", "Email");

            //Email History Documents 
            ContactHistory[] emailHistoryDocs = cosmosFixture.CosmosQueryItems<ContactHistory>(query).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve the Customer Doc for customer {customerId}");

            ContactDetails currentEmailDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y");

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            if (currentEmailDetails is not null)
            {

                emailHistoryDocs = emailHistoryDocs.Append(new ContactHistory
                {
                    CreatedBy = currentEmailDetails.CreatedBy,
                    CreatedOn = currentEmailDetails.CreatedOn.Value.DateTime,
                    LastUpdatedBy = currentEmailDetails.LastUpdatedBy,
                    LastUpdatedOn = currentEmailDetails.LastUpdatedOn.Value.DateTime,
                    PartitionKey = customerId,
                    CustomerId = customerId,
                    ContactId = currentEmailDetails.ContactId,
                    ContactType = currentEmailDetails.ContactType,
                    ContactSubType = currentEmailDetails.ContactSubType,
                    ContactValue = currentEmailDetails.ContactValue,
                    CountryCode = currentEmailDetails.CountryCode,
                    Valid = currentEmailDetails.Valid,
                    Primary = currentEmailDetails.Primary,
                    Usage = currentEmailDetails.Usage,
                }).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            }

            #endregion

            #region - PowerApps Validations for the Audit History Email details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryEmailTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultEmailAuditDataSet));

            Assert.True(result.Any(),
                     $"Email Audit History not Found for the Flying Club Member Number {fcMemberNumber}");

            Assert.True(result.Count() == emailHistoryDocs.Count(),
                    $"The No Of Rows on PA Email history page is not equal to no of documents for the Flying Club Member Number {fcMemberNumber} .Expected {emailHistoryDocs.Count()} rows but found {result.Count()} ");

            int i = 0;

            foreach (IWebElement element in result)
            {
                string actualEmailAddress = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string actualEmailValidFlag = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string actualEmailUseType = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedBy = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedOn = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                Assert.True(actualEmailAddress == emailHistoryDocs[i].ContactValue,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the EmailAddress to be {emailHistoryDocs[i].ContactValue} but found {actualEmailAddress}");

                Assert.True(actualEmailValidFlag == emailHistoryDocs[i].Valid,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Valid Flag to be {emailHistoryDocs[i].Valid} but found {actualEmailValidFlag}");

                Assert.True(actualEmailUseType == emailHistoryDocs[i].Usage,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Usage/UseType to be {emailHistoryDocs[i].Usage} but found {actualEmailUseType}");

                Assert.True(actualLastUpdatedBy == emailHistoryDocs[i].LastUpdatedBy,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the LastUpdatedBy to be {emailHistoryDocs[i].LastUpdatedBy} but found {actualLastUpdatedBy}");

                Assert.True(actualLastUpdatedOn == emailHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss"),
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Last Updated On to be {emailHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss")} but found {actualLastUpdatedOn}");

                i++;

            }

            #endregion
        }

        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(6)]
        public void TC02_RetrievePhoneAuditHistoryForMemberSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_RetrievePhoneAuditHistoryForMemberSuccessScenario");

            #region test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.ContactType = @contactType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "ContactHistory").WithParameter("@contactType", "Phone");

            ContactHistory contactHistoryDoc = cosmosFixture.CosmosQueryItem<ContactHistory>(parameterizedQuery);

            Assert.True(contactHistoryDoc is not null, "Failed to retrieve the Test Data");

            string customerId = contactHistoryDoc.CustomerId;

            string queryForPhoneHistoryDocs = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ContactType = @contactType ";

            QueryDefinition query = new QueryDefinition(query: queryForPhoneHistoryDocs).WithParameter("@docType", "ContactHistory").WithParameter("@partitionKey", customerId).WithParameter("@contactType", "Phone");

            //Email History Documents 
            ContactHistory[] phoneHistoryDocs = cosmosFixture.CosmosQueryItems<ContactHistory>(query).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve the Customer Doc for customer {customerId}");

            ContactDetails currentPhoneDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y");

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            if (currentPhoneDetails is not null)
            {
                phoneHistoryDocs = phoneHistoryDocs.Append(new ContactHistory
                {
                    CreatedBy = currentPhoneDetails.CreatedBy,
                    CreatedOn = currentPhoneDetails.CreatedOn.Value.DateTime,
                    LastUpdatedBy = currentPhoneDetails.LastUpdatedBy,
                    LastUpdatedOn = currentPhoneDetails.LastUpdatedOn.Value.DateTime,
                    PartitionKey = customerId,
                    CustomerId = customerId,
                    ContactId = currentPhoneDetails.ContactId,
                    ContactType = currentPhoneDetails.ContactType,
                    ContactSubType = currentPhoneDetails.ContactSubType,
                    ContactValue = currentPhoneDetails.ContactValue,
                    CountryCode = currentPhoneDetails.CountryCode,
                    Valid = currentPhoneDetails.Valid,
                    Primary = currentPhoneDetails.Primary,
                    Usage = currentPhoneDetails.Usage,
                }).OrderByDescending(x => x.LastUpdatedOn).ToArray();
            }
            #endregion

            #region - PowerApps Validations for the Audit History Email details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryPhoneTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultPhoneAuditDataSet));

            Assert.True(result.Any(),
                     $"Phone Audit History not Found for the Flying Club Member Number {fcMemberNumber}");

            Assert.True(result.Count() == phoneHistoryDocs.Count(),
                    $"The No Of Rows on PA Phone history page is not equal to no of documents for the Flying Club Member Number {fcMemberNumber} .Expected {phoneHistoryDocs.Count()} rows but found {result.Count()}");

            int i = 0;

            foreach (IWebElement element in result)
            {
                string actualPhoneCountryCode = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string actualPhoneNumber = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string actualPhoneValidFlag = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string actualPhoneUseType = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string actualPhonePrimaryFlag = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedBy = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedOn = element.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

                Assert.True(actualPhoneCountryCode == phoneHistoryDocs[i].CountryCode,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Phone CountryCode to be {phoneHistoryDocs[i].CountryCode} but found {actualPhoneCountryCode}");

                Assert.True(actualPhoneNumber == phoneHistoryDocs[i].ContactValue,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Phone Number to be {phoneHistoryDocs[i].ContactValue} but found {actualPhoneNumber}");

                Assert.True(actualPhoneValidFlag == phoneHistoryDocs[i].Valid,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Valid Flag to be {phoneHistoryDocs[i].Valid} but found {actualPhoneValidFlag}");

                Assert.True(actualPhoneUseType == phoneHistoryDocs[i].ContactSubType,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Usage/UseType to be {phoneHistoryDocs[i].ContactSubType} but found {actualPhoneUseType}");

                Assert.True(actualPhonePrimaryFlag == phoneHistoryDocs[i].Primary,
                  $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Primary Flag to be {phoneHistoryDocs[i].Primary} but found {actualPhonePrimaryFlag}");

                Assert.True(actualLastUpdatedBy == phoneHistoryDocs[i].LastUpdatedBy,
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the LastUpdatedBy to be {phoneHistoryDocs[i].LastUpdatedBy} but found {actualLastUpdatedBy}");

                Assert.True(actualLastUpdatedOn == phoneHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss"),
                   $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Last Updated On to be {phoneHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss")} but found {actualLastUpdatedOn}");

                i++;

            }

            #endregion
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

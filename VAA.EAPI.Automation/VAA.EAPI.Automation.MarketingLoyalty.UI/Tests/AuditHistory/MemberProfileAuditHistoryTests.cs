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
    public class MemberProfileAuditHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly MemberSearchPage memberSearchPage;
        private readonly AuditHistoryPage auditHistoryPage;
        private readonly IWebDriver webDriver;
        private readonly MemberProfilePage memberProfilePage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public string env = Config.Environment;

        public MemberProfileAuditHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerProfileAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);

        }

        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(7)]
        public void TC01_RetrieveMemberProfileAuditDetailsSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveMemberProfileAuditDetailsSuccessScenario");

            #region Test Data           

            string queryForCustomerProfileAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.CreatedBy <> 'WEB' order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForCustomerProfileAuditDoc).WithParameter("@docType", "CustomerProfileAudit");

            CustomerProfileAudit customerProfAuditDoc = cosmosFixture.CosmosQueryItem<CustomerProfileAudit>(query);

            Assert.True(customerProfAuditDoc is not null, "No Customer Profile Audit Document Found.");

            string customerId = customerProfAuditDoc.CustomerId;

            string queryForCustProfAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustProfAuditDocs).WithParameter("@docType", "CustomerProfileAudit").WithParameter("@customerId", customerId);

            CustomerProfileAudit[] custProfAuditCosmosDocs = cosmosFixture.CosmosQueryItems<CustomerProfileAudit>(paramQuery).OrderByDescending(x => x.CreatedOn).ToArray();

            Assert.True(custProfAuditCosmosDocs.Any(), $" No Customer Profile Audit Docs found for customerId {customerId}");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion

            #region - PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryMemberProfileTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultMemberProfileAuditDataSet));

            Assert.True(result.Any(),
                     $"No Audit History Found for the Flying Club Member Number {fcMemberNumber}");

            int i = 0;

            foreach (IWebElement element in result)
            {
                string actualMemberTitle = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string actualMemberFirstName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string actualMemberMiddleName = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string actualMemberLastName = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string actualMemberDateOfBirth = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string actualMemberStatus = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                string actualMemberNameChangeReason = element.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

                string actualMemberAuthUser = element.FindElement(By.XPath("td[8]/div/div/span")).GetAttribute("value");

                string actualDowngradeProhibitFlag = element.FindElement(By.XPath("td[9]/div/div/span")).GetAttribute("value");

                string actualMemberLastUpdatedBy = element.FindElement(By.XPath("td[10]/div/div/span")).GetAttribute("value");

                string actualMemberLastUpdatedOn = element.FindElement(By.XPath("td[11]/div/div/span")).GetAttribute("value");

                Assert.True(actualMemberTitle == custProfAuditCosmosDocs[i].Title,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected Title to be {custProfAuditCosmosDocs[i].FirstName} but was {actualMemberTitle}");

                Assert.True(actualMemberFirstName == custProfAuditCosmosDocs[i].FirstName,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected FirstName to be {custProfAuditCosmosDocs[i].FirstName} but was {actualMemberFirstName}");

                Assert.True(actualMemberMiddleName == custProfAuditCosmosDocs[i].MiddleName,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected MiddleName to be {custProfAuditCosmosDocs[i].MiddleName} but was {actualMemberMiddleName}");

                Assert.True(actualMemberLastName == custProfAuditCosmosDocs[i].LastName,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected LastName to be {custProfAuditCosmosDocs[i].LastName} but was {actualMemberLastName}");

                string DoB = Convert.ToDateTime(custProfAuditCosmosDocs[i].DoB).ToString("dd-MMM-yyyy");

                Assert.True(actualMemberDateOfBirth == DoB.ToString(),
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected DateOfBirth to be {custProfAuditCosmosDocs[i].DoB} but was {actualMemberDateOfBirth}");

                Assert.True(actualMemberStatus == custProfAuditCosmosDocs[i].Status,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected Account Status to be {custProfAuditCosmosDocs[i].Status} but was {actualMemberStatus}");

                Assert.True(actualMemberNameChangeReason == custProfAuditCosmosDocs[i].NameChangeReason,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected Name Change Reason to be {custProfAuditCosmosDocs[i].NameChangeReason} but was {actualMemberNameChangeReason}");

                Assert.True(actualMemberAuthUser == custProfAuditCosmosDocs[i].AuthorisedUser,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected Auth User Name to be {custProfAuditCosmosDocs[i].AuthorisedUser} but was {actualMemberAuthUser}");

                Assert.True(actualDowngradeProhibitFlag == custProfAuditCosmosDocs[i].DowngradeProhibitFlag,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected Downgrade Prohibit flag value  to be {custProfAuditCosmosDocs[i].DowngradeProhibitFlag} but was {actualDowngradeProhibitFlag}");

                Assert.True(actualMemberLastUpdatedBy == custProfAuditCosmosDocs[i].CreatedBy,
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected the Last Updated By to be {custProfAuditCosmosDocs[i].CreatedBy} but was {actualMemberLastUpdatedBy}");

                Assert.True(actualMemberLastUpdatedOn == custProfAuditCosmosDocs[i].CustomerProfileUpdatedOn.ToString("dd-MMM-yyyy HH:mm:ss"),
                    $"Under Member Profile history >> for the FC member {fcMemberNumber}, expected the Last Updated On to be {custProfAuditCosmosDocs[i].CustomerProfileUpdatedOn.ToString("dd-MMM-yyyy HH:mm:ss")} but was {actualMemberLastUpdatedOn}");

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



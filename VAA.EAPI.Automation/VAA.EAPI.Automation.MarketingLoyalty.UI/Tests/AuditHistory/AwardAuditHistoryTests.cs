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
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests.AuditHistory
{
    [Collection("AuditManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class AwardAuditHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
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

        public AwardAuditHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string cosmosEndPointForCustTxnDB = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndPointForCustTxnDB, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndPointForCustTxnDB, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);

        }



        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(2)]
        public void TC01_RetrieveLTGAwardAuditDetailsSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveLTGAwardAuditDetailsSuccessScenario");

            #region Pre-requsite of upgrading a member to an LTG Award 

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM c where c.DocType=@docType and c.AwardProductId='1634561772' order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found.");

            string customerId = awardAuditDoc.CustomerId;

            //fetching the Loyalty member Number

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion


            #region fetching data from the Audit Cosmos DB

            string queryForCustLTGAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.AwardProductId='1634561772' and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustLTGAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId);

            IEnumerable<AwardAudit> custLTGAwardAuditDocs = cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery);

            #endregion

            #region - PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryAwardTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultAwardAuditDataSet));

            Assert.True(result.Any(),
                     $"No Award Audit History Found for the Flying Club Member Number {fcMemberNumber}");

            foreach (IWebElement element in result)
            {
                string actualFlagName = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                if (actualFlagName == "Lifetime Gold")
                {
                    string actualStartDate = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                    string actualEndDate = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                    string actualUpdatedBy = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                    string actualUpdatedOn = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                    string expectedAwardName = custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardName;

                    string? expectedStartDate = custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.HasValue ? custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.Value.ToString("dd-MMM-yyyy") : null;

                    string? expectedEndDate = custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.HasValue ? custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.Value.ToString("dd-MMM-yyyy") : null;

                    string expectedUpdatedBy = custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedBy;

                    string? expectedUpdatedOn = custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.HasValue ? custLTGAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss") : null;

                    Assert.True(actualFlagName == expectedAwardName,
                                            $"Under Award history >> for the FC member {fcMemberNumber}, expected Award Number to be {expectedAwardName} but was {actualFlagName}");

                    Assert.True(actualStartDate == expectedStartDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award StartDate to be {expectedStartDate} but was {actualStartDate}");

                    Assert.True(actualEndDate == expectedEndDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award EndDate to be {expectedEndDate} but was {actualEndDate}");


                    Assert.True(actualUpdatedBy == expectedUpdatedBy,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedBy to be {expectedUpdatedBy} but was {actualUpdatedBy}");

                    Assert.True(actualUpdatedOn == expectedUpdatedOn,
                                     $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedOn to be {expectedUpdatedOn} but was {actualUpdatedOn}");

                    break;

                }

            }
            #endregion
        }

        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(3)]
        public void TC02_RetrieveMillionMilerAwardAuditDetailsSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_RetrieveMillionMillerAwardAuditDetailsSuccessScenario");

            #region Pre-requsite of upgrading a member to an MM Award 

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM c where c.DocType=@docType and c.AwardProductId='1634561592' order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found.");

            string customerId = awardAuditDoc.CustomerId;

            //fetching the Loyalty member Number

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion


            #region fetching data from the Audit Cosmos DB

            string queryForCustMMAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.AwardProductId='1634561592' and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustMMAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId);

            IEnumerable<AwardAudit> custMMAwardAuditDocs = cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery);

            #endregion

            #region - PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryAwardTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultAwardAuditDataSet));

            Assert.True(result.Any(),
                     $"No Award Audit History Found for the Flying Club Member Number {fcMemberNumber}");

            foreach (IWebElement element in result)
            {
                string actualFlagName = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                if (actualFlagName == "Million Miler")
                {
                    string actualStartDate = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                    string actualEndDate = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                    string actualUpdatedBy = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                    string actualUpdatedOn = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                    string expectedAwardName = custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardName;

                    string? expectedStartDate = custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.HasValue ? custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.Value.ToString("dd-MMM-yyyy") : null;

                    string? expectedEndDate = custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.HasValue ? custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.Value.ToString("dd-MMM-yyyy") : null;

                    string expectedUpdatedBy = custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedBy;

                    string? expectedUpdatedOn = custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.HasValue ? custMMAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss") : null;

                    Assert.True(actualFlagName == expectedAwardName,
                                            $"Under Award history >> for the FC member {fcMemberNumber}, expected Award Number to be {expectedAwardName} but was {actualFlagName}");

                    Assert.True(actualStartDate == expectedStartDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award StartDate to be {expectedStartDate} but was {actualStartDate}");

                    Assert.True(actualEndDate == expectedEndDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award EndDate to be {expectedEndDate} but was {actualEndDate}");


                    Assert.True(actualUpdatedBy == expectedUpdatedBy,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedBy to be {expectedUpdatedBy} but was {actualUpdatedBy}");

                    Assert.True(actualUpdatedOn == expectedUpdatedOn,
                                     $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedOn to be {expectedUpdatedOn} but was {actualUpdatedOn}");

                    break;

                }

            }
            #endregion
        }



        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(4)]
        public void TC03_RetrieveTopsMemberAwardAuditDetailsSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC03_RetrieveTopsMemberAwardAuditDetailsSuccessScenario");

            #region Pre-requsite of upgrading a member to an TopsMember Award 

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM c where c.DocType=@docType and c.AwardProductId='1634561747' order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found.");

            string customerId = awardAuditDoc.CustomerId;

            //fetching the Loyalty member Number

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion


            #region fetching data from the Audit Cosmos DB

            string queryForCustTopsMemberAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.AwardProductId='1634561747' and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustTopsMemberAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId);

            IEnumerable<AwardAudit> custTopsMemberAwardAuditDocs = cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery);

            #endregion

            #region - PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryAwardTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultAwardAuditDataSet));

            Assert.True(result.Any(),
                     $"No Award Audit History Found for the Flying Club Member Number {fcMemberNumber}");

            foreach (IWebElement element in result)
            {
                string actualFlagName = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                if (actualFlagName == "Top Member")
                {
                    string actualStartDate = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                    string actualEndDate = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                    string actualUpdatedBy = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                    string actualUpdatedOn = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                    string expectedAwardName = custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardName;

                    string? expectedStartDate = custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.HasValue ? custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().StartDate.Value.ToString("dd-MMM-yyyy") : null;

                    string? expectedEndDate = custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.HasValue ? custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().EndDate.Value.ToString("dd-MMM-yyyy") : null;

                    string expectedUpdatedBy = custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedBy;

                    string? expectedUpdatedOn = custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.HasValue ? custTopsMemberAwardAuditDocs.OrderByDescending(x => x.CreatedOn).FirstOrDefault().AwardUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss") : null;

                    Assert.True(actualFlagName == expectedAwardName,
                                            $"Under Award history >> for the FC member {fcMemberNumber}, expected Award Number to be {expectedAwardName} but was {actualFlagName}");

                    Assert.True(actualStartDate == expectedStartDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award StartDate to be {expectedStartDate} but was {actualStartDate}");

                    Assert.True(actualEndDate == expectedEndDate,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award EndDate to be {expectedEndDate} but was {actualEndDate}");


                    Assert.True(actualUpdatedBy == expectedUpdatedBy,
                                    $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedBy to be {expectedUpdatedBy} but was {actualUpdatedBy}");

                    Assert.True(actualUpdatedOn == expectedUpdatedOn,
                                     $"Under Award history >> for the FC member {fcMemberNumber}, expected Award LastUpdatedOn to be {expectedUpdatedOn} but was {actualUpdatedOn}");

                    break;

                }

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

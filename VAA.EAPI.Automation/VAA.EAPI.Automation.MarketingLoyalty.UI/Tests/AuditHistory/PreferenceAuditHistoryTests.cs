using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
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
    public class PreferenceAuditHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
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

        public PreferenceAuditHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<PreferenceAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);

        }

        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(8)]
        public void TC01_RetrievePreferencesAuditHistoryForMemberSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrievePreferencesAuditHistoryForMemberSuccessScenario");

            #region test Data

            string queryForTestData = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "PreferenceAudit");

            PreferenceAudit preferenceAuditDoc = cosmosFixture.CosmosQueryItem<PreferenceAudit>(parameterizedQuery);

            Assert.True(preferenceAuditDoc is not null, "Failed to retrieve the Test Data");

            string customerId = preferenceAuditDoc.CustomerId;

            string queryForPreferenceAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey  ";

            QueryDefinition query = new QueryDefinition(query: queryForPreferenceAuditDocs).WithParameter("@docType", "PreferenceAudit").WithParameter("@partitionKey", customerId);

            PreferenceAudit[] preferenceAuditDocs = cosmosFixture.CosmosQueryItems<PreferenceAudit>(query).OrderByDescending(x => x.PreferenceUpdatedOn).ToArray();

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve the Customer Doc for customer {customerId}");

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion

            #region - PowerApps Validations for the Audit History Preference details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            auditHistoryPage.ClickOnAuditHistoryTab(logger);

            auditHistoryPage.ClickOnAuditHistoryPreferencesTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultPreferencesAuditDataSet));

            string text = result.First().FindElement(By.XPath("td[1]")).Text;

            if (text == "No preference audit exists for this member profile")
            {

                string expectedMessage = "No preference audit exists for this member profile";

                string actualMessage = auditHistoryPage.GetPopUpMessageContent();

                Assert.True(expectedMessage.Equals(actualMessage),
                    $"When No Pref Audit is found , expected the Pop Up Message to be {expectedMessage} but found {actualMessage}");
            }

            else
            {

                Assert.True(result.Count() == preferenceAuditDocs.Count(),
                        $"The No Of Rows on PA Preference history page is not equal to no of documents for the Flying Club Member Number {fcMemberNumber}." +
                        $"Expected {preferenceAuditDocs.Count()} rows but found {result.Count()} ");

                int i = 0;

                foreach (IWebElement element in result)
                {
                    string actualVaMarketingPermissionValue = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                    string actualDoNotCallValue = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                    string actualDoNotSMSValue = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                    string actualDoNotEmailValue = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                    string actualUpdatedBy = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                    string actualUpdatedOn = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                    Assert.True(actualVaMarketingPermissionValue == preferenceAuditDocs[i].VAMarketingPreference,
                       $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the VA Marketing Permission Value to be {preferenceAuditDocs[i].VAMarketingPreference} but found {actualVaMarketingPermissionValue}");

                    Assert.True(actualDoNotCallValue == preferenceAuditDocs[i].DoNotCall,
                       $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Do Not Call Value to be {preferenceAuditDocs[i].DoNotCall} but found {actualDoNotCallValue}");

                    Assert.True(actualDoNotSMSValue == preferenceAuditDocs[i].DoNotSMS,
                      $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Do Not SMS Value to be {preferenceAuditDocs[i].DoNotSMS} but found {actualDoNotSMSValue}");

                    Assert.True(actualDoNotEmailValue == preferenceAuditDocs[i].DoNotEmail,
                      $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Do Not Email Value to be {preferenceAuditDocs[i].DoNotEmail} but found {actualDoNotEmailValue}");

                    Assert.True(actualUpdatedBy == preferenceAuditDocs[i].PreferenceUpdatedBy,
                       $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the UpdatedBy to be {preferenceAuditDocs[i].PreferenceUpdatedBy} but found {actualUpdatedBy}");

                    Assert.True(actualUpdatedOn == preferenceAuditDocs[i].PreferenceUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss"),
                       $" For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Updated On to be {preferenceAuditDocs[i].PreferenceUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss")} but found {actualUpdatedOn}");

                    i++;

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

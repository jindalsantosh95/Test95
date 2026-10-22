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
    public class AddressAuditHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly ReportFixture reportFixture;
        private readonly MemberSearchPage memberSearchPage;
        private readonly AuditHistoryPage auditHistoryPage;
        private readonly IWebDriver webDriver;
        private readonly MemberProfilePage memberProfilePage;

        private readonly string customerCosmosDb;

        public string env = Config.Environment;

        public AddressAuditHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            this.reportFixture = reportFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Address>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<AddressHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);
        }


        [Trait("Category", Traits.AuditManagementUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_RetrieveAddressAuditHistoryForFCMember_SuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveAddressAuditHistoryForFCMember_SuccessScenario");

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "AddressHistory");

            AddressHistory addressHistoryDoc = cosmosFixture.CosmosQueryItem<AddressHistory>(parameterizedQuery);

            Assert.True(addressHistoryDoc is not null, "Failed to retrieve the Test Data");

            string customerId = addressHistoryDoc.CustomerId;

            string queryForAddressHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForAddressHistoryDoc).WithParameter("@docType", "AddressHistory").WithParameter("@partitionKey", customerId);

            //Address History Documents 
            AddressHistory[] addressHistoryDocs = cosmosFixture.CosmosQueryItems<AddressHistory>(query).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve the Customer Doc for the customer {customerId}");

            string fcMemberNumber = customerDoc.LoyaltyMemberNumber;

            //Current Member Address
            Core.DB_Model.Entities.Address currentAddress = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            if (currentAddress is not null)
            {
                //appending Current to the List of History Docs
                addressHistoryDocs = addressHistoryDocs.Append(new AddressHistory
                {
                    PartitionKey = customerId,
                    CustomerId = customerId,
                    CreatedBy = currentAddress.CreatedBy,
                    CreatedOn = currentAddress.CreatedOn.Value.DateTime,
                    LastUpdatedBy = currentAddress.LastUpdatedBy,
                    LastUpdatedOn = currentAddress.LastUpdatedOn.Value.DateTime,
                    AddressId = currentAddress.AddressId,
                    AddressLine1 = currentAddress.AddressLine1,
                    AddressLine2 = currentAddress.AddressLine2,
                    AddressLine3 = currentAddress.AddressLine3,
                    AddressType = currentAddress.AddressType,
                    LandlordOrCompanyName = currentAddress.LandlordOrCompanyName,
                    Province = currentAddress.Province,
                    Valid = currentAddress.Valid,
                    Primary = currentAddress.Primary,
                    City = currentAddress.City,
                    Country = currentAddress.Country,
                    PostCode = currentAddress.PostCode

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

            auditHistoryPage.ClickOnAuditHistoryAddressTab();

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultAddressAuditDataSet));

            Assert.True(result.Any(),
                     $"Address Audit History not Found for the Flying Club Member Number {fcMemberNumber}");

            Assert.True(result.Count() == addressHistoryDocs.Count(),
                     $"The No Of Rows on PA address history page is not equal to no of documents for the Flying Club Member Number {fcMemberNumber}. Expected {addressHistoryDocs.Count()} rows but found {result.Count()} ");


            int i = 0;

            foreach (IWebElement element in result)
            {
                string actualAddressType = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string actualCompanyName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string actualAddressLine1 = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string actualAddressLine2 = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string actualAddressLine3 = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string actualCity = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                string actualState = element.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

                string actualCountry = element.FindElement(By.XPath("td[8]/div/div/span")).GetAttribute("value");

                string actualPostalCode = element.FindElement(By.XPath("td[9]/div/div/span")).GetAttribute("value");

                string actualValidFlag = element.FindElement(By.XPath("td[10]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedBy = element.FindElement(By.XPath("td[11]/div/div/span")).GetAttribute("value");

                string actualLastUpdatedOn = element.FindElement(By.XPath("td[12]/div/div/span")).GetAttribute("value");

                Assert.True(actualAddressType == addressHistoryDocs[i].AddressType,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Address Type to be {addressHistoryDocs[i].AddressType} but found {actualAddressType}");

                Assert.True(actualCompanyName == addressHistoryDocs[i].LandlordOrCompanyName,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Company Name to be {addressHistoryDocs[i].LandlordOrCompanyName} but found {actualCompanyName}");

                Assert.True(actualAddressLine1 == addressHistoryDocs[i].AddressLine1,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Address Line 1 to be {addressHistoryDocs[i].AddressLine1} but found {actualAddressLine1}");

                Assert.True(actualAddressLine2 == addressHistoryDocs[i].AddressLine2,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Address Line 2 to be {addressHistoryDocs[i].AddressLine2} but found {actualAddressLine2}");

                Assert.True(actualAddressLine3 == addressHistoryDocs[i].AddressLine3,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Address Line 3 to be {addressHistoryDocs[i].AddressLine3} but found {actualAddressLine3}");

                Assert.True(actualCity == addressHistoryDocs[i].City,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the City to be {addressHistoryDocs[i].City} but found {actualCity}");

                Assert.True(actualState == addressHistoryDocs[i].Province,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Province/State to be {addressHistoryDocs[i].Province} but found {actualState}");

                Assert.True(actualCountry == addressHistoryDocs[i].Country,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Country to be {addressHistoryDocs[i].Country} but found {actualCountry}");

                Assert.True(actualPostalCode == addressHistoryDocs[i].PostCode,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the PostalCode to be {addressHistoryDocs[i].PostCode} but found {actualPostalCode}");

                Assert.True(actualValidFlag == addressHistoryDocs[i].Valid,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the Valid Flag to be {addressHistoryDocs[i].Valid} but found {actualValidFlag}");

                Assert.True(actualLastUpdatedBy == addressHistoryDocs[i].LastUpdatedBy,
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the LastUpdatedBy to be {addressHistoryDocs[i].LastUpdatedBy} but found {actualLastUpdatedBy}");

                Assert.True(actualLastUpdatedOn == addressHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss"),
                   $"For Member {fcMemberNumber} ,On Row No {i + 1} ,Expected the LastUpdatedOn to be {addressHistoryDocs[i].LastUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss")} but found {actualLastUpdatedOn}");

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

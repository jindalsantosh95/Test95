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
    [Collection("MyOffersTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MyOffersTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly IWebDriver webDriver;
        private readonly MemberSearchPage memberSearchPage;
        private readonly MyOffersPage myOffersPage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public string env = Config.Environment;

        public MyOffersTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            memberSearchPage = new MemberSearchPage(webDriver);

            myOffersPage = new MyOffersPage(webDriver);

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);


        }

        [Trait("Category", Traits.MyOffersUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("Gold Companion Flight")]
        public void TC01_AdditionOfVoucher_SuccessScenario(string voucherName)
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_AdditionOfVoucher_SuccessScenario");

            string memberNumber = GetValidFCNumber();

            Assert.True(!string.IsNullOrEmpty(memberNumber), "Failed to fetch a Valid FC Number for test execution");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            myOffersPage.NavigateToMyOffersPage(logger);

            myOffersPage.ClickOnAddButtonOnMyOffersPage();

            myOffersPage.SelectTheVoucherFromTheDropdown(voucherName);

            myOffersPage.ClickOnAddVoucherOnAddPopUp();

            string expectedMessage = "Voucher rewarded successfully";

            string actualMessage = myOffersPage.GetPopUpMessageContent();

            Assert.True(expectedMessage.Equals(actualMessage), $"After adding the voucher expected the response message to be {expectedMessage} but found {actualMessage} .");

            myOffersPage.ClosePopUp();

            webDriver.WaitForElementToBeVisible(MyOffers.MyOffersPageDataTable);

            string currentDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

            string endDate = voucherName switch
            {
                "Gold Companion Flight" => DateTime.UtcNow.AddYears(1).ToString("dd-MMM-yyyy"),
                "Gold Lounge Pass" => DateTime.UtcNow.AddYears(1).ToString("dd-MMM-yyyy"),
                "Loyalty Companion Voucher" => DateTime.UtcNow.AddYears(2).ToString("dd-MMM-yyyy"),
                "Loyalty Upgrade Voucher" => DateTime.UtcNow.AddYears(2).ToString("dd-MMM-yyyy"),
                "Virgin Atlantic Credit Card Reward Voucher" => DateTime.UtcNow.AddYears(2).ToString("dd-MMM-yyyy"),
                _ => DateTime.UtcNow.AddYears(1).ToString("dd-MMM-yyyy")
            };

            bool flag = false;

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MyOffers.MyOffersPageDataTable));

            foreach (IWebElement element in result)
            {

                string voucherType = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                if (voucherType.Equals(voucherName))
                {
                    string issueDate = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                    string expiryDate = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                    flag = issueDate == currentDate && expiryDate == endDate;

                    if (flag)
                    { break; }
                }
            }

            Assert.True(flag, " No Voucher found with the start Date as the current Date ");

        }

        [Trait("Category", Traits.MyOffersUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_ValidateTheVoucherAuditDataOnMyOffersScreen()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_ValidateTheVoucherAuditDataOnMyOffersScreen");

            #region Test Data

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found in Audit Cosmos DB.");

            string customerId = awardAuditDoc.CustomerId;

            string queryForCustAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId);

            AwardAudit[] custAwardAuditDocs = cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery).OrderByDescending(x => x.AwardUpdatedOn).ToArray();

            Assert.True(custAwardAuditDocs.Any(), $" No Award Audit Docs found for customerId {customerId}");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Customer customerDoc = cosmosFixture.CosmosQueryItem<Customer>(parameterizedQuery);

            Assert.True(customerDoc is not null, $"No Customer Document Found in Cosmos DB for the Customer Id {customerId}.");

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            myOffersPage.NavigateToMyOffersPage(logger);

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);

            webDriver.WaitForElementToBeClickable(MyOffers.VoucherAddButton);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(MyOffers.MyOffersPageDataTable));

            string text;

            try
            {
                text = result.First().FindElement(By.XPath("td[1]")).Text;
            }
            catch
            {
                text = "";
            }

            string noDataMessage = "We didn't find any data to show at this time";

            Assert.True(!text.Equals(noDataMessage), "No Voucher Data Available on My Offers page. Expected Voucher Data as Audit data is present in audit Db.");

            for (int j = 1; j <= result.Count(); j++)
            {
                string voucherNumber = webDriver.GetAttributeTextValue(MyOffers.MyOffersPageDataTable + "[" + j + "]/td[1]/div/div/span", "value");

                AwardAudit[] voucherAudit = custAwardAuditDocs.Where(x => x.AwardNumber == voucherNumber).OrderByDescending(x => x.AwardUpdatedOn).ToArray();

                int auditDocsCount = voucherAudit.Count();

                webDriver.FindElementAndClick(MyOffers.MyOffersPageDataTable + "[" + j + "]/td[1]", "", 10);

                webDriver.WaitForElementToBeVisible(MyOffers.NavigateBackToMyOffersPageButton);

                webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);

                webDriver.WaitForElementToBePresent(MyOffers.MyOffersAuditPageDataTable + "[1]/td[1]", 10);

                IEnumerable<IWebElement> auditDataTable = webDriver.FindElements(By.XPath(MyOffers.MyOffersAuditPageDataTable));

                string message;

                try
                {
                    message = auditDataTable.First().FindElement(By.XPath("td[1]")).Text;
                }
                catch
                {
                    message = "";
                }

                string noAuditMessage = "No audit exists for this voucher";

                if (auditDocsCount == 0)
                {
                    Assert.True(auditDataTable.Count() == 1,
                        $"In case of no Audit docs, For the Voucher/Award Number {voucherNumber} Expected only row on audit page with default text message but row count was {auditDataTable.Count()}");

                    Assert.True(message.Equals(noAuditMessage),
                        $"In case of no Audit docs , For the Voucher/Award Number {voucherNumber} Expected the text message on the screen to be {noAuditMessage} but found {message}.");
                }

                else
                {

                    int i = 0;

                    foreach (IWebElement auditData in auditDataTable)
                    {
                        webDriver.TryFindElement("td[1]/div/div/span", 10);

                        string actualauditVoucherNumber = auditData.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherName = auditData.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherStatus = auditData.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherStartDate = auditData.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value") ?? "";

                        string actualauditVoucherEndDate = auditData.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value") ?? "";

                        string actualauditVoucherUsedDate = auditData.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value") ?? "";

                        string actualauditVoucherGiftedFromMemberNumber = auditData.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherGiftedToMemberNumber = auditData.FindElement(By.XPath("td[8]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherGiftedOn = auditData.FindElement(By.XPath("td[9]/div/div/span")).GetAttribute("value") ?? "";

                        string actualauditVoucherUpdatedBy = auditData.FindElement(By.XPath("td[10]/div/div/span")).GetAttribute("value");

                        string actualauditVoucherUpdatedOn = auditData.FindElement(By.XPath("td[11]/div/div/span")).GetAttribute("value") ?? "";

                        Assert.True(actualauditVoucherNumber == voucherAudit[i].AwardNumber,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher Number on Audit Screen to be {voucherAudit[i].AwardNumber} " +
                         $"but found {actualauditVoucherNumber}");

                        Assert.True(actualauditVoucherName == voucherAudit[i].AwardName,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher Name on Audit Screen to be {voucherAudit[i].AwardName} " +
                         $"but found {actualauditVoucherName}");

                        Assert.True(actualauditVoucherStatus == voucherAudit[i].Status,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher Status on Audit Screen to be {voucherAudit[i].Status} " +
                         $"but found {actualauditVoucherStatus}");

                        string expectedStartDate = voucherAudit[i].StartDate.HasValue ? voucherAudit[i].StartDate.Value.Date.ToString("dd-MMM-yyyy") : "";

                        Assert.True(actualauditVoucherStartDate == expectedStartDate,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher StartDate on Audit Screen to be {expectedStartDate} " +
                         $"but found {actualauditVoucherStartDate}");

                        string expectedEndDate = voucherAudit[i].EndDate.HasValue ? voucherAudit[i].EndDate.Value.Date.ToString("dd-MMM-yyyy") : "";

                        Assert.True(actualauditVoucherEndDate == expectedEndDate,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher EndDate on Audit Screen to be {expectedEndDate} " +
                         $"but found {actualauditVoucherEndDate}");

                        string expectedUsedDate = voucherAudit[i].UsedDate.HasValue ? voucherAudit[i].UsedDate.Value.Date.ToString("dd-MMM-yyyy") : "";

                        Assert.True(actualauditVoucherUsedDate == expectedUsedDate,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher UsedDate on Audit Screen to be {expectedUsedDate} " +
                         $"but found {actualauditVoucherUsedDate}");

                        Assert.True(actualauditVoucherGiftedFromMemberNumber == voucherAudit[i].GiftedFromMemberNumber,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher GiftedFromMemberNumber on Audit Screen to be {voucherAudit[i].GiftedFromMemberNumber} " +
                         $"but found {actualauditVoucherGiftedFromMemberNumber}");

                        Assert.True(actualauditVoucherGiftedToMemberNumber == voucherAudit[i].GiftedToMemberNumber,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher GiftedToMemberNumber on Audit Screen to be {voucherAudit[i].GiftedToMemberNumber} " +
                         $"but found {actualauditVoucherGiftedToMemberNumber}");

                        string expectedGiftedOnDate = voucherAudit[i].GiftedAwardDate.HasValue ? voucherAudit[i].GiftedAwardDate.Value.Date.ToString("dd-MMM-yyyy") : "";

                        Assert.True(actualauditVoucherGiftedOn == expectedGiftedOnDate,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher UsedDate on Audit Screen to be {expectedGiftedOnDate} " +
                         $"but found {actualauditVoucherGiftedOn}");

                        Assert.True(actualauditVoucherUpdatedBy == voucherAudit[i].AwardUpdatedBy,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher UpdatedBy on Audit Screen to be {voucherAudit[i].AwardUpdatedBy} " +
                         $"but found {actualauditVoucherUpdatedBy}");

                        string expectedUpdatedOnDate = voucherAudit[i].AwardUpdatedOn.HasValue ? voucherAudit[i].AwardUpdatedOn.Value.ToString("dd-MMM-yyyy HH:mm:ss") : "";

                        Assert.True(actualauditVoucherUpdatedOn == expectedUpdatedOnDate,
                         $"For the Voucher/Award Number {voucherNumber} ,Expected the Voucher UpdatedOn Date on Audit Screen to be {expectedUpdatedOnDate} " +
                         $"but found {actualauditVoucherUpdatedOn}");

                        i++;

                    }

                }

                myOffersPage.NavigateBackToMyOffersPageFromAuditPage();

            }

        }



        public string GetValidFCNumber()
        {
            string queryForFCMDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.MemberType = @memberType and c.Status = @status and c.SSAMemberNumber = '' and c.HeadOfGroupMemberNumber = '' order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@memberType", "Individual").WithParameter("@status", "Active");

            IEnumerable<Core.DB_Model.Entities.FlyingClubMember> fcmDocs = cosmosFixture.CosmosQueryItems<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            return fcmDocs.FirstOrDefault(x => !(x.RetainGoldCounter.Has(".") && x.RetainSilvercounter.Has(".")) && string.IsNullOrEmpty(x.TelephonePassword)).MemberNumber;
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

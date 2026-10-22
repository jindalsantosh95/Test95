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
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("VoucherRedemptionTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class VoucherRedemptionTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private TokenFixture tokenFixture;
        private IWebDriver webDriver;

        private MemberSearchPage memberSearchPage;
        private RedemptionPage redemptionPage;

        private readonly string customerTransactionCosmosDb;
        private readonly string customerCosmosDb;

        public string env = Config.Environment;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public VoucherRedemptionTests(ReportFixture reportFixture, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            redemptionPage = new RedemptionPage(webDriver);

        }

        [Trait("Category", Traits.VoucherRedemptionUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("Reward voucher redemption - Upgrade")]
        [InlineData("Reward voucher redemption - Companion")]
        public void CreateVoucherRedemption_SuccessScenario(string product)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"CreateVoucherRedemption_SuccessScenarioFor_{product}");
            #region Member and Voucher Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve a valid member whose status is valid for Create Accrual Transaction");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId,ExpirationPeriod from dbo.Product where ProductType ='Electronic Voucher' and active = '1' and RowId != '1-1ZKH' order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            string voucherExpiryPeriod = voucherDetails[0][2]?.ToString();

            DateTime currentDate = DateTime.UtcNow;

            string startDate = currentDate.ToString("yyyy-MM-dd");

            string endDate = currentDate.AddYears(voucherExpiryPeriod == "24" ? 2 : 1).ToString("yyyy-MM-dd");

            #endregion

            #region Create Voucher API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            string expectedResponse = "Voucher rewarded successfully";

            Assert.True(result.Has(expectedResponse), $"Expected {expectedResponse} but was {result} .");

            string queryForVoucherNumberDetails = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.CustomerId = @customerId and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition parameterizedVoucherNumberQuery = new QueryDefinition(query: queryForVoucherNumberDetails).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@productId", voucherProductId);

            Award fcmAwardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedVoucherNumberQuery);

            string voucherNumber = fcmAwardDoc.AwardNumber;

            #endregion

            #region PowerApps navigation for Voucher remption transaction creation

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            redemptionPage.NavigateToRedemptionPage();

            redemptionPage.ClickOnClearAllButton();

            string activityDate = DataGenerator.RandomFutureDate(20).ToString("dd-MMM-yyyy");

            string partnerCode = "VS";

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string pointsToBeAccrued = "20000";

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            redemptionPage.EnterPartnerCode(partnerCode);

            redemptionPage.EnterProduct(product);

            redemptionPage.EnterActivityDate(activityDate);

            redemptionPage.EnterPoints("0");

            redemptionPage.EnterComments("Test Transaction");

            redemptionPage.EnterOriginAirportCode(originAirportCode);

            redemptionPage.EnterDestinationAirportCode(destinationAirportCode);

            redemptionPage.EnterPNR(pnr);

            redemptionPage.EnterBookingClass("J");

            redemptionPage.EnterNoOfPassengers("1");

            redemptionPage.EnterNoOfSegments("1");

            redemptionPage.SelectVoucher(voucherNumber);

            redemptionPage.EnterVoucherPointsToBeCredited(pointsToBeAccrued);

            redemptionPage.ClickOnApplyRedemptionButton();

            string expectedMessage = "Successful Manual Redemption Applied";

            string actualMemberNumber = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[3]");

            string actualActivityDate = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[5]");

            string actualPartnerCode = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[6]");

            string actualProduct = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[8]");

            string actualMessage = webDriver.GetElementText(Redemption.PopUpMessageContent + "/p/b");

            Assert.True(memberNumber.Equals(actualMemberNumber),
                $"Expected the FC Number on the Pop up to be {memberNumber} but found {actualMemberNumber}.");

            Assert.True(activityDate.Equals(actualActivityDate),
                $"Expected the Activity Date on the Pop up to be {activityDate} but found {actualActivityDate}.");

            Assert.True(partnerCode.Equals(actualPartnerCode),
                $"Expected the PartnerCode on the Pop up to be {partnerCode} but found {actualPartnerCode}.");

            Assert.True(product.Equals(actualProduct),
                $"Expected the Product on the Pop up to be {product} but found {actualProduct}.");

            Assert.True(expectedMessage.Equals(actualMessage),
               $"Expected the Success Message on the Pop up to be {expectedMessage} but found {actualMessage}.");

            redemptionPage.ClickOnClosePopupIcon();

            #endregion

            #region Accrual transaction validations on the member transaciton detail page

            bool flag = false;

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(ElementsLibrary.MemberProfile.MemberDetailTxnList));

            foreach (IWebElement element in txnList)
            {

                string txnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                string pointsOnPA = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                pointsOnPA = pointsOnPA.Has("(") ? pointsOnPA.Replace("(", "").Replace(")", "") : pointsOnPA;

                string actualTxnDate = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                string ActivityDateonPA = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                if (txnType.Equals("Accrual") && pointsOnPA.Equals(pointsToBeAccrued.ToString()) && actualTxnDate.Equals(DateTime.UtcNow.ToString("dd-MMM-yyyy")) && ActivityDateonPA.Equals(DateTime.UtcNow.ToString("dd-MMM-yyyy")))
                {

                    string actualStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualProductonPA = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    string StatementDescriptiononPA = element.FindElement(By.XPath("td[14]/div/div/span")).Text;

                    Assert.True("Processed".Equals(actualStatus),
                        $"Expected the Transaction Status to be Processed but found {actualStatus}.");

                    Assert.True("Voucher Points".Equals(actualProductonPA),
                       $"Expected the Transaction Product to be Voucher Points but found {actualProductonPA}.");

                    Assert.True("Points credit for reward voucher redemption".Equals(StatementDescriptiononPA),
                       $"Expected the Statement description on Member Txn detail page to be Points credit for reward voucher redemption but found {StatementDescriptiononPA}.");

                    flag = true;

                    break;
                }
            }
            Assert.True(flag, "The Transaction not present in the Transaction history (Member Detail Page)");
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

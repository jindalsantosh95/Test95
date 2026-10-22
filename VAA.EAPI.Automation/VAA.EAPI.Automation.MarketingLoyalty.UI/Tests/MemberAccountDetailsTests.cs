using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("MemberAccountDetailsTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberAccountDetailsTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private MemberAccountDetailsPage memberAccountDetailsPage;

        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;
        private MemberProfilePage memberProfilePage;

        private readonly string customerTransactionCosmosDb;

        public MemberAccountDetailsTests(ReportFixture reportFixture, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            memberAccountDetailsPage = new MemberAccountDetailsPage(webDriver);
        }

        [Trait("Category", Traits.MemberAccountDetailsUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("Red")]
        [InlineData("Silver")]
        [InlineData("Gold")]
        public void TC01_MemberAccountDetailsSuccessScenario(string tierCode)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC01_MemberAccountDetails{tierCode}MemberSuccessScenario");

            #region - Pre-Requisite for fetching a Red member from COSMOS DB

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.TierCode = @tierCode order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@tierCode", tierCode);

            Core.DB_Model.Entities.FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, $"Failed to retrieve a {tierCode} member for test execution");

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            string customerId = flyingClubMemberDoc.CustomerId;

            #endregion

            #region - Pre-Requisite - Retrieve member details from the graphql endpoint

            HttpResponseMessage response = GraphQL.RetrieveMemberAccountDetails(env, tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
              $" but was {retrieveMemberAccountResult?.ToString()}");

            string byDate = DateTime.Parse(retrieveMemberAccountResult.data.loyaltyMemberAccount.byDate.ToString()).ToString("dd-MMM-yyyy");

            string rollingPointTotal = retrieveMemberAccountResult.data.loyaltyMemberAccount.rollingPointTotal.ToString() ?? "0";

            string retainPointTotal = retrieveMemberAccountResult.data.loyaltyMemberAccount.retainPointTotal.ToString() ?? "0";

            string totalVsflownMiles = retrieveMemberAccountResult.data.loyaltyMemberAccount.totalVsflownMiles.ToString() ?? "0";

            string tierStartDate = DateTime.Parse(retrieveMemberAccountResult.data.loyaltyMemberAccount.tierStartDate.ToString()).ToString("dd-MMM-yyyy");

            string tierEndDate = DateTime.Parse(retrieveMemberAccountResult.data.loyaltyMemberAccount.tierEndDate.ToString()).ToString("dd-MMM-yyyy");

            string noOfYearsInGoldTier = retrieveMemberAccountResult.data.loyaltyMemberAccount.numberofYearsAtGold.ToString() ?? "0";

            string totalVSTierPointsInCurrentYear = retrieveMemberAccountResult.data.loyaltyMemberAccount.totalVSTierPointsInCurrentYear.ToString() ?? "0";

            string totalVSTierPointsSinceBecomingGold = retrieveMemberAccountResult.data.loyaltyMemberAccount.totalVSTierPointsSinceBecomingGold.ToString() ?? "0";

            string tierPointsForRedToSilver = "400";

            string tierPointsForRetainToSilver = "400";

            string tierPointsForSilverToGold = "1000";

            string tierPointsForRetainToGold = "1000";

            #endregion 

            #region - PowerApps Validations for the Member Account details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            memberAccountDetailsPage.ClickOnAccountDetailsTab();

            memberAccountDetailsPage.ClickOnRefreshBtn(tierCode);

            if (tierCode == "Red")
            {
                string actualTotalTierPointsNeededToUpgradeToSilver = webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.TotalTierPointsNeededToUpgradeToSilverTextBox, "value");

                string actualByDateToUpgradeToSilverTier = DateTime.Parse(webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.ByDate_ToSilverTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualTierPointsEarnedToUpgradeToSilverTier = webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.TierPointsEarnedToUpgradeToSilverTextBox, "value");

                string actualTotalTierPointsNeededToUpgradeToGold = webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.TotalTierPointsNeededToUpgradeToGoldTextBox, "value");

                string actualByDateToUpgradeToGoldTier = DateTime.Parse(webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.ByDate_ToGoldTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualTierPointsEarnedToUpgradeToGoldTier = webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.TierPointsEarnedToUpgradeToGoldTextBox, "value");

                string actualTotalVSFlownMiles = webDriver.GetAttributeTextValue(RedTierMemberAccountDetails.TotalVSFlownMilesTextBox, "value");

                long expectedTotalTierPointsNeededToUpgradeToSilverTier = Convert.ToInt64(tierPointsForRedToSilver) - Convert.ToInt64(rollingPointTotal) < 0 ? 0 :
                    Convert.ToInt64(tierPointsForRedToSilver) - Convert.ToInt64(rollingPointTotal);

                Assert.True(Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToSilver) == expectedTotalTierPointsNeededToUpgradeToSilverTier,
                    $"For {tierCode} , Expected the total tier points needed to upgrade to Silver Tier to be {expectedTotalTierPointsNeededToUpgradeToSilverTier}" +
                    $" but found {Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToSilver)}");

                Assert.True(actualByDateToUpgradeToSilverTier == byDate,
                    $"For {tierCode} , Expected the ByDate To upgrade to Silver Tier to be {byDate} but found {actualByDateToUpgradeToSilverTier}");

                Assert.True(actualTierPointsEarnedToUpgradeToSilverTier == rollingPointTotal,
                    $"For {tierCode} , Expected the total tier points earned to upgrade to Silver Tier to be {rollingPointTotal} but found {actualTierPointsEarnedToUpgradeToSilverTier}");

                long expectedTotalTierPointsNeededToUpgradeToGoldTier = Convert.ToInt64(tierPointsForSilverToGold) - Convert.ToInt64(rollingPointTotal) < 0 ? 0 :
                    Convert.ToInt64(tierPointsForSilverToGold) - Convert.ToInt64(rollingPointTotal);

                Assert.True(Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToGold) == expectedTotalTierPointsNeededToUpgradeToGoldTier,
                    $"For {tierCode} , Expected the total tier points needed to upgrade to Gold Tier to be {expectedTotalTierPointsNeededToUpgradeToGoldTier} " +
                    $"but found {Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToGold)}");

                Assert.True(actualByDateToUpgradeToGoldTier == byDate,
                    $"For {tierCode} , Expected the ByDate To upgrade to Gold Tier to be {byDate} but found {actualByDateToUpgradeToGoldTier}");

                Assert.True(actualTierPointsEarnedToUpgradeToGoldTier == rollingPointTotal,
                    $"For {tierCode} , Expected the total tier points earned to upgrade to Gold Tier to be {rollingPointTotal} but found {actualTierPointsEarnedToUpgradeToGoldTier}");

                Assert.True(actualTotalVSFlownMiles == totalVsflownMiles,
                   $"For {tierCode} , Expected the VS Flown Files to be {totalVsflownMiles} but found {actualTotalVSFlownMiles}");

            }

            if (tierCode == "Silver")
            {
                string actualTotalTierPointsNeededRetainToSilver = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TotalTierPointsNeededForSilverRenewalTextBox, "value");

                string actualCurrentTierRenewalStartDate = DateTime.Parse(webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TierRenewalFromDateTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualCurrentTierRenewalEndDate = DateTime.Parse(webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TierRenewalToDateTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualTierPointsEarnedToRetainToSilverTier = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TierPointEarnedForSilverTierRenewalTextBox, "value");

                string actualTotalTierPointsNeededToUpgradeToGold = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TotalTierPointsNeededToUpgradeToGoldTextBox, "value");

                string actualByDateToUpgradeToGoldTier = DateTime.Parse(webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.ByDate_ToGoldTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualTierPointsEarnedToUpgradeToGoldTier = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TotalTierPointsEarnedToUpgradeToGoldTextBox, "value");

                string actualTotalVSFlownMiles = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TotalVSFlownMilesTextBox, "value");

                string actualTotalVSTierPointsEarnedCurrentYear = webDriver.GetAttributeTextValue(SilverTierMemberAccountDetails.TotalVSTierPtsInCurrentMembershipYearTextBox, "value");

                long expectedTotalTierPointsNeededToRetainToSilverTier = Convert.ToInt64(tierPointsForRetainToSilver) - Convert.ToInt64(retainPointTotal) < 0 ? 0 :
                    Convert.ToInt64(tierPointsForRetainToSilver) - Convert.ToInt64(retainPointTotal);

                Assert.True(Convert.ToInt64(actualTotalTierPointsNeededRetainToSilver) == expectedTotalTierPointsNeededToRetainToSilverTier,
                    $"For {tierCode} , Expected the total tier points needed to Retain to Silver Tier to be {expectedTotalTierPointsNeededToRetainToSilverTier} " +
                    $"but found {Convert.ToInt64(actualTotalTierPointsNeededRetainToSilver)}");

                Assert.True(actualCurrentTierRenewalStartDate == tierStartDate,
                    $"For {tierCode} , Expected the Tier StartDate of current silver to be {tierStartDate} but found {actualCurrentTierRenewalStartDate}");

                Assert.True(actualCurrentTierRenewalEndDate == tierEndDate,
                    $"For {tierCode} , Expected the Tier EndDate of current silver to be {tierEndDate} but found {actualCurrentTierRenewalEndDate}");

                Assert.True(actualTierPointsEarnedToRetainToSilverTier == retainPointTotal,
                    $"For {tierCode} , Expected the total tier points earned to Retain to Silver Tier to be {retainPointTotal} but found {actualTierPointsEarnedToRetainToSilverTier}");

                long expectedTotalTierPointsNeededToUpgradeToGoldTier = Convert.ToInt64(tierPointsForSilverToGold) - Convert.ToInt64(rollingPointTotal) < 0 ? 0 :
                    Convert.ToInt64(tierPointsForSilverToGold) - Convert.ToInt64(rollingPointTotal);

                Assert.True(Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToGold) == expectedTotalTierPointsNeededToUpgradeToGoldTier,
                    $"For {tierCode} , Expected the total tier points needed to upgrade to Gold Tier to be {expectedTotalTierPointsNeededToUpgradeToGoldTier} " +
                    $"but found {Convert.ToInt64(actualTotalTierPointsNeededToUpgradeToGold)}");

                Assert.True(actualByDateToUpgradeToGoldTier == byDate,
                    $"For {tierCode} , Expected the ByDate To upgrade to Gold Tier to be {byDate} but found {actualByDateToUpgradeToGoldTier}");

                Assert.True(actualTierPointsEarnedToUpgradeToGoldTier == rollingPointTotal,
                    $"For {tierCode} , Expected the total tier points earned to upgrade to Gold Tier to be {rollingPointTotal} but found {actualTierPointsEarnedToUpgradeToGoldTier}");

                Assert.True(actualTotalVSFlownMiles == totalVsflownMiles,
                   $"For {tierCode} , Expected the VS Flown Files to be {totalVsflownMiles} but found {actualTotalVSFlownMiles}");

                Assert.True(actualTotalVSTierPointsEarnedCurrentYear == totalVSTierPointsInCurrentYear,
                   $"For {tierCode} , Expected the VS Flown Files to be {totalVSTierPointsInCurrentYear} but found {actualTotalVSTierPointsEarnedCurrentYear}");

            }

            if (tierCode == "Gold")
            {
                string actualTotalTierPointsNeededRetainToGold = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalTierPointsNeededForGoldRenewalTextBox, "value");

                string actualCurrentTierRenewalStartDate = DateTime.Parse(webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TierRenewalFromDateTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualCurrentTierRenewalEndDate = DateTime.Parse(webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TierRenewalToDateTextBox, "value")).ToString("dd-MMM-yyyy");

                string actualTierPointsEarnedToRetainToGoldTier = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TierPointEarnedForGoldTierRenewalTextBox, "value");

                string actualNoOfConsecutiveYearsInGoldTier = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.NoOfConsecutiveYearsAtGold, "value");

                string actualVSTierPointsSinceBecomingGold = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalVSTierPointsSinceBecomingGold, "value");

                string actualTotalVSFlownMiles = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalVSFlownMilesTextBox, "value");

                string actualTotalVSTierPointsEarnedCurrentYear = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalVSTierPtsInCurrentMembershipYearTextBox, "value");

                long expectedTierPointsNeededToRetainToGold = Convert.ToInt64(tierPointsForRetainToGold) - Convert.ToInt64(retainPointTotal) < 0 ? 0 :
                    Convert.ToInt64(tierPointsForRetainToGold) - Convert.ToInt64(retainPointTotal);

                Assert.True(Convert.ToInt64(actualTotalTierPointsNeededRetainToGold) == expectedTierPointsNeededToRetainToGold,
                    $"For {tierCode} , Expected the total tier points needed to Retain to Gold Tier to be {expectedTierPointsNeededToRetainToGold} " +
                    $"but found {Convert.ToInt64(actualTotalTierPointsNeededRetainToGold)}");

                Assert.True(actualCurrentTierRenewalStartDate == tierStartDate,
                    $"For {tierCode} , Expected the Tier StartDate of current Gold to be {tierStartDate} but found {actualCurrentTierRenewalStartDate}");

                Assert.True(actualCurrentTierRenewalEndDate == tierEndDate,
                    $"For {tierCode} , Expected the Tier EndDate of current Gold to be {tierEndDate} but found {actualCurrentTierRenewalEndDate}");

                Assert.True(actualTierPointsEarnedToRetainToGoldTier == retainPointTotal,
                    $"For {tierCode} , Expected the total tier points earned to Retain to Gold Tier to be {retainPointTotal} but found {actualTierPointsEarnedToRetainToGoldTier}");

                Assert.True(actualNoOfConsecutiveYearsInGoldTier == noOfYearsInGoldTier,
                    $"For {tierCode} , Expected the total No Of Consecutive Years Gold Tier to be {noOfYearsInGoldTier} but found {actualNoOfConsecutiveYearsInGoldTier}");

                Assert.True(actualVSTierPointsSinceBecomingGold == totalVSTierPointsSinceBecomingGold,
                    $"For {tierCode} , Expected the total VS tier points earned Since Becoming Gold to be {totalVSTierPointsSinceBecomingGold} but found {actualVSTierPointsSinceBecomingGold}");

                Assert.True(actualTotalVSFlownMiles == totalVsflownMiles,
                   $"For {tierCode} , Expected the VS Flown Files to be {totalVsflownMiles} but found {actualTotalVSFlownMiles}");

                Assert.True(actualTotalVSTierPointsEarnedCurrentYear == totalVSTierPointsInCurrentYear,
                   $"For {tierCode} , Expected the VS Flown Files to be {totalVSTierPointsInCurrentYear} but found {actualTotalVSTierPointsEarnedCurrentYear}");

            }


            #endregion
        }

        [Trait("Category", Traits.MemberAccountDetailsUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_WingsMemberDetailsValidation()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_WingsMemberDetailsValidation");

            string wingsMemberProductId = "1634561747";

            DateTime currentDate = DateTime.Now;

            string startDate = currentDate.AddDays(-1).ToString("yyyy-MM-dd");

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId = @productId and c.StartDate <= @startDate and c.ExpiryDate = @expiryDate order by c.StartDate desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@productId", wingsMemberProductId).WithParameter("@startDate", startDate).WithParameter("@expiryDate", "");

            Core.DB_Model.Entities.Award awardDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            Assert.True(awardDoc is not null,
              "Failed to Retrieve Wings Member for test execution.");

            string customerId = awardDoc.CustomerId;

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Test Data not found for the Test Execution");

            Assert.True(fcmDoc.TierCode == "Gold", $"Invalid Test Data found for the Test Execution. Member is not a Gold Member .The Member Tier is {fcmDoc.TierCode}");

            string memberNumber = fcmDoc.MemberNumber;

            #region - Pre-Requisite - Retrieve member details from the graphql endpoint

            HttpResponseMessage response = GraphQL.RetrieveMemberAccountDetails(env, tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
              $" but was {retrieveMemberAccountResult?.ToString()}");

            string expectedWingsStartDate = DateTime.Parse(retrieveMemberAccountResult.data.loyaltyMemberAccount.wingsStartDate.ToString()).ToString("dd-MMM-yyyy");

            string expectedTotalTierPointsSinceBecomingWings = retrieveMemberAccountResult.data.loyaltyMemberAccount.totalTierPointsSinceBecomingWings.ToString() ?? "0";

            string expectedTotalVSTierPointsSinceBecomingWings = retrieveMemberAccountResult.data.loyaltyMemberAccount.totalVSTierPointsSinceBecomingWings.ToString() ?? "0";

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            memberAccountDetailsPage.ClickOnAccountDetailsTab();

            memberAccountDetailsPage.ClickOnRefreshBtn(fcmDoc.TierCode);

            string actualWingsStartDate = DateTime.Parse(webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.WingsStartDate, "value")).ToString("dd-MMM-yyyy");

            string actualTierPointsEarnedSinceBecomingWings = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalTierPointsEarnedSinceBecomingWings, "value");

            string actualVSTierPointsEarnedSinceBecomingWings = webDriver.GetAttributeTextValue(GoldTierMemberAccountDetails.TotalVSTierPointsEarnedSinceBecomingWings, "value");

            Assert.True(actualWingsStartDate == expectedWingsStartDate,
                 $"For {memberNumber} , Expected the Wings StartDate to be {expectedWingsStartDate} but found {actualWingsStartDate}");

            Assert.True(actualTierPointsEarnedSinceBecomingWings == expectedTotalTierPointsSinceBecomingWings,
                 $"For {memberNumber} , Expected the Total Tier Points Earned Since Becoming Wings to be {expectedTotalTierPointsSinceBecomingWings} but found {actualTierPointsEarnedSinceBecomingWings}");

            Assert.True(actualVSTierPointsEarnedSinceBecomingWings == expectedTotalVSTierPointsSinceBecomingWings,
                 $"For {memberNumber} , Expected the Total VS Tier Points Earned Since Becoming Wings to be {actualVSTierPointsEarnedSinceBecomingWings} but found {actualVSTierPointsEarnedSinceBecomingWings}");
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

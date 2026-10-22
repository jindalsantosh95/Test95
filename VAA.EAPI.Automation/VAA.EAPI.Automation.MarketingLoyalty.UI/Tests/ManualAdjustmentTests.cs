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

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("ManualAdjustmentTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class ManualAdjustmentTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private ManualAdjustmentPage manualAdjustmentPage;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public ManualAdjustmentTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            manualAdjustmentPage = new ManualAdjustmentPage(webDriver);

        }

        [Trait("Category", Traits.ManualAdjustmentUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("VS")]
        public void TC01_ManualAdjustmentForPointsSuccessScenario(string partnerCode)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC01_ManualAdjustmentForPointsSuccessScenarioForPartnerCode{partnerCode}");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            manualAdjustmentPage.ClickOnMenuBarOnMemberProfilePage();

            string points = DataGenerator.RandomNumber(1, 100, true);

            string activityDate = DataGenerator.RandomPastDate(182).ToString("dd-MMM-yyyy");

            manualAdjustmentPage.EnterPartnerCode(partnerCode);

            manualAdjustmentPage.EnterProduct("Authorised Points Credit");

            manualAdjustmentPage.EnterPoints(points);

            manualAdjustmentPage.EnterActivityDate(activityDate);

            manualAdjustmentPage.EnterComments("Test Transaction");

            manualAdjustmentPage.ClickOnApplyManualAdjustmentButton();

            string text = manualAdjustmentPage.GetPopUpMessageContent();

            string expectedMessage = "Success - Manual Adjustment";

            Assert.True(text.Has(expectedMessage),
                "Manual Adjustment transaction was not created for the member");
        }


        [Trait("Category", Traits.ManualAdjustmentUIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("VS")]
        public void TC02_ManualAdjustmentForTierPointsSuccessScenario(string partnerCode)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC02_ManualAdjustmentForTierPointsSuccessScenario-{partnerCode}");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            manualAdjustmentPage.ClickOnMenuBarOnMemberProfilePage();

            string tierPoints = DataGenerator.RandomNumber(10, 100, true);

            string activityDate = DataGenerator.RandomPastDate(182).ToString("dd-MMM-yyyy");

            manualAdjustmentPage.EnterPartnerCode(partnerCode);

            manualAdjustmentPage.EnterProduct("Tier Points Adjustment");

            manualAdjustmentPage.EnterTierPoints(tierPoints);

            manualAdjustmentPage.EnterActivityDate(activityDate);

            manualAdjustmentPage.EnterComments("Test Transaction");

            manualAdjustmentPage.ClickOnApplyManualAdjustmentButton();

            string text = manualAdjustmentPage.GetPopUpMessageContent();

            string expectedMessage = "Success - Manual Adjustment";

            Assert.True(text.Has(expectedMessage),
                "Manual Adjustment transaction was not created for the member");
        }

        [Trait("Category", Traits.ManualAdjustmentUIRegression)]
        [Theory, RunOrder(3)]
        [InlineData("partnerCode")]
        [InlineData("points")]
        [InlineData("comment")]
        public void TC03_ApplyButtonValidationWhenMandatoryFieldValuesAreMissing(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC03_ApplyButtonValidationWhenMandatoryField-{mandatoryField}-ValuesAreMissing");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            manualAdjustmentPage.ClickOnMenuBarOnMemberProfilePage();

            string points = DataGenerator.RandomNumber(10, 100, true);

            string activityDate = DataGenerator.RandomPastDate(182).ToString("dd-MMM-yyyy");

            string partnerCode = "VS";

            string product = "Authorised Points Credit";

            string comments = "Test Manual Adjustment Transaction";

            switch (mandatoryField)
            {
                case "partnerCode":
                    partnerCode = "";
                    product = "";
                    break;

                case "points":
                    points = "";
                    break;

                case "comment":
                    comments = "";
                    break;

                default:
                    break;

            }

            manualAdjustmentPage.EnterPartnerCode(partnerCode);

            manualAdjustmentPage.EnterProduct(product);

            manualAdjustmentPage.EnterPoints(points);

            manualAdjustmentPage.EnterActivityDate(activityDate);

            manualAdjustmentPage.EnterComments(comments);

            Assert.True(manualAdjustmentPage.IsApplyManualAdjustmentBuuttonDisabled(),
                $"Expected the Apply Button to be in disabled state when Mandatory field {mandatoryField} value is missing.");
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

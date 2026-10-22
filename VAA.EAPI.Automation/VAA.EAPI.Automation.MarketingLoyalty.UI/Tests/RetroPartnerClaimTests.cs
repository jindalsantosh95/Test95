using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("RetroPartnerClaimTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetroPartnerClaimTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private RetroPartnerClaimPage retroPartnerClaimPage;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public RetroPartnerClaimTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            retroPartnerClaimPage = new RetroPartnerClaimPage(webDriver);
        }


        [Trait("Category", Traits.RetroPartnerUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_RetroClaimSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_TC01_RetroClaimSuccessScenario");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            retroPartnerClaimPage.ClickOnMenuBarOnMemberProfilePage();

            retroPartnerClaimPage.SelectRetroClaims(logger, RetroClaims.SkyTeamAutoRetro.ToString());

            string operAirlinePartnerCode = "AM";

            string operBookingClass = "A";

            string markBookingClass = "B";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string cabinClass = "Business (C)";

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string seatNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            string pnrNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            retroPartnerClaimPage.EnterOperAirPartnerCode(operAirlinePartnerCode);

            retroPartnerClaimPage.EnterOperBookingClass(operBookingClass);

            retroPartnerClaimPage.EnterMarkBookingClass(markBookingClass);

            retroPartnerClaimPage.EnterOperFlightNumber(flightNumber);

            retroPartnerClaimPage.EnterOriginAiport(originAirportCode);

            retroPartnerClaimPage.EnterDestinationAiport(destinationAirportCode);

            retroPartnerClaimPage.EnterCabinClass(cabinClass);

            retroPartnerClaimPage.EnterActivityDate(activityDate);

            retroPartnerClaimPage.EnterSeatNumber(seatNumber);

            retroPartnerClaimPage.EnterTicketNumber(ticketNumber);

            retroPartnerClaimPage.EnterPnrNumber(pnrNumber);

            retroPartnerClaimPage.ClickOnCreateButton();

            string[] text = retroPartnerClaimPage.GetMessageFromThePopUpScreen();

            string expectedMessage = "SkyTeam partner retro transaction created successfully.";

            Assert.True(text[0].Contains(expectedMessage),
                "SkyTeam partner retro transaction was not created for the member");

            retroPartnerClaimPage.CloseThePopUp();
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

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
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("TierHistoryTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class TierHistoryTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly IWebDriver webDriver;
        private readonly MemberSearchPage memberSearchPage;
        private readonly TierHistoryPage tierHistoryPage;
        private readonly MemberProfilePage memberProfilePage;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public TierHistoryTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            memberSearchPage = new MemberSearchPage(webDriver);
            tierHistoryPage = new TierHistoryPage(webDriver);
            memberProfilePage = new MemberProfilePage(webDriver);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }


        [Trait("Category", Traits.TierHistoryUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_ValidationOfActiveTierDetails()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_ValidationOfActiveTierDetails");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null,
             "Failed to Retrieve Test Data");

            string memberNumber = fcmDoc.MemberNumber;

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.Active = @activeFlag order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@customerId", fcmDoc.CustomerId).WithParameter("@activeFlag", "Y");

            MembershipChange activeMembershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

            Assert.True(activeMembershipChangeDoc is not null, "Failed to retrieve Active MembershipChange Doc");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            Dictionary<string, string> activeTierDetails = tierHistoryPage.GetActiveTierDetails();

            Assert.True(activeTierDetails.Any(), "No Active Tier Details Found");

            Assert.True(activeTierDetails["TierCode"].Equals(activeMembershipChangeDoc.TierCode),
                $"Expected the Active Tier Code to be {activeMembershipChangeDoc.TierCode} but found {activeTierDetails["TierCode"]}");

            Assert.True(activeTierDetails["StartDate"].Equals(activeMembershipChangeDoc.StartDate.Value.ToString("dd-MMM-yyyy")),
                $"Expected the Active Tier StartDate to be {activeMembershipChangeDoc.StartDate.Value.ToString("dd-MMM-yyyy")} but found {activeTierDetails["StartDate"]}");

            Assert.True(activeTierDetails["EndDate"].Equals(activeMembershipChangeDoc.EndDate.Value.ToString("dd-MMM-yyyy")),
               $"Expected the Active Tier EndDate to be {activeMembershipChangeDoc.EndDate.Value.ToString("dd-MMM-yyyy")} but found {activeTierDetails["EndDate"]}");

            Assert.True(activeTierDetails["UserCreated"].Equals(activeMembershipChangeDoc.CreatedBy),
                $"Expected the Active Tier CreatedBy to be {activeMembershipChangeDoc.CreatedBy} but found {activeTierDetails["UserCreated"]}");

        }

        [Trait("Category", Traits.TierHistoryUIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("Red", "Silver", "Manual Upgrades")]
        [InlineData("Red", "Gold", "Manual Upgrades")]
        [InlineData("Silver", "Gold", "Manual Upgrades")]
        [InlineData("Silver", "Red", "Manual Downgrades")]
        [InlineData("Gold", "Red", "Manual Downgrades")]
        [InlineData("Gold", "Silver", "Manual Downgrades")]
        public void TC02_ManualTierChanges_SuccessScenario(string currentTier, string newTier, string issueReason)
        {
            ExtentTest logger = reportFixture.Current.CreateTest($"TC02_ManualTierChanges_SuccessScenario - From {currentTier} To {newTier}");

            #region Test Data

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode = @tierCode and c.Active = @activeFlag and c.StartDate < @startDate and c.DowngradeProhibitReason in ('',null) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@tierCode", currentTier).WithParameter("@activeFlag", "Y").WithParameter("@startDate", DateTime.UtcNow.AddDays(-4).Date.ToString("yyyy-MM-dd"));

            MembershipChange membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

            Assert.True(membershipChangeDoc is not null,
             "Failed to Retrieve MembershipChange Doc with active tier");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", membershipChangeDoc.CustomerId);

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to FCM Doc for Test Data");

            string memberNumber = fcmDoc.MemberNumber;

            Assert.True(fcmDoc.TierCode.Equals(membershipChangeDoc.TierCode),
            "Tier Code not same in fcmDoc and MembershipChange active tier doc.");


            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            tierHistoryPage.ClickOnReviseButton();

            tierHistoryPage.SelectNewTier(newTier);

            tierHistoryPage.SelectTierIssueReason(issueReason);

            tierHistoryPage.ClickOnReviseMemberTierButton();

            webDriver.TryFindElementAndClick(TierHistory.SuppHHWarningYesButton, "Supp Silver Or HH Tier downgrade warning Message Yes Button", 5);

            string actualMessage = tierHistoryPage.GetPopUpMessageContent();

            string expectedMessage = "Tier Created Successfully";

            Assert.True(actualMessage.Equals(expectedMessage),
                           $"After Clicking on Member Tier Revise Button ,Expected Message to be {expectedMessage} but found {actualMessage}");

            tierHistoryPage.ClosePopUp();

            tierHistoryPage.RefreshTierHistoryPage();

            webDriver.WaitForElementToBeVisible(TierHistory.TierHistoryDataTable, 10);

            IEnumerable<IWebElement> tierhistories = webDriver.FindElements(By.XPath(TierHistory.TierHistoryDataTable));

            bool flag = false;

            string newTierStartDate = DateTime.UtcNow.Date.AddDays(+1).ToString("dd-MMM-yyyy");

            foreach (IWebElement element in tierhistories)
            {
                string tierCode = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string tierStartDate = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string activeFlag = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string actualIssueReason = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                if (tierCode.Equals(newTier) && tierStartDate.Equals(newTierStartDate) && activeFlag.Equals("N") && actualIssueReason.Equals(issueReason))
                {
                    element.FindElement(By.XPath("td[1]/div/div/span")).Click();

                    flag = true;

                    break;
                }

            }

            Assert.True(flag, "New Tier Row is not Created");

            tierHistoryPage.ClickOnManualTierChangeButton();

            actualMessage = tierHistoryPage.GetPopUpMessageContent();

            expectedMessage = "Request Initiated successfully";

            Assert.True(actualMessage.Equals(expectedMessage),
                           $"After Clicking on Manual Tier Change Button ,Expected Message to be {expectedMessage} but found {actualMessage}");

            tierHistoryPage.ClosePopUp();

            tierHistoryPage.ClickOnMenuBarOnTierHistoryPage(logger);

            memberProfilePage.ClickOnMemberDetailTab();

            string currentTierCode = webDriver.GetAttributeTextValue(MemberDetail.MemberCurrentActiveTier, "value");

            Assert.True(currentTierCode.Equals(newTier),
                $"On the Member Detail Page, Expected the new Tier Code for the Member {memberNumber} to be {newTier} but found {currentTierCode} ");

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            tierHistoryPage.RefreshTierHistoryPage();

            webDriver.WaitForElementToBeVisible(TierHistory.ReviseTierButton, 20);

            Dictionary<string, string> activeTierDetails = tierHistoryPage.GetActiveTierDetails();

            Assert.True(activeTierDetails.Any(), "No Active Tier Details Found");

            Assert.True(activeTierDetails["TierCode"].Equals(newTier),
                $"After Clicking on the Manual Tier Change , Expected the Active Tier Code to be {newTier} but found {activeTierDetails["TierCode"]}");

            Assert.True(activeTierDetails["StartDate"].Equals(newTierStartDate),
                $"After Clicking on the Manual Tier Change , Expected the Active Tier StartDate to be {newTierStartDate} but found {activeTierDetails["StartDate"]}");



        }

        [Trait("Category", Traits.TierHistoryUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_ValidationOfErrorMessageWhileMemberTierDowngradeIfDowngradeProhibitFlagIsY()
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC03_ValidationOfErrorMessageWhileMemberTierDowngradeIfDowngradeProhibitFlagIsY");

            #region Test Data

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.DowngradeProhibitReason not in ('',null) and c.Active = @activeFlag order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@activeFlag", "Y");

            MembershipChange membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(paramQuery);

            Assert.True(membershipChangeDoc is not null,
             "Failed to Retrieve MembershipChange Doc for active tier");

            string memberNumber = membershipChangeDoc.MemberNumber;

            string tierCode = membershipChangeDoc.TierCode;

            string issueReason = "Manual Downgrades";

            string newTier = tierCode switch
            {
                "Gold" => "Silver",
                "Silver" => "Red",
                _ => "Red",
            };

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            tierHistoryPage.ClickOnReviseButton();

            tierHistoryPage.SelectNewTier(newTier);

            tierHistoryPage.SelectTierIssueReason(issueReason);

            tierHistoryPage.ClickOnReviseMemberTierButton();

            string actualMessage = tierHistoryPage.GetPopUpMessageContent();

            string expectedMessage = "Tier can not be downgraded. The revision critria are not met.";

            Assert.True(actualMessage.Equals(expectedMessage),
                           $"Expected Message to be {expectedMessage} but found {actualMessage}");

            tierHistoryPage.ClosePopUp();


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

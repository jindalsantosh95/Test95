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
    [Collection("MemberCardLinkTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberCardLinkTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly MemberSearchPage memberSearchPage;
        private readonly IWebDriver webDriver;
        private readonly MemberProfilePage memberProfilePage;
        private readonly FinancialPage financialPage;
        private readonly TierHistoryPage tierHistoryPage;
        private readonly AuditHistoryPage auditHistoryPage;


        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        public string env = Config.Environment;

        public MemberCardLinkTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
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

            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndPointForCustTxnDB, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<CardLinkingAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            financialPage = new FinancialPage(webDriver);

            tierHistoryPage = new TierHistoryPage(webDriver);

            auditHistoryPage = new AuditHistoryPage(webDriver);
        }


        [Trait("Category", Traits.MemberCardLinksUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public void TC01_RetrieveMemberCardLinksSuccessScenario(string partnerStatusCode)
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveMemberCardLinksSuccessScenario");

            #region PreRequisite to retrieve member with AMEX card details 

            string queryForPartnerMembershipDoc = "SELECT top 1 * FROM CustomerTransaction c where c.DocType = @docType and c.PartnerId=@partnerId and c.PartnerStatusCode=@partnerStatusCode order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", partnerStatusCode);

            PartnerMembership partnerMembership = cosmosFixture.CosmosQueryItem<PartnerMembership>(query);

            Assert.True(partnerMembership is not null, "No Partner Membership document found.");

            string customerId = partnerMembership.CustomerId;

            string memberNumber = partnerMembership.MemberNumber;

            // retrieve all partnerMembership docs 

            string queryForAllPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.CustomerId=@customerId and c.PartnerId=@partnerId and c.PartnerStatusCode=@partnerStatusCode order by c.CreatedOn desc ";

            QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", partnerStatusCode);

            IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs);

            PartnerMembership[] partnerMembershipDocuments = partnerMembershipDocs.OrderByDescending(x => x.CreatedOn).ToArray();

            #endregion

            #region - PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            financialPage.ClickOnFinancialTab(logger);

            financialPage.ClickOnAuditAlertPopUpIfPresent();

            string message = webDriver.GetElementText(ElementsLibrary.Financial.SearchResultForVRedCardLinks);

            Assert.True(!message.Equals("No items in Gallery"), $"No Member Card Links are Present for the Flying Club Member Number {memberNumber}");

            string[] rowCount = message.Split(':');

            int count = Int32.Parse(rowCount[1].Trim());

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(ElementsLibrary.Financial.SearchResultMemberCardLinksDataSet));

            Assert.True(count == result.Count(), $"Count of the row items doesnot match withe Card links returned on the screen");

            int i = 0;

            foreach (IWebElement element in result)
            {
                string partnerCode = element.FindElement(By.XPath("div/div[1]/div/div/div/div/div")).Text;

                string cardType = element.FindElement(By.XPath("div/div[3]/div/div/div/div/div")).Text;

                if (partnerCode == "ACENT" && cardType == partnerStatusCode)
                {
                    string partnerName = element.FindElement(By.XPath("div/div[2]/div/div/div/div/div")).Text;

                    string linkedDate = element.FindElement(By.XPath("div/div[4]/div/div/div/div/div")).Text;

                    string endDate = element.FindElement(By.XPath("div/div[5]/div/div/div/div/div")).Text;

                    string anniversaryDate = element.FindElement(By.XPath("div/div[5]/div/div/div/div/div")).Text;

                    Assert.True(partnerMembershipDocuments[i].PartnerStatusCode == cardType, $"Under Partner Membership doc >> for the FC member {memberNumber}, expected status code to be {partnerMembershipDocuments[i].PartnerStatusCode} but was {cardType}");

                    string startDateInCosmos = partnerMembershipDocuments[i].StartDate?.ToString("dd-MMM-yyyy");

                    Assert.True(startDateInCosmos == linkedDate, $"Under Partner Membership doc >> for the FC member {memberNumber}, expected created On  to be {startDateInCosmos} but was {linkedDate}");

                    string endDateInCosmos = partnerMembershipDocuments[i].EndDate?.ToString("dd-MMM-yyyy");

                    endDateInCosmos = endDateInCosmos is null ? String.Empty : endDateInCosmos;

                    Assert.True(endDateInCosmos == endDate, $"Under Partner Membership doc >> for the FC member {memberNumber}, expected created On  to be {endDateInCosmos} but was {endDate}");

                    i++;
                }

            }
            #endregion
        }

        [Trait("Category", Traits.MemberCardLinksUIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public void TC02_AddMemberCardLinksSuccessScenario(string cardType)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC02_AddMemberCardLinksSuccessScenarioForCardType:{cardType}");

            #region PreRequisite to retrieve member with no AMEX card linked to the account 

            string queryForFlyingClubmemberDoc = "SELECT top 10 * FROM CustomerTransaction c where c.DocType = @docType and c.TierCode <> @TierCode order by c.CreatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@TierCode", "Glass");

            IEnumerable<FlyingClubMember> flyingClubDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

            Assert.True(flyingClubDocuments is not null, "No flying club documents found.Cannot proceed with further test execution");

            string customerId = string.Empty;

            string memberNumber = string.Empty;

            string tierCode = string.Empty;

            bool flag = false;

            foreach (FlyingClubMember flyingClubMember in flyingClubDocuments)
            {
                customerId = flyingClubMember.CustomerId;

                memberNumber = flyingClubMember.MemberNumber;

                tierCode = flyingClubMember.TierCode;

                string queryForAllPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode order by c.CreatedOn desc ";

                QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType);

                IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs);

                if (!partnerMembershipDocs.Any())
                {
                    flag = true;
                    break;
                }

            }

            Assert.True(flag, $"The test Data does not meet the test case requirement. Currently we are not able to fetch any test member with no associated AMEX centurion card aassociated with them.");

            #endregion

            #region - PowerApps Validations for the Financial Tab- addding member card details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            financialPage.ClickOnFinancialTab(logger);

            financialPage.ClickOnAuditAlertPopUpIfPresent();

            financialPage.ClickOnAddCard();

            financialPage.SelectCardFromDropDown(cardType);

            string addCardConfirmationMessage = webDriver.GetElementText(Financial.AddMemberCardPopUpMessage);

            string expectedMessage = "Member card added successfully.";

            Assert.True(addCardConfirmationMessage == expectedMessage, $"Failed to add Card {cardType} to {memberNumber}. The Response was {addCardConfirmationMessage}");

            financialPage.ClickOnaddCardConfirmationPopup();

            string message = webDriver.GetElementText(Financial.SearchResultForVRedCardLinks);

            Assert.True(!message.Equals("No items in Gallery"), $"No Member Card Links are Present for the Flying Club Member Number {memberNumber}");

            string[] rowCount = message.Split(':');

            int count = int.Parse(rowCount[1].Trim());

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(Financial.SearchResultMemberCardLinksDataSet));

            Assert.True(count == result.Count(), $"Count of the row items doesnot match with Card links returned on the screen");

            int i = 0;

            foreach (IWebElement element in result)
            {
                string partnerCode = element.FindElement(By.XPath("div/div/div[1]/div/div/div/div")).Text;

                string CardTypeOnPA = element.FindElement(By.XPath("div/div[3]/div/div/div/div/div")).Text;

                if (partnerCode == "ACENT" && CardTypeOnPA == cardType)
                {
                    string partnerName = element.FindElement(By.XPath("div/div[2]/div/div/div/div/div")).Text;

                    string linkedDate = element.FindElement(By.XPath("div/div[4]/div/div/div/div/div")).Text;

                    string endDate = element.FindElement(By.XPath("div/div[5]/div/div/div/div/div")).Text;

                    string anniversaryDate = element.FindElement(By.XPath("div/div[5]/div/div/div/div/div")).Text;

                    Assert.True(partnerName.Equals("Amex Centurion"), $"On PA in Finanacial Tab >> for the FC member {memberNumber}, expected partner name to be Amex Centurion but was {partnerName}");

                    Assert.True(CardTypeOnPA.Equals(cardType), $"On PA in Finanacial Tab >> for the FC member {memberNumber}, expected cardType to be {cardType} but was {CardTypeOnPA}");

                    string currentDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

                    Assert.True(currentDate == linkedDate, $"On PA in Finanacial Tab >> for the FC member {memberNumber}, expected linked date to be {currentDate} but was {linkedDate}");

                    Assert.True(anniversaryDate == string.Empty, $"On PA in Finanacial Tab >> for the FC member {memberNumber}, expected anniversay date to be blank but was {anniversaryDate}");

                    i++;
                }

            }

            financialPage.NavigateToMemberDetailsPage();

            string centurionFlagValue = string.Empty;

            try
            {
                webDriver.WaitForElementToBePresent(MemberDetail.CenturionMemberFlag, 10);

                webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 15);

                centurionFlagValue = webDriver.FindElement(By.XPath(MemberDetail.CenturionMemberFlag)).GetAttribute("checked");
            }
            catch (Exception ex)
            {
                centurionFlagValue = ex.Message;
            }

            Assert.True(centurionFlagValue == "true", $"After Adding the Centurion Card to the Member, Expected the Centurion Flag Checkbox to be checked but found Unchecked for member number :{memberNumber}");

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            Dictionary<string, string> activeTierDetails = tierHistoryPage.GetActiveTierDetails();

            string tierStartDate = DateTime.Now.AddDays(+1).ToString("dd-MMM-yyyy");

            string tierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("dd-MMM-yyyy");

            if (activeTierDetails.Any())
            {
                if (tierCode.IsAny("Red", "Silver"))
                {
                    Assert.True(activeTierDetails["TierCode"].Equals("Gold"), $"Expected the Active Tier to be Gold after Centurion Card linking for the a {tierCode} tier Member - {memberNumber} ");

                    Assert.True(activeTierDetails["StartDate"].Equals(tierStartDate), $"Expected the Start Date of the New Gold tier to be {tierStartDate} after Centurion Card linking for the {activeTierDetails["StartDate"]} after Centurion Card linking for the {tierCode} tier Member - {memberNumber} ");

                    Assert.True(activeTierDetails["EndDate"].Equals(tierEndDate), $"Expected the End Date of the New Gold tier to be {tierEndDate} but found {activeTierDetails["EndDate"]} after Centurion Card linking for the {tierCode} tier Member - {memberNumber} ");
                }
            }

            if (tierCode.IsAny("Red", "Silver"))
            {

                tierHistoryPage.ClickOnMenuBarOnTierHistoryPage(logger);

                auditHistoryPage.ClickOnAuditHistoryTab(logger);

                auditHistoryPage.ClickOnAuditHistoryMemberProfileTab();

                IEnumerable<IWebElement> memberProfileAuditDataSet = webDriver.FindElements(By.XPath(ElementsLibrary.AuditHistory.SearchResultMemberProfileAuditDataSet));

                Assert.True(result.Any(), $"No Member Profile Audit History Found for the Flying Club Member Number {memberNumber}");

                int auditRecordCount = result.Count();

                Assert.True(auditRecordCount > 1, $"Expected the No of Audit Records to be > 1 but found {auditRecordCount}");

                string expectedLastUpdatedBy = "LOOPS";

                DateTime currentDate = DateTime.UtcNow.Date;

                IWebElement element = memberProfileAuditDataSet.First();

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

                memberProfileAuditDataSet.Skip(1); //Skip the latest and take the second latest

                element = memberProfileAuditDataSet.First();

                string prevAuditMemberTitle = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                string prevAuditMemberFirstName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string prevAuditMemberMiddleName = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string prevAuditMemberLastName = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string prevAuditMemberDateOfBirth = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string prevAuditMemberStatus = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                string prevAuditMemberNameChangeReason = element.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

                string prevAuditMemberAuthUser = element.FindElement(By.XPath("td[8]/div/div/span")).GetAttribute("value");

                Assert.True(actualMemberTitle == prevAuditMemberTitle, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the Title to be {prevAuditMemberTitle} but found {actualMemberTitle} ");

                Assert.True(actualMemberFirstName == prevAuditMemberFirstName, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the firstName to be {prevAuditMemberFirstName} but found {actualMemberFirstName} ");

                Assert.True(actualMemberMiddleName == prevAuditMemberMiddleName, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the MiddleName to be {prevAuditMemberMiddleName} but found {actualMemberMiddleName} ");

                Assert.True(actualMemberLastName == prevAuditMemberLastName, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the LastName to be {prevAuditMemberLastName} but found {actualMemberLastName} ");

                Assert.True(actualMemberDateOfBirth == prevAuditMemberDateOfBirth, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the DateOfBirth to be {prevAuditMemberDateOfBirth} but found {actualMemberDateOfBirth} ");

                Assert.True(actualMemberStatus == prevAuditMemberStatus, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the Member Status to be {prevAuditMemberStatus} but found {actualMemberStatus} ");

                Assert.True(actualMemberNameChangeReason == prevAuditMemberNameChangeReason, $"For the Member {memberNumber}, In the Member Profile Audit History, Expected the NameChangeReason to be {prevAuditMemberNameChangeReason} but found {actualMemberNameChangeReason} ");

                Assert.True(actualDowngradeProhibitFlag == "Y", $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the DowngradeProhibitFlag to be Y but found {actualDowngradeProhibitFlag}");

                Assert.True(actualMemberAuthUser == prevAuditMemberAuthUser, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the Auth User to be {prevAuditMemberAuthUser} but found {actualMemberAuthUser} ");

                Assert.True(actualMemberLastUpdatedBy == expectedLastUpdatedBy, $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the LastUpdatedBy  to be {prevAuditMemberNameChangeReason} but found {expectedLastUpdatedBy} ");

                actualMemberLastUpdatedOn = actualMemberLastUpdatedOn.IsNullOrEmpty() ? "" : DateTime.Parse(actualMemberLastUpdatedOn).ToString("dd-MMM-yyyy");

                Assert.True(actualMemberLastUpdatedOn == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber}, In the Member Profile Audit History,  Expected the LastUpdatedOn to be {currentDate.ToString("dd-MMM-yyyy")} but found {actualMemberLastUpdatedOn} ");
            }

            #endregion

        }

        [Trait("Category", "CardLinkingAuditRegression")]
        [Theory, RunOrder(3)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public void TC03_CardLinkingAuditSuccessScenario(string partnerStatusCode)
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC03_CardLinkingAuditSuccessScenario");

            #region PreRequisite to retrieve member with AMEX card details 

            string queryForPartnerMembershipDoc = "SELECT top 1 * FROM CustomerTransaction c where c.DocType = @docType and c.PartnerId=@partnerId and c.PartnerStatusCode=@partnerStatusCode order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", partnerStatusCode);

            PartnerMembership partnerMembership = cosmosFixture.CosmosQueryItem<PartnerMembership>(query);

            Assert.True(partnerMembership is not null, "No Partner Membership document found with valid Card Link Data.Test Data Criteria Didn't met");

            string customerId = partnerMembership.CustomerId;

            string memberNumber = partnerMembership.MemberNumber;

            string queryForAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c.CreatedOn desc";

            QueryDefinition auditDocQuery = new QueryDefinition(query: queryForAuditDocs).WithParameter("@docType", "CardLinkingAudit").WithParameter("@partitionKey", customerId);

            IEnumerable<CardLinkingAudit> cardAuditDocs = cosmosFixture.CosmosQueryItems<CardLinkingAudit>(auditDocQuery);

            #endregion

            #region PowerApps Validations for the Audit History Profile details

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            financialPage.ClickOnFinancialTab(logger);

            financialPage.ClickOnAuditAlertPopUpIfPresent();

            IEnumerable<IWebElement> listOfCards = webDriver.FindElements(Financial.ListOfCardLinks, "List of Cards Table Data");

            foreach (IWebElement card in listOfCards)
            {
                string cardType = card.FindElement(By.XPath(Financial.CardType)).Text;

                IEnumerable<CardLinkingAudit> cardLinkingAudits = cardAuditDocs.Where(x => x.StatusEnhancementCode == cardType).OrderByDescending(x => x.CreatedOn);

                if (cardLinkingAudits.Any() && cardType.Equals(partnerStatusCode))
                {
                    webDriver.WaitForElementToBeVisible($"{Financial.AuditHistoryIcon}/div/div/div", 5);

                    card.FindElement(By.XPath($"{Financial.AuditHistoryIcon}/div/div/div")).Click();

                    webDriver.WaitForElementToBeVisible(Financial.CardAuditHistoryWindow, 5);

                    IEnumerable<IWebElement> cardAuditData = webDriver.FindElements(Financial.SearchResultCardAuditDataSet, "Audit History Data,", 10);

                    Assert.True(cardLinkingAudits.Count() == cardAuditData.Count(), $"For the Member {memberNumber} ,the Audit Docs count does not match for the Card Type {cardType}. Expected {cardLinkingAudits.Count()} but found {cardAuditData.Count()}");

                    int i = 0;

                    foreach (IWebElement auditCardLinkRecord in cardAuditData)
                    {
                        string actualEndDateValue = auditCardLinkRecord.FindElement(By.XPath(Financial.AuditEndDate)).Text;

                        string actualUpdatedByValue = auditCardLinkRecord.FindElement(By.XPath(Financial.AuditUpdatedBy)).Text;

                        string actualUpdatedOnValue = auditCardLinkRecord.FindElement(By.XPath(Financial.AuditUpdatedOn)).Text;

                        actualEndDateValue = !actualEndDateValue.IsNullOrWhiteSpace() ? actualEndDateValue : "";

                        actualUpdatedByValue = actualUpdatedByValue.IsNullOrWhiteSpace() ? "" : actualUpdatedByValue;

                        actualUpdatedOnValue = !actualUpdatedOnValue.IsNullOrWhiteSpace() ? actualUpdatedOnValue.Split(" ")[0].Trim() : "";

                        string expectedEndDate = cardLinkingAudits.ElementAt(i).EndDate.HasValue ? cardLinkingAudits.ElementAt(i).EndDate.Value.ToString("dd-MMM-yyyy") : "";

                        string expectedUpdatedBy = cardLinkingAudits.ElementAt(i).CardLinkingUpdatedBy;

                        string expectedUpdatedOnDate = cardLinkingAudits.ElementAt(i).CardLinkingUpdatedOn.HasValue ? cardLinkingAudits.ElementAt(i).CardLinkingUpdatedOn.Value.ToString("dd-MMM-yyyy") : "";

                        Assert.True(actualEndDateValue == expectedEndDate, $"EndDate does not match for the audit card type {cardType} for doc Id {cardLinkingAudits.ElementAt(i).id}");

                        Assert.True(actualUpdatedOnValue == expectedUpdatedOnDate, $"UpdatedOn Date does not match for the audit card type {cardType} for doc Id {cardLinkingAudits.ElementAt(i).id}");

                        Assert.True(actualUpdatedByValue == expectedUpdatedBy, $"UpdatedBy does not match for the audit card type {cardType} for doc Id {cardLinkingAudits.ElementAt(i).id}");

                        i++;
                    }

                    financialPage.CloseAuditHistoryWindow();
                }

                else if (!cardLinkingAudits.Any())
                {
                    string borderColor = webDriver.GetAttributeTextValue(Financial.AuditHistoryIcon, "border-color");

                    Assert.True(borderColor.Is("rgb(166, 166, 166)"), "Expected the Audit History Icon to be disabled. Found Enabled/Clickable when Card has no audit History data");
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

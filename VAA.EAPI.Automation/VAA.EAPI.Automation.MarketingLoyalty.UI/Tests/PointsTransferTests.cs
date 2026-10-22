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
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("PointsTransferTests")]
    public class PointsTransferTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly IWebDriver webDriver;
        private readonly MemberSearchPage memberSearchPage;
        private readonly PointsTransferPage pointsTransferPage;
        PreRequisiteTestData preRequisiteTestData;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public PointsTransferTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            memberSearchPage = new MemberSearchPage(webDriver);
            pointsTransferPage = new PointsTransferPage(webDriver);
            preRequisiteTestData = new();

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);


        }

        [Trait("Category", Traits.PointsTransferUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_PointsTransferSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_PointsTransferSuccessScenario");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode and c.ProductId in ('1-1ZWY5ZT','1-1Z9D') order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Sender Account.");

            string senderAccountFcNumber = txnDoc.MemberNumber;

            string senderAccountCustomerId = txnDoc.CustomerId;

            string queryForReceipientFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.Status = @status and c.PartitionKey <> @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForReceipientFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active").WithParameter("@customerId", senderAccountCustomerId);

            FlyingClubMember recipientFCMDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(recipientFCMDoc is not null,
             "Failed to Retrieve Test Data for Recipient Account.");

            string recipientAccountFcNumber = recipientFCMDoc.MemberNumber;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, senderAccountFcNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string memberPointBalance = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            Assert.True(!memberPointBalance.Equals("0"),
                "The Member Point Balance is 0 .Points Transfer Transaction cannot be created");

            pointsTransferPage.NavigateToPointsTransferPage(logger);

            string points = DataGenerator.RandomNumber(1, Convert.ToInt32(memberPointBalance), true);

            pointsTransferPage.EnterRecipientMemberNumber(recipientAccountFcNumber);

            pointsTransferPage.ClickOnSearchRecipientAccountDetails();

            string recipientFirstName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientFirstNameTxtBox, "value");

            string recipientLastName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientSurNameTxtBox, "value");

            Assert.True(!string.IsNullOrWhiteSpace(recipientFirstName),
               "The Recipient FirstName is found Null");

            Assert.True(!string.IsNullOrWhiteSpace(recipientLastName),
              "The Recipient SurName is found Null");

            pointsTransferPage.EnterNoOfPointsToTransfer(points);

            pointsTransferPage.ClickOnApplyPointsTransferButton();

            string text = pointsTransferPage.GetSuccessPopUPMessage();

            string expectedMessage = "Success - Points Transfered";

            Assert.True(text.Equals(expectedMessage),
               $"After Applying the Point Transfer ,Expected the Message to be {expectedMessage} but found {text}.");

            string actualPointsTransfered = webDriver.GetElementText(PointsTransfer.SuccessPopUPPointsDetails);

            Assert.True(points.Equals(actualPointsTransfered),
               $"After Applying the Point Transfer ,On the Success Pop Up Expected the points to be {points} but found {actualPointsTransfered}.");

            pointsTransferPage.CloseSuccessPopUp();
        }

        [Trait("Category", Traits.PointsTransferUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_ValidateApplyPointsTransferButtonWhenRecipientFCNumberIsNotValid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_ValidateApplyPointsTransferButtonWhenRecipientFCNumberIsNotValid");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode and c.ProductId in ('1-1ZWY5ZT','1-1Z9D') order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null,
              "Failed to Retrieve Test Data for Sender Account.");

            string senderAccountFcNumber = txnDoc.MemberNumber;

            string recipientAccountFcNumber = "1000965423";

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, senderAccountFcNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string memberPointBalance = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            Assert.True(!memberPointBalance.Equals("0"),
                "The Member Point Balance is 0 .Points Transfer Transaction cannot be created");

            pointsTransferPage.NavigateToPointsTransferPage(logger);

            string points = DataGenerator.RandomNumber(1, Convert.ToInt32(memberPointBalance), true);

            pointsTransferPage.EnterRecipientMemberNumber(recipientAccountFcNumber);

            pointsTransferPage.ClickOnSearchRecipientAccountDetails();

            string recipientFirstName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientFirstNameTxtBox, "value", 10);

            string recipientLastName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientSurNameTxtBox, "value", 10);

            Assert.True(string.IsNullOrWhiteSpace(recipientFirstName),
               $"Expected the Recipient FirstName to be Null or Empty String but found {recipientFirstName}");

            Assert.True(string.IsNullOrWhiteSpace(recipientLastName),
              $"Expected the Recipient LastName to be Null or Empty String but found {recipientLastName}");

            pointsTransferPage.EnterNoOfPointsToTransfer(points);

            Assert.True(pointsTransferPage.IsApplyPointsTransferButtonDisabled(),
                "Expected the Apply Button to be in disabled state when Recipient Member is not valid but found Enabled");

        }

        [Trait("Category", Traits.PointsTransferUIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_ValidateApplyPointsTransferButtonWhenSenderAccountPointBalanceIsZero()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("ValidateApplyPointsTransferButtonWhenSenderAccountPointBalanceIsZero");

            #region PreRequisite to create an enrolment before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "POWERAPP";
            string enroledBy = DataGenerator.RandomFirstName();

            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");

            (string memberNumber, string customerId) = await preRequisiteTestData.MemberEnrolment(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null,
              "Failed to Retrieve Test Data for Sender Account.");

            string senderAccountFcNumber = flyingClubMemberDoc.MemberNumber;

            string senderAccountCustomerId = flyingClubMemberDoc.CustomerId;

            string queryForReceipientFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode and c.Status = @status and c.PartitionKey <> @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForReceipientFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active").WithParameter("@customerId", senderAccountCustomerId);

            FlyingClubMember recipientFCMDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(recipientFCMDoc is not null,
             "Failed to Retrieve Test Data for Recipient Account.");

            string recipientAccountFcNumber = recipientFCMDoc.MemberNumber;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, senderAccountFcNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string memberPointBalance = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            Assert.True(memberPointBalance.Equals("0"), $"This Test Case needs an Account with 0 Point Balance but found {memberPointBalance}.");

            pointsTransferPage.NavigateToPointsTransferPage(logger);

            string points = DataGenerator.RandomNumber(1, 100, true);

            pointsTransferPage.EnterRecipientMemberNumber(recipientAccountFcNumber);

            pointsTransferPage.ClickOnSearchRecipientAccountDetails();

            string recipientFirstName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientFirstNameTxtBox, "value", 10);

            string recipientLastName = webDriver.GetAttributeTextValue(PointsTransfer.RecipientSurNameTxtBox, "value", 10);

            Assert.True(!string.IsNullOrWhiteSpace(recipientFirstName),
               "The Recipient FirstName is found Null");

            Assert.True(!string.IsNullOrWhiteSpace(recipientLastName),
              "The Recipient SurName is found Null");

            pointsTransferPage.EnterNoOfPointsToTransfer(points);

            Assert.True(pointsTransferPage.IsApplyPointsTransferButtonDisabled(),
               "Expected the Apply Button to be in disabled state when Recipient Member balance is 0 but found Enabled");

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

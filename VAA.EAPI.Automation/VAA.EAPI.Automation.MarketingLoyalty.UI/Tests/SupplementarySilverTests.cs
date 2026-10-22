using AventStack.ExtentReports;
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
using Customer = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{

    [Collection("SupplementarySilverTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class SupplementarySilverTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {

        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private MemberSearchPage memberSearchPage;
        private SupplementarySilverPage supplementarySilverPage;
        private TierHistoryPage tierHistoryPage;
        private MemberProfilePage memberProfilePage;
        private IWebDriver webDriver;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public SupplementarySilverTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            supplementarySilverPage = new SupplementarySilverPage(webDriver);

            tierHistoryPage = new TierHistoryPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_ValidateThatLinkAndDelinkButtonAreDisabledForRedAndSilverMembersIfThereIsNoLinkedGoldAccount()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_ValidateThatLinkAndDelinkButtonAreDisabledForRedAndSilverMembersIfThereIsNoLinkedGoldAccount");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.TierCode not in ('Gold','Glass') and c.SSAMemberNumber='' order by c.CreatedOn desc ";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            Assert.True(supplementarySilverPage.IsLinkButtonDisabled(),
                "Expected the Link button to be disabled but found enabled.");

            Assert.True(supplementarySilverPage.IsDeLinkButtonDisabled(),
                "Expected the DeLink button to be disabled but found enabled.");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_ValidateThatLinkButtonIsDisabledAndDelinkButtonIsEnabledForSuppSilverMemberIfItHasALinkedGoldAccount()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC02_ValidateThatLinkButtonIsDisabledAndDelinkButtonIsEnabledForSuppSilverMemberIfItIsLinkedGoldAccount");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber!='' order by c.CreatedOn desc ";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnRefreshButton();

            DateTime associationDate;

            bool isValidDate = DateTime.TryParse(supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberAssociationDate), out associationDate);

            DateTime currentDate = DateTime.UtcNow.Date;

            int associationPeriod = isValidDate ? Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays) : 0;

            Assert.True(supplementarySilverPage.IsLinkButtonDisabled(),
                "Expected the Link button to be disabled but found enabled.");

            supplementarySilverPage.ClickOnDeLinkButton(logger);

            string expectedMessage = "The account should be linked for a minimum of 12 months. Please select the override box and delink the account if it was linked in error.";

            if (associationPeriod < 365)
            {
                Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"If association period is < 365 days , On Delink button Click ,Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");
            }
            else
            {
                Assert.True(webDriver.TryFindElement(SupplementarySilver.DelinkConfirmYesButton) is not null,
                    "If association period is > 365 days , On Delink button Click ,Expected the Yes Button to proceed for delinking");
            }

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_ValidateThatLinkButtonIsEnabledAndDelinkButtonIsDisabledForGoldMembersIfNoSuppSilverIsLinked()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_ValidateThatLinkButtonIsEnabledAndDelinkButtonIsDisabledForGoldMembersIfNoSuppSilverIsLinked");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? memberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion            

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            Assert.False(supplementarySilverPage.IsLinkButtonDisabled(),
                "Expected the Link button to be enabled but found disabled.");

            Assert.True(supplementarySilverPage.IsDeLinkButtonDisabled(),
                "Expected the DeLink button to be disabled but found enabled.");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_ValidateThatLinkButtonIsDisabledAndDelinkButtonIsEnabledForGoldMembersIfThereIsASuppSilverAccountLinkedToIt()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC04_ValidateThatLinkButtonIsDisabledAndDelinkButtonIsEnabledForGoldMembersIfThereIsASuppSilverAccountLinkedToIt");

            #region Fetching a Supplementary Silver Account

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string childMemberNumber = string.Empty;

            string? parentMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {
                childMemberNumber = x.MemberNumber;

                string queryForParentFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.MemberNumber='{x.SSAMemberNumber}'";

                IEnumerable<FlyingClubMember> parentFcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

                return parentFcmDoc.Any(c => c.TierCode.Equals("Gold"));

            })?.SSAMemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Parent Or Gold Member");

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Expected childMemberNumber to have a value but found empty or null.");

            #endregion            

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            DateTime associationDate;

            bool isValidDate = DateTime.TryParse(supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberAssociationDate), out associationDate);

            DateTime currentDate = DateTime.UtcNow.Date;

            int associationPeriod = isValidDate ? Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays) : 0;

            Assert.True(supplementarySilverPage.IsLinkButtonDisabled(),
               "Expected the Link button to be disabled but found enabled.");

            supplementarySilverPage.ClickOnDeLinkButton(logger);

            string expectedMessage = "The account should be linked for a minimum of 12 months. Please select the override box and delink the account if it was linked in error.";

            if (associationPeriod < 365)
            {
                Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"If association period is < 365 days , On Delink button Click ,Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");
            }
            else
            {
                Assert.True(webDriver.TryFindElement(SupplementarySilver.DelinkConfirmYesButton) is not null,
                    "If association period is > 365 days , On Delink button Click ,Expected the Yes Button to proceed for delinking");
            }

        }


        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_ValidateTheSuppSilverHistoryDisplayedMessageForGoldMembersIfNoSuppSilverIsLinked()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC05_ValidateTheSuppSilverHistoryDisplayedMessageForGoldMembersIfNoSuppSilverIsLinked");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? memberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion  

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            string expectedMessage = "No relationship to display";

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_ErrorMessageValidationOnClickingLinkButtonForGoldMemberIfMemberStatusIsNotActive()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC06_ErrorMessageValidationOnClickingLinkButtonForGoldMemberIfMemberStatusIsNotActive");

            #region Fetching a Gold FC Member whose status is not active

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status not in ('Active','') and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion            

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            string expectedMessage = "The account does not have a valid status";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(7)]
        public async Task TC07_ErrorMessageValidationOnLinkPopUpScreenWhenParentAccountEmailAddressIsInvalidAsync()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC07_ErrorMessageValidationOnLinkPopUpScreenWhenParentAccountEmailAddressIsInvalid");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string customerId = string.Empty;

            string memberNumber = flyingClubMemberDocs.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                customerId = x.CustomerId;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{customerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(memberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents for furthr test execution");

            #endregion

            #region Pre-Requisite for updating the emailAddress Valid Flag to  'N'

            string queryForCustomerDocument = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{customerId}'";

            Customer customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            int retry = 5;

            while (retry > 0 && customerDocument == null)
            {
                Thread.Sleep(500);

                customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

                retry--;
            }

            Assert.True(customerDocument is not null,
                $"No Document is available for the searched Query {queryForCustomerDocument}.");

            IEnumerable<ContactDetails> contact = customerDocument.ContactDetails.Where(c => c.ContactType == "Email");

            foreach (ContactDetails contactItem in contact)
            {
                if (contactItem.Primary != "Y")
                {
                    contactItem.Primary = "Y";
                }
                contactItem.Valid = "N";
            }

            Customer scvCustomerDoc = customerDocument;

            string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, null);

            Assert.True(updatestatus.Equals("OK"),
                $"EmailAddress Valid Flag for the member {memberNumber} was not updated to N in customer collection DB");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            string expectedMessage = "Unable to complete the request as the current account has an invalid email address";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");

            #region  Updating EmailAddress Valid Flag back to Y

            contact = scvCustomerDoc.ContactDetails.Where(c => c.ContactType == "Email" && c.Valid == "N");

            foreach (ContactDetails contactItem in contact)
            {
                contactItem.Valid = "Y";
            }

            updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, null);

            Assert.True(updatestatus.Equals("OK"),
                $"EmailAddress Valid Flag for the member {memberNumber} was not updated to Y in customer collection DB");

            #endregion

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(8)]
        public void TC08_ErrorMessageValidationOnValidateAndLinkPopUpScreenWhenEnteredChildFCAccountStatusIsNotValid()
        {
            // Valid Status {'Active','Active Pending', 'Inactive' ,'Frozen', 'Expired'}

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_ErrorMessageValidationOnLinkPopUpScreenWhenEnteredChildFCAccountStatusIsNotValid");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion  

            #region Fetching Child Member whose Status is Not Active

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Gold','Glass') and c.Status not in ('Active','ActivePending','Active Pending','Inactive','Expired','Frozen','') and c.SSAMemberNumber='' order by c.CreatedOn desc";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string? childMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                Customer customerDoc = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc);

                return customerDoc.ContactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");
            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, childMemberNumber);

            supplementarySilverPage.ClickOnValidateAndLinkButton(logger);

            string expectedMessage = "The account does not have a valid status";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(9)]
        public async Task TC09_ErrorMessageValidationOnLinkPopUpScreenWhenChildAccountEmailAddressIsInvalidAsync()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC09_ErrorMessageValidationOnLinkPopUpScreenWhenChildAccountEmailAddressIsInvalid");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion  

            #region Fetching a Child FC Member

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode in ('Red','Silver') and c.Status in ('Active','Active Pending','Inactive','Frozen','Expired') and c.SSAMemberNumber='' order by c.CreatedOn desc ";

            FlyingClubMember childFcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            string childMemberNumber = childFcmDoc.MemberNumber;

            #endregion

            #region Pre-Requisite for updating the Child FC EmailAddress Valid Flag to  'N'

            string queryForCustomerDocument = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{childFcmDoc.CustomerId}'";

            Customer customerDocument = cosmosFixture.QueryItem<Customer>(queryForCustomerDocument);

            Assert.True(customerDocument is not null,
                $"No Document is available for the searched Query {queryForCustomerDocument}.");

            IEnumerable<ContactDetails> emailContact = customerDocument.ContactDetails.Where(c => c.ContactType == "Email");

            foreach (ContactDetails contactItem in emailContact)
            {
                if (contactItem.Primary != "Y")
                {
                    contactItem.Primary = "Y";
                }

                contactItem.Valid = "N";
            }

            Customer scvCustomerDoc = customerDocument;

            string updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, null);

            Assert.True(updatestatus.Equals("OK"),
                $"EmailAddress Valid Flag for the member {childFcmDoc.MemberNumber} was not updated to N in customer collection DB");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, childMemberNumber);

            supplementarySilverPage.ClickOnValidateAndLinkButton(logger);

            string expectedMessage = "Unable to complete the request as the searched account has an invalid email address";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");


            #region  Updating EmailAddress Valid Flag back to Y

            emailContact = scvCustomerDoc.ContactDetails.Where(c => c.ContactType == "Email" && c.Valid == "N");

            foreach (ContactDetails contactItem in emailContact)
            {
                contactItem.Valid = "Y";
            }

            updatestatus = await UpdateCosmosAttribute.UpdateAttributeInCustomerCollectionDB(scvCustomerDoc, null);

            Assert.True(updatestatus.Equals("OK"),
                $"EmailAddress Valid Flag for the member {childFcmDoc.MemberNumber} was not updated to Y in customer collection DB");

            #endregion
        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(10)]
        public void TC10_ErrorMessageValidationWhenTheSearchedChildFCNumberIsInvalid()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC10_ErrorMessageValidationWhenTheSearchedChildFCNumberIsInvalid");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion  

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, "1010016513");

            string expectedMessage = "Invalid flying club member number";

            Assert.True(supplementarySilverPage.GetTextFromChildFCSearchPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromChildFCSearchPopUpScreen()}");


        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(11)]
        public void TC11_ErrorMessageValidationWhenTheSearchedChildFCNumberAndTheParentMemberNumberIsSame()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC11_ErrorMessageValidationWhenTheSearchedChildFCNumberAndTheParentMemberNumberIsSame");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Gold FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion  

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, parentMemberNumber);

            supplementarySilverPage.ClickOnValidateAndLinkButton(logger);

            string expectedMessage = "The parent member number and the linked member number must be different";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");


        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(12)]
        public void TC12_ErrorMessageValidationWhenTheSearchedChildIsAlreadyLinkedToAGoldAccount()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC12_ErrorMessageValidationWhenTheSearchedChildIsAlreadyLinkedToAGoldAccount");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion

            #region Fetching Child Member details which is already linked to a Gold Account

            string queryForChildFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember'  and c.Status!='' and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            FlyingClubMember childFcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForChildFcmDoc);

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, childFcmDoc.MemberNumber);

            string expectedMessage = "The selected member is already linked to another Gold member";

            Assert.True(supplementarySilverPage.GetTextFromChildFCSearchPopUpScreen().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromChildFCSearchPopUpScreen()}");


        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(13)]
        public void TC13_ErrorMessageValidationWhenTheSearchedChildIsAGoldTierMember()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC13_ErrorMessageValidationWhenTheSearchedChildIsAGoldTierMember");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion

            #region Fetching Child Member details (Gold)

            string queryForChildFcmDoc = $"SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' and c.MemberNumber!='{parentMemberNumber}' order by c.CreatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string? childMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Didn't find Valid Child FC Member whose Email Valid flag is Y from the retrieved FCM Documents");


            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, childMemberNumber);

            supplementarySilverPage.ClickOnValidateAndLinkButton(logger);

            string expectedMessage = "The member tier should be either Red or Silver";

            Assert.True(supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen().Equals(expectedMessage),
                 $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen()}");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(14)]
        public void TC14_SupplementarySilver_Linking_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC14_SupplementarySilver_Linking_SuccessScenario");

            #region Fetching a Valid Gold FC Member

            string queryForParentFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode='Gold' and c.Status='Active' and c.SSAMemberNumber='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

            string? parentMemberNumber = flyingClubMemberDoc.FirstOrDefault(x =>
            {
                string queryForFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber='{x.MemberNumber}'";

                FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

                if (flyingClubMemberDoc is not null)
                    return false;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion

            #region Fetching a Valid Child FC Member

            string queryForFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode in ('Red','Silver') and c.Status in ('Active','Inactive','Frozen','Expired','Active Pending','ActivePending') and c.SSAMemberNumber='' order by c.CreatedOn desc ";

            IEnumerable<FlyingClubMember> fcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFcmDoc);

            string childStatus = string.Empty;

            string childTierCode = string.Empty;

            string? childMemberNumber = fcmDoc.FirstOrDefault(x =>
            {
                childTierCode = x.TierCode;

                childStatus = x.Status;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{x.CustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            Assert.True(!string.IsNullOrEmpty(childStatus),
               "Expected childStatus to have a value but found null or empty value");

            Assert.True(!string.IsNullOrEmpty(childTierCode),
               "Expected child TierCode to have a value but found null or empty value");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnLinkButton(logger);

            supplementarySilverPage.EnterChildFCNumberAndSearch(logger, childMemberNumber);

            supplementarySilverPage.ClickOnValidateAndLinkButton(logger);

            #region Validating the data after linking on Parent Account

            string expectedMessage = "Supplementary silver linking successful.";

            string actualMessage = supplementarySilverPage.GetTextFromLinkAndDelinkButtonPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected Message was {expectedMessage} but found {actualMessage}.");

            supplementarySilverPage.CloseThePopUp(logger);

            string currentDate = DateTime.UtcNow.Date.ToString("dd-MMM-yyyy");

            string linkedMemberNumber = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberNumber);

            string linkedAccountStatus = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberStatus);

            string linkedAccountRelationshipStatus = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberRelationShipType);

            string linkedAccountAssociationDate = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberAssociationDate);

            Assert.True(linkedMemberNumber.Equals(childMemberNumber),
                $"On Parent Account expected the linked Member Number to be {childMemberNumber} but found {linkedMemberNumber}.");

            Assert.True(linkedAccountStatus.Equals(childStatus),
                $"On Parent Account expected the linked Member Number to be {childStatus} but found {linkedAccountStatus}.");

            Assert.True(linkedAccountRelationshipStatus.Equals("Supp Silver"),
                $"On Parent Account expected the linked Member RelationShip Type to be Supp Silver but found {linkedAccountRelationshipStatus}.");

            Assert.True(linkedAccountAssociationDate.Equals(currentDate),
               $"On Parent Account expected the linked Member AssociationDate to be {currentDate} but found {linkedAccountAssociationDate}.");

            Assert.True(supplementarySilverPage.IsLinkButtonDisabled(),
                "On Parent Account expected the Link Button to be in disable state after a successful linking of child account");

            #endregion

            #region Validation of Linking Record in the Audit Section of Parent Account

            supplementarySilverPage.ClickOnRefreshButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            string memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber);

            string activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType);

            string relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType);

            string updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy);

            string updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn);

            Assert.True(memberNumberInAuditSection.Equals(childMemberNumber),
               "After Successful linking , In the SSA Audit Section of the Parent Account," +
               $"Expected the  Member Number to be {childMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Linking"),
                 "After Successful linking , In the SSA Audit Section of the Parent Account," +
                $"Expected the ActivityType to be Linking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Supp Silver"),
                 "After Successful linking , In the SSA Audit Section of the Parent Account," +
                $"Expected the RelationshipType to be Supp Silver but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful linking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy");

            Assert.True(updatedOnInAuditSection.Equals(currentDate),
                 "After Successful linking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedOn value to {currentDate} but found {updatedOnInAuditSection}.");


            #endregion

            memberSearchPage.ClickOnPowerAppsHomeButton(logger);

            memberSearchPage.ClearTheSearchParametersAndResults(logger);

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, childMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            tierHistoryPage.NavigateToTierHistoryTab(logger);

            #region Validating the Tier Details of child account after successful linking

            string tierStartDate = DateTime.Now.AddDays(+1).ToString("dd-MMM-yyyy");

            string tierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("dd-MMM-yyyy");

            Dictionary<string, string> activeTierDetails = tierHistoryPage.GetActiveTierDetails();

            if (childTierCode.Equals("Red"))
            {
                Assert.True(activeTierDetails["TierCode"].Equals("Silver"),
                    "Expected the Active Tier to be Silver after supp silver linking for the Red Member");

                Assert.True(activeTierDetails["StartDate"].Equals(tierStartDate),
                   $"Expected the Start Date of the New Silver tier to be {tierStartDate}" +
                   $" but found {activeTierDetails["StartDate"]} after supp silver linking for the Red Member");

                Assert.True(activeTierDetails["EndDate"].Equals(tierEndDate),
                   $"Expected the End Date of the New Silver tier to be {tierEndDate}" +
                   $" but found {activeTierDetails["EndDate"]} after supp silver linking for the Red Member");

            }

            #endregion

            tierHistoryPage.ClickOnMenuBarOnTierHistoryPage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            supplementarySilverPage.ClickOnRefreshButton();

            #region Validating the Parent Details on Supplementary Page from Child Account

            linkedMemberNumber = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberNumber);

            linkedAccountStatus = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberStatus);

            linkedAccountRelationshipStatus = supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberRelationShipType);

            Assert.True(linkedMemberNumber.Equals(parentMemberNumber),
                $"On Child Account expected the linked Member Number to be {parentMemberNumber} but found {linkedMemberNumber}.");

            Assert.True(linkedAccountStatus.Equals("Active"),
                $"On Child Account expected the linked Member Number to be Active but found {linkedAccountStatus}.");

            Assert.True(linkedAccountRelationshipStatus.Equals("Linked Gold"),
                $"On Child Account expected the linked Member RelationShip Type to be Linked Gold but found {linkedAccountRelationshipStatus}.");

            Assert.True(supplementarySilverPage.IsLinkButtonDisabled(),
                "On Child Account expected the Link Button to be in disable state after a successful linking of child account");

            #endregion

            #region Validation of Linking Record in the Audit Section of Child Account

            memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber) ?? "";

            activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType) ?? "";

            relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType) ?? "";

            updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy) ?? "";

            updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn) ?? "";

            Assert.True(memberNumberInAuditSection.Equals(parentMemberNumber),
               "After Successful linking , In the SSA Audit Section of the Child Account," +
               $"Expected the  Member Number to be {parentMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Linking"),
                 "After Successful linking , In the SSA Audit Section of the Child Account," +
                $"Expected the ActivityType to be Linking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Linked Gold"),
                 "After Successful linking , In the SSA Audit Section of the Child Account," +
                $"Expected the RelationshipType to be Linked Gold but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful linking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy") ?? "";

            Assert.True(updatedOnInAuditSection.Equals(currentDate),
                 "After Successful linking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedOn value to {currentDate} but found {updatedOnInAuditSection}.");

            #endregion
        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(15)]
        public void TC15_SupplementarySilver_Delinking_FromParentAccount_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC15_SupplementarySilver_Delinking_FromParentAccount_SuccessScenario");

            #region Fetching a Supplementary Silver Account

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string childMemberNumber = string.Empty;

            string? parentMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {
                childMemberNumber = x.MemberNumber;

                string queryForParentFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.MemberNumber='{x.SSAMemberNumber}'";

                IEnumerable<FlyingClubMember> parentFcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

                return parentFcmDoc.Any(c => c.TierCode.Equals("Gold"));

            })?.SSAMemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Parent Or Gold Member for Delinking");

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Expected childMemberNumber to have a value but found empty or null.");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            DateTime associationDate = DateTime.Parse(supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberAssociationDate));

            DateTime currentDate = DateTime.UtcNow.Date;

            int associationPeriod = Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays);

            if (associationPeriod < 365)
            {
                supplementarySilverPage.ClickOnOverrideCheckbox();
            }

            supplementarySilverPage.ClickOnDeLinkButton(logger);

            supplementarySilverPage.ClickOnDeLinkYesButton(logger);

            string message = "Supplementary silver delinking successful";

            Assert.True(supplementarySilverPage.WaitForTextToBeDisplayedAfterSuccessfulDelinking(message),
                "Expected the Delinking Success Message to be displayed after delinking.");

            supplementarySilverPage.CloseThePopUp(logger);

            string expectedMessage = "No relationship to display";

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(!supplementarySilverPage.IsLinkButtonDisabled(),
                "On Parent Account expected the Link Button to be in Enabled state after a successful DeLinking of child account");

            #region Audit data validation after delinking from Parent Account

            supplementarySilverPage.ClickOnRefreshButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            string memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber);

            string activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType);

            string relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType);

            string updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy);

            string updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn);

            Assert.True(memberNumberInAuditSection.Equals(childMemberNumber),
               "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
               $"Expected the  Member Number to be {childMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the ActivityType to be Delinking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Supp Silver"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the RelationshipType to be Supp Silver but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy");

            Assert.True(updatedOnInAuditSection.Equals(currentDate.ToString("dd-MMM-yyyy")),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedOn value to {currentDate.ToString("dd-MMM-yyyy")} but found {updatedOnInAuditSection}.");

            #endregion

            memberSearchPage.ClickOnPowerAppsHomeButton(logger);

            memberSearchPage.ClearTheSearchParametersAndResults(logger);

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, childMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(supplementarySilverPage.IsDeLinkButtonDisabled(),
                "On Child Account expected the DeLink Button to be in disabled state after a successful DeLinking from parent account");

            #region Validation of Linking Record in the Audit Section of Child Account

            memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber) ?? "";

            activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType) ?? "";

            relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType) ?? "";

            updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy) ?? "";

            updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn) ?? "";

            Assert.True(memberNumberInAuditSection.Equals(parentMemberNumber),
               "After Successful Delinking , In the SSA Audit Section of the Child Account," +
               $"Expected the  Member Number to be {parentMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the ActivityType to be Linking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Linked Gold"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the RelationshipType to be Linked Gold but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy") ?? "";

            Assert.True(updatedOnInAuditSection.Equals(currentDate.ToString("dd-MMM-yyyy")),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedOn value to {currentDate.ToString("dd-MMM-yyyy")} but found {updatedOnInAuditSection}.");

            #endregion

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(16)]
        public void TC16_SupplementarySilver_Delinking_FromChildAccount_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC16_SupplementarySilver_Delinking_FromChildAccount_SuccessScenario");

            #region Fetching a Supplementary Silver Account

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!='' and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string childMemberNumber = string.Empty;

            string? parentMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {
                childMemberNumber = x.MemberNumber;

                string queryForParentFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!=''  and c.MemberNumber='{x.SSAMemberNumber}'";

                IEnumerable<FlyingClubMember> parentFcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

                return parentFcmDoc.Any(c => c.TierCode.Equals("Gold"));

            })?.SSAMemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Parent Or Gold Member for Delinking");

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Expected childMemberNumber to have a value but found empty or null.");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, childMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            DateTime associationDate = DateTime.Parse(supplementarySilverPage.GetTextFromLinkedAccountsSection(SupplementarySilver.LinkedMemberAssociationDate));

            DateTime currentDate = DateTime.UtcNow.Date;

            int associationPeriod = Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays);

            if (associationPeriod < 365)
            {
                supplementarySilverPage.ClickOnOverrideCheckbox();
            }

            supplementarySilverPage.ClickOnDeLinkButton(logger);

            supplementarySilverPage.ClickOnDeLinkYesButton(logger);

            string message = "Supplementary silver delinking successful";

            Assert.True(supplementarySilverPage.WaitForTextToBeDisplayedAfterSuccessfulDelinking(message),
                "Expected the Delinking Success Message to be displayed after delinking.");

            supplementarySilverPage.CloseThePopUp(logger);

            string expectedMessage = "No relationship to display";

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(supplementarySilverPage.IsDeLinkButtonDisabled(),
                "On Child Account expected the DeLink Button to be in Disabled state after a successful DeLinking from parent account");

            #region Audit data validation after delinking from Child Account

            supplementarySilverPage.ClickOnRefreshButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            string memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber);

            string activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType);

            string relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType);

            string updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy);

            string updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn);

            Assert.True(memberNumberInAuditSection.Equals(parentMemberNumber),
               "After Successful Delinking , In the SSA Audit Section of the Child Account," +
               $"Expected the  Member Number to be {parentMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the ActivityType to be Linking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Linked Gold"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the RelationshipType to be Linked Gold but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy") ?? "";

            Assert.True(updatedOnInAuditSection.Equals(currentDate.ToString("dd-MMM-yyyy")),
                 "After Successful Delinking , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedOn value to {currentDate.ToString("dd-MMM-yyyy")} but found {updatedOnInAuditSection}.");

            #endregion

            memberSearchPage.ClickOnPowerAppsHomeButton(logger);

            memberSearchPage.ClearTheSearchParametersAndResults(logger);

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedMessage),
                $"Expected the Text to be {expectedMessage} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(!supplementarySilverPage.IsLinkButtonDisabled(),
                "On Parent Account expected the Link Button to be in Enabled state after a successful DeLinking of a child account");

            memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber);

            activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType);

            relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType);

            updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy);

            updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn);


            Assert.True(memberNumberInAuditSection.Equals(childMemberNumber),
               "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
               $"Expected the  Member Number to be {childMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the ActivityType to be Delinking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Supp Silver"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the RelationshipType to be Supp Silver but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("@fly.virgin.com"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedBy value to have @fly.virgin.com but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy");

            Assert.True(updatedOnInAuditSection.Equals(currentDate.ToString("dd-MMM-yyyy")),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedOn value to {currentDate.ToString("dd-MMM-yyyy")} but found {updatedOnInAuditSection}.");

        }

        [Trait("Category", Traits.SupplementaryUIRegression)]
        [Fact, RunOrder(17)]
        public void TC17_SupplementarySilver_AutoDissolve_OnStatusUpdate_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC17_SupplementarySilver_AutoDissolve_OnStatusUpdate_SuccessScenario");

            #region Fetching a Supplementary Silver Account

            string queryForChildFcmDoc = "SELECT TOP 5 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.Status!=''  and c.SSAMemberNumber!='' order by c.LastUpdatedOn desc ";

            IEnumerable<FlyingClubMember> childFcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFcmDoc);

            string childMemberNumber = string.Empty;

            string? parentMemberNumber = childFcmDocs.FirstOrDefault(x =>
            {
                childMemberNumber = x.MemberNumber;

                string queryForParentFcmDoc = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember'  and c.Status!=''  and c.MemberNumber='{x.SSAMemberNumber}'";

                IEnumerable<FlyingClubMember> parentFcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForParentFcmDoc);

                return parentFcmDoc.Any(c => c.TierCode.Equals("Gold") && (c.Status.Equals("Active") || c.Status.Equals("Frozen") || c.Status.Equals("Suspended")));

            })?.SSAMemberNumber;

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber),
                "Didn't find a valid Parent Or Gold Member to test Auto Dissolve ");

            Assert.True(!string.IsNullOrEmpty(childMemberNumber),
                "Expected childMemberNumber to have a value but found empty or null.");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            //List of Status valid for Auto Dissolve of Supp Silver Account
            string[] status = new string[] { "Inactive", "Active Pending", "Fraud", "Deceased", "Cancelled" };

            Random random = new();

            string newStatus = status[random.Next(status.Length)];

            string firstName = "TestFirstName" + DataGenerator.RandomString(4).ToLower();

            string lastName = "TestLastName" + DataGenerator.RandomString(4).ToLower();

            memberProfilePage.SelectNewMemberStatus(logger, newStatus);

            string expectedAlertMessage = "This account has linked members and/or has a Supplementary Silver account. Changing the status will delink all linked members. Are you sure you want to proceed?";

            string actualAlertMessage = memberProfilePage.GetTextFromAlertPopUpScreen();

            Assert.True(actualAlertMessage.Equals(expectedAlertMessage),
                $"Expected the Alert Message to be {expectedAlertMessage} but found {actualAlertMessage}.");

            memberProfilePage.ClickOnConfirmYesButton();

            memberProfilePage.EnterNameChangeReason("Test");

            if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
            {

                string currentTitle = memberProfilePage.GetMemberCurrentTitle();

                if (string.IsNullOrEmpty(currentTitle))
                {
                    memberProfilePage.SelectNewTitle(logger, "Mr");
                }

                if (memberProfilePage.IsProfileUpdateSaveButtonDisabled())
                {

                    memberProfilePage.UpdateMemberFirstName(firstName);

                    memberProfilePage.UpdateMemberLastName(lastName);

                    memberProfilePage.UpdateMemberPrefCardName(firstName + lastName);
                }

            }

            memberProfilePage.ClickOnMemberPIDetailsSaveButton(logger);

            string expectedMessage = "Member details updated successfully!";

            string actualMessage = memberProfilePage.GetTextFromPopUpScreen();

            Assert.True(actualMessage.Equals(expectedMessage),
                $"Expected the Success Message to be {expectedMessage} but found {actualMessage}.");

            memberProfilePage.CloseThePopUpScreen(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            string expectedText = "No relationship to display";

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedText),
                $"Expected the Text to be {expectedText} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(!supplementarySilverPage.IsLinkButtonDisabled(),
                "On Parent Account expected the Link Button to be in Enabled state after a successful DeLinking of child account");

            #region Audit data validation after delinking from Parent Account

            supplementarySilverPage.ClickOnRefreshButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            string memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber);

            string activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType);

            string relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType);

            string updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy);

            string updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn);

            string currentDate = DateTime.UtcNow.Date.ToString("dd-MMM-yyyy");

            Assert.True(memberNumberInAuditSection.Equals(childMemberNumber),
               "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
               $"Expected the  Member Number to be {childMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the ActivityType to be Delinking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Supp Silver"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the RelationshipType to be Supp Silver but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Equals("Customer API"),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedBy value to have Customer API but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy");

            Assert.True(updatedOnInAuditSection.Equals(currentDate),
                 "After Successful Delinking , In the SSA Audit Section of the Parent Account," +
                $"Expected the UpdatedOn value to {currentDate} but found {updatedOnInAuditSection}.");

            #endregion

            memberSearchPage.ClickOnPowerAppsHomeButton(logger);

            memberSearchPage.ClearTheSearchParametersAndResults(logger);

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, childMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            supplementarySilverPage.ClickOnMenuBarOnMemberProfilePage(logger);

            supplementarySilverPage.ClickOnRelationShipTab(logger);

            supplementarySilverPage.ClickOnSupplementarySilverButton(logger);

            Assert.True(supplementarySilverPage.GetTextFromSuppSilverHistorySection().Equals(expectedText),
                $"Expected the Text to be {expectedText} but found {supplementarySilverPage.GetTextFromSuppSilverHistorySection()}");

            Assert.True(supplementarySilverPage.IsDeLinkButtonDisabled(),
                "On Child Account expected the DeLink Button to be in disabled state after a successful DeLinking from parent account");

            #region Validation of Linking Record in the Audit Section of Child Account

            memberNumberInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditMemberNumber) ?? "";

            activityTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditActivityType) ?? "";

            relationshipTypeInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditRelationshipType) ?? "";

            updatedByInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedBy) ?? "";

            updatedOnInAuditSection = supplementarySilverPage.GetTextFromAuditSection(SupplementarySilver.AuditUpdatedOn) ?? "";

            Assert.True(memberNumberInAuditSection.Equals(parentMemberNumber),
               "After Auto Dissolve , In the SSA Audit Section of the Child Account," +
               $"Expected the  Member Number to be {parentMemberNumber} but found {memberNumberInAuditSection}.");

            Assert.True(activityTypeInAuditSection.Equals("Delinking"),
                 "After Auto Dissolve , In the SSA Audit Section of the Child Account," +
                $"Expected the ActivityType to be Linking but found {activityTypeInAuditSection}.");

            Assert.True(relationshipTypeInAuditSection.Equals("Linked Gold"),
                 "After Auto Dissolve , In the SSA Audit Section of the Child Account," +
                $"Expected the RelationshipType to be Linked Gold but found {relationshipTypeInAuditSection}.");

            Assert.True(updatedByInAuditSection.Has("Customer API"),
                 "After Auto Dissolve , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedBy value to have Customer API but found {updatedByInAuditSection}.");

            updatedOnInAuditSection = DateTime.Parse(updatedOnInAuditSection).Date.ToString("dd-MMM-yyyy") ?? "";

            Assert.True(updatedOnInAuditSection.Equals(currentDate),
                 "After Auto Dissolve , In the SSA Audit Section of the Child Account," +
                $"Expected the UpdatedOn value to {currentDate} but found {updatedOnInAuditSection}.");

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

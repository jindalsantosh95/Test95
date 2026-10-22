using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsBackendAPITests
{
    [Collection("FinancialPartnerProcessingAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class FinancialPartnerProcessingAPITests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;

        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public FinancialPartnerProcessingAPITests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForFinancialPartnerApiFromKeyVault("financialPartner");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            this.cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

        }

        [Trait("Category", Traits.LOOPSFinancialPartnerAPIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("SYNF")]
        [InlineData("VCCU")]
        public async Task TC01_AwardVoucherViaFinancialPartnerProcessingApi(string voucherType)
        {
            string usCoBrandVoucherProductId = "1718882825098";
            string vccuVoucherProductId = "1-41G2YIN";

            #region Test Data           

            string queryForFcmemberDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.CreatedBy != @createdBy order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@createdBy", "WEB-MYA");

            FlyingClubMember flyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = flyingClubDocument?.CustomerId;

            string memberNumber = flyingClubDocument?.MemberNumber;

            string cardLinkPartnerUserId = DataGenerator.RandomNumber(1000000);

            DateTime randomDate = DataGenerator.RandomPastDate(100);

            DateTime currentDate = DateTime.UtcNow.Date;

            string cardOpenDate = randomDate.ToString("yyyy-MM-dd");

            string voucherActivityDate = randomDate == currentDate ? randomDate.ToString("yyyy-MM-dd") : randomDate.AddDays(+1).ToString("yyyy-MM-dd");

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.FinancialPartnerMigration.VoucherAward(tokenFixture.GetToken("financialPartner"), memberNumber, voucherType, cardLinkPartnerUserId, voucherActivityDate, cardOpenDate).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(result, "errors"), $"Response was not as expected. The response returned is \n {result} ");

            Assert.True(result.FinancialPartnerProcessVred_Output.TransactionNumber is not null, $"For memberNumber {memberNumber} - {voucherType} was not awarded. ");

            string transactionId = result.FinancialPartnerProcessVred_Output?.TransactionNumber;

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId ";

            QueryDefinition parameterizedAwardQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionId);

            Award awardDocument = cosmosFixture.CosmosQueryItem<Award>(parameterizedAwardQuery);

            Assert.True(awardDocument is not null, $"Award Document not found after Successful API Response for the voucher type {voucherType} with transactionId {transactionId} for the Member {memberNumber}");

            string awardNumber = awardDocument.AwardNumber;

            #endregion

            #region Response and Award document Validations

            SoftAssertHelper softAssert = new();

            softAssert.True(result.FinancialPartnerProcessVred_Output.MemberNumber.ToString().Equals(memberNumber), $"Voucher {voucherType} is not assigned to the {memberNumber} , Expected memberNumber in response to be {memberNumber} but was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.ErrorCode.ToString()), $"Voucher {voucherType} is not assigned to the {memberNumber} , Expected ErrorCode was null in response but actual was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.ErrorMessage.ToString()), $"Voucher {voucherType} is not assigned to the {memberNumber} , Expected ErrorMessage was null in response but actual was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(awardDocument.StartDate.ToString("yyyy-MM-dd").Equals(currentDate.ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Start Date on the Award Document is current date / today's date but actual date in the award document is : {awardDocument.StartDate} ");

            softAssert.True(awardDocument.ExpiryDate.ToString("yyyy-MM-dd").Equals(currentDate.AddYears(+2).ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected End Date on the Award Document is {currentDate.AddYears(+2).ToString("yyyy-MM-dd")} but actual date in the award document is : {awardDocument.ExpiryDate} ");

            string expectedProductId = voucherType.Equals("SYNF") ? usCoBrandVoucherProductId : vccuVoucherProductId;

            softAssert.True(awardDocument.ProductId == expectedProductId, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Product Id is {expectedProductId} but actual in the cosmos document is : {awardDocument.ProductId} ");

            softAssert.AssertAll();

            #endregion

            #region Audit Document Validation

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardNumber = @awardNumber ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId).WithParameter("@awardNumber", awardNumber);

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            int retry = 5;

            while (awardAuditDoc is null && retry > 0)
            {
                await Task.Delay(1000);

                awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

                retry--;
            }

            Assert.True(awardAuditDoc is not null, $"No Award Audit Document Found for the Voucher {voucherType} with Award Number {awardNumber} .");

            Assert.True(awardAuditDoc.StartDate.Value.ToString("yyyy-MM-dd").Equals(currentDate.ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Start Date in the Award Audit Document is current date / today's date but actual date in the award document is : {awardAuditDoc.StartDate} ");

            Assert.True(awardAuditDoc.EndDate.Value.ToString("yyyy-MM-dd").Equals(currentDate.AddYears(+2).ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected End Date in the Award Audit Document is {currentDate.AddYears(+2).ToString("yyyy-MM-dd")} but actual date in the award document is : {awardAuditDoc.EndDate} ");

            string expectedAwardName = voucherType.Equals("SYNF") ? "Flying Club reward voucher - Virgin Red US credit card" : "Flying Club reward voucher - US BoA credit card";

            Assert.True(awardAuditDoc.AwardName == expectedAwardName, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award Name in the Audit Document to be {expectedAwardName} but actual in the cosmos document is : {awardAuditDoc.AwardName} ");

            Assert.True(awardAuditDoc.AwardProductId == expectedProductId, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award ProductId in the Audit Document to be {expectedProductId} but actual in the cosmos document is : {awardAuditDoc.AwardProductId} ");

            string expectedStatus = "Available";

            Assert.True(awardAuditDoc.Status == expectedStatus, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award Status in the Audit Document to be {expectedStatus} but actual in the cosmos document is : {awardAuditDoc.Status} ");

            string expectedCreatedBy = "Azure API";

            Assert.True(awardAuditDoc.CreatedBy == expectedCreatedBy, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected the CreatedBy in the Audit Document to be {expectedCreatedBy} but actual in the cosmos document is : {awardAuditDoc.CreatedBy} ");

            Assert.True(awardAuditDoc.AwardUpdatedBy == expectedCreatedBy, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected the AwardUpdatedBy in the Audit Document to be {expectedCreatedBy} but actual in the cosmos document is : {awardAuditDoc.AwardUpdatedBy} ");

            #endregion
        }

        [Trait("Category", Traits.LOOPSFinancialPartnerAPIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("PREM")]
        [InlineData("STND")]
        public async Task TC02_AwardVMCCVoucherViaFinancialPartnerProcessingApi(string voucherType)
        {

            string vmccPremiumVoucherProductId = "1718881074393";
            string vmccStandardVoucherProductId = "1-2MG5VE1";

            #region Test Data

            string queryForFcmemberDoc = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            IEnumerable<FlyingClubMember> flyingClubDocuments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(parameterizedQuery);

            Assert.True(flyingClubDocuments.Any(), "No flying club documents found. Cannot proceed with test execution");

            string customerId = string.Empty;

            string memberNumber = string.Empty;

            bool flag = false;

            foreach (FlyingClubMember flyingClubMemberDoc in flyingClubDocuments)
            {
                customerId = flyingClubMemberDoc.CustomerId;

                memberNumber = flyingClubMemberDoc.MemberNumber;

                string queryForAllAwardDocs = $"SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.ProductId in ('{vmccPremiumVoucherProductId}', '{vmccStandardVoucherProductId}') order by c.CreatedOn desc ";

                QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllAwardDocs).WithParameter("@docType", "Award").WithParameter("@customerId", customerId);

                IEnumerable<Award> awardDocuments = cosmosFixture.CosmosQueryItems<Award>(queryForAllDocs);

                if (!awardDocuments.Any())
                {
                    flag = true;
                    break;
                }
            }

            Assert.True(flag, "No Members found for valid test data. Fetched members do have a voucher type already assigned");

            string cardLinkPartnerUserId = DataGenerator.RandomNumber(1000000);

            string cardLinkPartnerUserIdDomain = "VMCC";

            DateTime randomDate = DataGenerator.RandomPastDate(100);

            DateTime currentDate = DateTime.UtcNow.Date;

            string cardOpenDate = string.Empty;

            string voucherActivityDate = string.Empty;

            if (randomDate >= currentDate.AddDays(-7)) // For VMCC Card the activityDate must be >= cardOpenDate +7days
            {
                cardOpenDate = randomDate.AddDays(-7).ToString("yyyy-MM-dd");

                voucherActivityDate = randomDate.ToString("yyyy-MM-dd");
            }
            else
            {
                cardOpenDate = randomDate.ToString("yyyy-MM-dd");

                voucherActivityDate = randomDate.AddDays(+7).ToString("yyyy-MM-dd");
            }

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.FinancialPartnerMigration.VoucherAwardVMCC(tokenFixture.GetToken("financialPartner"), memberNumber, cardLinkPartnerUserId, cardLinkPartnerUserIdDomain, voucherActivityDate, cardOpenDate, voucherType).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(result?.FinancialPartnerProcessVred_Output?.ErrorCode.ToString()), $"For memberNumber {memberNumber} - {cardLinkPartnerUserIdDomain} was not awarded.The response was {result} ");

            string transactionId = result?.FinancialPartnerProcessVred_Output?.TransactionNumber;

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId ";

            QueryDefinition parameterizedAwardQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionId);

            Award awardDocument = cosmosFixture.CosmosQueryItem<Award>(parameterizedAwardQuery);

            Assert.True(awardDocument is not null, $"Award Document not found after Successful API Response for the voucher type {voucherType} with transactionId {transactionId} for the Member {memberNumber}");

            #endregion

            #region Validations

            SoftAssertHelper softAssert = new();

            softAssert.True(result.FinancialPartnerProcessVred_Output.MemberNumber.ToString().Equals(memberNumber), $"For the Member {memberNumber} , the Voucher VMCC {voucherType} is not assigned to the {memberNumber} , Expected memberNumber in response to be {memberNumber} but was {result}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.ErrorCode.ToString()), $"For the Member {memberNumber} ,the Voucher VMCC {voucherType} is not assigned to the {memberNumber}, Expected ErrorCode was null in response but actual was {result}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.ErrorMessage.ToString()), $"For the Member {memberNumber} ,the Voucher VMCC {voucherType} is not assigned to the {memberNumber}, Expected ErrorMessage was null in response but actual was {result}");

            softAssert.True(transactionId.Equals(awardDocument.TransactionId), $"For the Member {memberNumber} , the Voucher VMCC {voucherType} is not assigned to the {memberNumber}, Expected transaction id {transactionId} doesnot match the award doc txn id. Error message is {result}");

            softAssert.True(awardDocument.StartDate.ToString("yyyy-MM-dd") == currentDate.ToString("yyyy-MM-dd"), $"For the Member {memberNumber} , for the award of type VMCC - {voucherType} Expected Start Date on the Award Document is current date / today's date but actual date in the award document is : {awardDocument.StartDate.ToString()} ");

            softAssert.True(awardDocument.ExpiryDate.ToString("yyyy-MM-dd") == currentDate.AddYears(+2).ToString("yyyy-MM-dd"), $"For the Member {memberNumber} , for the award of type VMCC - {voucherType}, Expected End Date on the Award Document is {currentDate.AddYears(+2).ToString("yyyy-MM-dd")} but actual date in the award document is : {awardDocument.ExpiryDate} ");

            string expectedProductId = voucherType.Equals("PREM") ? vmccPremiumVoucherProductId : vmccStandardVoucherProductId;

            softAssert.True(awardDocument.ProductId == expectedProductId, $"For the Member {memberNumber}, for the award of type VMCC -{voucherType} , for the award of type VMCC - expected ProductId is  {expectedProductId} but actual in the cosmos document is : {awardDocument.ProductId} ");

            softAssert.AssertAll();

            string awardNumber = awardDocument.AwardNumber;

            #endregion

            #region Audit Document Validation

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardNumber = @awardNumber ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId).WithParameter("@awardNumber", awardNumber);

            AwardAudit awardAuditDoc = cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, $"No Award Audit Document Found for the Voucher {voucherType} with Award Number {awardNumber} .");

            Assert.True(awardAuditDoc.StartDate.Value.ToString("yyyy-MM-dd").Equals(currentDate.ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Start Date in the Award Audit Document is current date / today's date but actual date in the award document is : {awardAuditDoc.StartDate} ");

            Assert.True(awardAuditDoc.EndDate.Value.ToString("yyyy-MM-dd").Equals(currentDate.AddYears(+2).ToString("yyyy-MM-dd")), $"For the Member {memberNumber} , for the award of type {voucherType}, Expected End Date in the Award Audit Document is {currentDate.AddYears(+2).ToString("yyyy-MM-dd")} but actual date in the award document is : {awardAuditDoc.EndDate} ");

            string expectedAwardName = voucherType.Equals("PREM") ? "Flying Club reward voucher - UK Reward+ credit card" : "Flying Club reward voucher - UK Reward credit card";

            Assert.True(awardAuditDoc.AwardName == expectedAwardName, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award Name in the Audit Document to be {expectedAwardName} but actual in the cosmos document is : {awardAuditDoc.AwardName} ");

            Assert.True(awardAuditDoc.AwardProductId == expectedProductId, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award ProductId in the Audit Document to be {expectedProductId} but actual in the cosmos document is : {awardAuditDoc.AwardProductId} ");

            string expectedStatus = "Available";

            Assert.True(awardAuditDoc.Status == expectedStatus, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected Award Status in the Audit Document to be {expectedStatus} but actual in the cosmos document is : {awardAuditDoc.Status} ");

            string expectedCreatedBy = "Azure API";

            Assert.True(awardAuditDoc.CreatedBy == expectedCreatedBy, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected the CreatedBy in the Audit Document to be {expectedCreatedBy} but actual in the cosmos document is : {awardAuditDoc.CreatedBy} ");

            Assert.True(awardAuditDoc.AwardUpdatedBy == expectedCreatedBy, $"For the Member {memberNumber} , for the award of type {voucherType}, Expected the AwardUpdatedBy in the Audit Document to be {expectedCreatedBy} but actual in the cosmos document is : {awardAuditDoc.AwardUpdatedBy} ");

            #endregion
        }

        [Trait("Category", Traits.LOOPSFinancialPartnerAPIRegression)]
        [Theory, RunOrder(3)]
        [InlineData("PREM")]
        public async Task TC03_AwardVMCCVoucherDuplicateCheck(string activityCode)
        {
            #region Test Data

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@productId", "1718881074393");

            Award awardDocument = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDocument is not null, "No award document found. Cannot proceed with test execution");

            string customerId = awardDocument.CustomerId;

            string queryForFcmemberDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.ActivityNumber = @activityCode ";

            QueryDefinition parameterizedTxnQuery = new QueryDefinition(query: queryForFcmemberDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@activityCode", activityCode);

            Transaction transactionDocument = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedTxnQuery);

            Assert.True(transactionDocument is not null, $"No trnsaction document found with activity code {activityCode}. Cannot proceed with test execution");

            string memberNumber = transactionDocument.MemberNumber;

            string cardLinkPartnerUserId = transactionDocument?.FinancialCreditDetails?.PANNumber;

            string cardLinkPartnerUserIdDomain = "VMCC";

            DateTime randomDate = DataGenerator.RandomPastDate(100);

            DateTime currentDate = DateTime.UtcNow.Date;

            string cardOpenDate = string.Empty;

            string voucherActivityDate = string.Empty;

            if (randomDate >= currentDate.AddDays(-7)) // For VMCC Card the activityDate must be >= cardOpenDate +7days
            {
                cardOpenDate = randomDate.AddDays(-7).ToString("yyyy-MM-dd");

                voucherActivityDate = randomDate.ToString("yyyy-MM-dd");
            }
            else
            {
                cardOpenDate = randomDate.ToString("yyyy-MM-dd");

                voucherActivityDate = randomDate.AddDays(+7).ToString("yyyy-MM-dd");
            }

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.FinancialPartnerMigration.VoucherAwardVMCC(tokenFixture.GetToken("financialPartner"), memberNumber, cardLinkPartnerUserId, cardLinkPartnerUserIdDomain, voucherActivityDate, cardOpenDate, activityCode).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(result, "FinancialPartnerProcessVred_Output"), $"For memberNumber {memberNumber} - {cardLinkPartnerUserIdDomain} was not awarded. The response returned is {result} ");

            #endregion

            #region Validations

            SoftAssertHelper softAssert = new();

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.MemberNumber.ToString()), $"Expected member number to be null for duplicate voucher assignment scenario for the member {memberNumber} but was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(result.FinancialPartnerProcessVred_Output.ErrorCode.ToString().Equals("AZ409"), $"Expected Error code for duplicate voucher scenario is AZ409 but actual was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(result.FinancialPartnerProcessVred_Output.ErrorMessage.ToString().Contains("The voucher award is a duplicate"), $"Expected Error code for duplicate voucher scenario is The voucher award is a duplicate but actual wa{result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.MemberStatus.ToString()), $"Expected member status to be null for duplicate voucher assignment scenario for the member {memberNumber} but was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.True(string.IsNullOrEmpty(result.FinancialPartnerProcessVred_Output.TransactionNumber.ToString()), $"Expected member TransactionNumber to be null for duplicate voucher assignment scenario for the member {memberNumber} but was {result.FinancialPartnerProcessVred_Output.ToString()}");

            softAssert.AssertAll();

            #endregion
        }

        [Trait("Category", Traits.LOOPSFinancialPartnerAPIRegression)]
        [Theory, RunOrder(4)]
        [InlineData(">=2500")]
        [InlineData(">=5000")]
        public async Task TC04_AwardTierPointsForUSCoBrandVouchersViaFinancialPartnerProcessingApi(string amountSpendCriteria)
        {

            string usCoBrandVoucherProductId = "1724331539953";

            #region Test Data

            string queryForFcmemberDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(flyingClubDocument is not null, "No flying club documents found. Cannot proceed with test execution");

            string customerId = flyingClubDocument.CustomerId;

            string memberNumber = flyingClubDocument.MemberNumber;

            string transactionId = DataGenerator.RandomString(10).ToLowerInvariant();

            transactionId = transactionId.Substring(0, 2).ToUpperInvariant() + transactionId.Substring(2);

            string cardLinkPartnerUserId = DataGenerator.RandomNumber(1000000, 9999999, true);

            string cardLinkPartnerUserIdDomain = "SYNF";

            string transactionActivityDate = DataGenerator.RandomPastDate(100).ToString("yyyy-MM-dd");

            string monthlySpendAmount;

            string tierPointsAssigned;

            if (amountSpendCriteria.Equals(">=2500"))
            {
                monthlySpendAmount = DataGenerator.RandomNumber(2500, 4999, true).ToString();

                tierPointsAssigned = "25";
            }
            else
            {
                monthlySpendAmount = DataGenerator.RandomNumber(5000, 10000, true).ToString();

                tierPointsAssigned = "50";
            }

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.FinancialPartnerMigration.UsCoBrandVoucherTierpointsTxn(tokenFixture.GetToken("financialPartner"), transactionId, memberNumber, transactionActivityDate, cardLinkPartnerUserIdDomain, cardLinkPartnerUserId, monthlySpendAmount).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(result.Is("Message Posted Successfully"), $"For memberNumber {memberNumber} - and for {monthlySpendAmount} API message was not posted successfully to award API. The response was \n {result} ");

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.ParentTransactionId = @transactionId Order by c.CreatedOn desc ";

            QueryDefinition parameterizedTxnQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionId);

            Transaction TransactionDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedTxnQuery);

            int retry = 10;

            while (TransactionDoc is null && retry > 0)
            {
                await Task.Delay(2000);

                TransactionDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedTxnQuery);

                retry--;
            }

            Assert.True(TransactionDoc is not null, $"For memberNumber {memberNumber} - and for monthly amount spent {monthlySpendAmount} , after successful response from api ,failed to find the transaction doc with transaction {transactionId} in the cosmos database.");

            #endregion

            #region Validations

            string expectedTransactionStatus = "Processed";

            string expectedStatementText = "Tier points – Virgin Red US credit card";

            SoftAssertHelper softAssert = new();

            softAssert.True(TransactionDoc.TransactionValue.Equals(tierPointsAssigned), $"Tier Points {tierPointsAssigned} for {amountSpendCriteria} CoBrand Cards (SYNF) Type is not assigned to the {memberNumber}. Expected value : {tierPointsAssigned} Actual response is : {TransactionDoc.TransactionValue}");

            softAssert.True(TransactionDoc.ProductId.Equals(usCoBrandVoucherProductId), $"ProductId -{usCoBrandVoucherProductId} for {amountSpendCriteria} CoBrand Cards (SYNF) Type is not avaliable in txn document for the {memberNumber}. Expected value : {usCoBrandVoucherProductId} Actual response is : {TransactionDoc.ProductId}");

            softAssert.True(TransactionDoc.StatusCode.Equals(expectedTransactionStatus), $"The transaction status code of the transaction created  for {amountSpendCriteria} CoBrand Cards (SYNF) Type for the {memberNumber} is not as expected. Expected value : {expectedTransactionStatus} Actual response is : {TransactionDoc.StatusCode}");

            softAssert.True(TransactionDoc.StatementText.Equals(expectedStatementText), $"The transaction Statement Text of the transaction created  for {amountSpendCriteria} CoBrand Cards (SYNF) Type for the {memberNumber} is not as expected. Expected value : {expectedStatementText} Actual response is : {TransactionDoc.StatementText}");

            softAssert.AssertAll();

            #endregion
        }

        [Trait("Category", Traits.LOOPSFinancialPartnerAPIRegression)]
        [Theory, RunOrder(5)]
        [InlineData(">=2500")]
        [InlineData(">=5000")]
        public async Task TC05_AwardTierPointsForVCCUVouchersViaFinancialPartnerProcessingApi(string amountSpendCriteria)
        {
            string usccAwardTierPointsProductId = "1-305P2L";

            #region Test Data

            string queryForFcmemberDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFcmemberDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(flyingClubDocument is not null, "No flying club documents found. Cannot proceed with test execution");

            string customerId = flyingClubDocument.CustomerId;

            string memberNumber = flyingClubDocument.MemberNumber;

            string transactionId = DataGenerator.RandomString(10);

            string cardLinkPartnerUserId = DataGenerator.RandomNumber(1000000);

            string cardLinkPartnerUserIdDomain = "VCCU";

            string creditCardType = "PREMIUM";

            string transactionActivityDate = DataGenerator.RandomPastDate(100).ToString("yyyy-MM-dd");

            string monthlySpendAmount;

            string tierPointsAssigned;

            if (amountSpendCriteria.Equals(">=2500"))
            {
                monthlySpendAmount = DataGenerator.RandomNumber(2500, 4999, true).ToString();

                tierPointsAssigned = "25";
            }
            else
            {
                monthlySpendAmount = DataGenerator.RandomNumber(5000, 10000, true).ToString();

                tierPointsAssigned = "50";
            }

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.FinancialPartnerMigration.VCCUVoucherTierpointsTxn(tokenFixture.GetToken("financialPartner"), transactionId, memberNumber, transactionActivityDate, cardLinkPartnerUserIdDomain, cardLinkPartnerUserId, monthlySpendAmount, creditCardType).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(result.Is("Message Posted Successfully"), $"For memberNumber {memberNumber} - and for {monthlySpendAmount} API message was not posted successfully to award API. The response was \n {result} ");

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.ParentTransactionId = @transactionId ";

            QueryDefinition parameterizedTxnQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionId);

            Transaction TransactionDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedTxnQuery);

            int retry = 3;

            while (TransactionDoc is null && retry > 0)
            {
                await Task.Delay(1000);

                TransactionDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedTxnQuery);

                retry--;
            }

            Assert.True(TransactionDoc is not null, $"For memberNumber {memberNumber} - and for for monthly amount spent {monthlySpendAmount} , after successful response from api ,failed to find the transaction doc with transaction {transactionId} in the cosmos database.");

            #endregion

            #region Validations

            string expectedTransactionStatus = "Processed";

            SoftAssertHelper softAssert = new();

            softAssert.True(TransactionDoc.StatusCode.Equals(expectedTransactionStatus), $"Transaction status is not Processed for Tier Points {tierPointsAssigned} for {amountSpendCriteria} VCCU Cards (VCCU) Type for {memberNumber}. Expected value : {expectedTransactionStatus} Actual response is : {TransactionDoc.StatusCode}");

            softAssert.True(TransactionDoc.TransactionValue.Equals(tierPointsAssigned), $"Tier Points {tierPointsAssigned} for {amountSpendCriteria} VCCU Cards (VCCU) Type is not assigned to the {memberNumber}. Expected value : {tierPointsAssigned} Actual response is : {TransactionDoc.TransactionValue}");

            softAssert.True(TransactionDoc.ProductId.Equals(usccAwardTierPointsProductId), $"ProductId -1-305P2L Name: 'Uscc Award Tier Points' for {amountSpendCriteria} VCCU Cards (VCCU) Type is not avaliable in txn document for the {memberNumber}. Expected value :{usccAwardTierPointsProductId} Actual response is : {TransactionDoc.ProductId}");

            softAssert.AssertAll();

            #endregion
        }
    }
}

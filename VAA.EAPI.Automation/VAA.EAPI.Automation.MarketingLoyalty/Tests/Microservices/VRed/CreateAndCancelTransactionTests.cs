using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.VglcTransactionDB;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.VRed
{
    [Collection("VRedMSTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CreateAndCancelTransactionTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {

        private readonly CosmosFixture _cosmosFixture;

        private readonly TokenFixture _tokenFixture;

        private static string env = Config.Environment;

        private readonly string customerCosmosDb;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public CreateAndCancelTransactionTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;

            _tokenFixture = tokenFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            ocpSubKey = Vault.Default.GetSecretValue("TestingVRedMSSubscriptionKey");
        }


        [Trait("Category", Traits.VRedMSRegression)]
        [Fact, RunOrder(1)]
        public async Task CreateVAADutyFreeEarnTransaction_SuccessScenario()
        {
            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.Status = @status and c.CreatedBy != 'WEB' order by c.CreatedOn desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Customer").WithParameter("@status", "Active");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve the Test Data. Customer Document from Cosmos DB");

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string partnerCode = "DF";
            string productName = "VAA Duty Free";
            string activityDate = DataGenerator.RandomPastDate(+365).ToString("dd-MMM-yyyy");
            string comments = "Test Transaction Created through Automation";
            int tierPoints = 0;
            int points = Convert.ToInt32(DataGenerator.RandomNumber(100, 500, true));
            string productDescription = "VAA Duty Free shopping";
            string partnerName = "Duty Free";
            string partnerCategory = "Ground Partner";
            string productCategory = "Manual Adjustment";
            string transactionSource = "RetailTherapy";
            string createdBy = "AutomationScript";
            string transactionChannelId = "WEBSITE";
            string activityType = "Accrual";

            #endregion

            #region Get Member Point Balance API Call Before Earn Transaction Creation

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

            long memberUsableBalanceBeforeTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            #endregion

            #region Create Earn Transaction API Call

            HttpResponseMessage earnTxnResponse = Microservice.CreateTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, customerId, memberNumber, partnerCode, productName,
                                                activityDate, comments, tierPoints, points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannelId, productDescription, activityType).EnsureIsAcceptedStatusCode();

            string responseUrl = earnTxnResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualEarnTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualEarnTxnResponse?.Content?.ReadAsStringAsync();

            string partnerTransactionId = JsonDocument.Parse(JsonDocument.Parse(content).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCreated = !partnerTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCreated, $"Earn Transaction creation failed for the member {memberNumber}. The response returned is \n {actualEarnTxnResponse}");

            #endregion

            #region Get Member Point Balance API Call After Earn Transaction Creation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            int retry = 5;

            while (retry > 0 && memberUsableBalanceAfterTxn <= memberUsableBalanceBeforeTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

                memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterTxn == memberUsableBalanceBeforeTxn + points,
                $"The Member- {memberNumber} - Point balance did not match after the successful Earn transaction Creation.Expected point balance to be  {memberUsableBalanceBeforeTxn + points} but found {memberUsableBalanceAfterTxn}");

            #endregion

            #region Validation of Transaction in the Member Transaction Statement

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            string response = await memberTxnStatement?.Content?.ReadAsStringAsync();

            VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(response);

            bool isTransactionPresent = false;

            IEnumerable<Statement> statements = vRedStatementResponse?.statements?.OrderByDescending(x => x.transactionDate);

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.transactionId == partnerTransactionId)
                {
                    isTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Accrual",
                        $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Accrual but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Product",
                        $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == productDescription,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Processed",
                     $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Processed but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Earn Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    break;
                }
            }

            Assert.True(isTransactionPresent, $"For the TransactionId {partnerTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion


            # region Audit transaction in  avateunsqldbvglctransaction SQLDB Validations


            IEnumerable<PowerAppTransactionAuditInfo> getPowerAppTransactionAudit = AzureVglcTransactionDBHelper.PowerAppTransactionAuditInfo(memberNumber);

            Assert.True(getPowerAppTransactionAudit.Any(), $"No audit records returned from VGLC DB for member number {memberNumber}");

            PowerAppTransactionAuditInfo powerAppTransactionAudit = getPowerAppTransactionAudit.FirstOrDefault(x => x.TransactionType == "Earn") ?? null;

            Assert.True(powerAppTransactionAudit is not null, "Original Earn transaction not found in Audit Info Table");

            Assert.True(!string.IsNullOrEmpty(powerAppTransactionAudit.AuditId.ToString()), $"Audit ID is not present in the SQL DB for the record {powerAppTransactionAudit.AuditId.ToString()}");

            Assert.True(powerAppTransactionAudit.TransactionID.Equals(partnerTransactionId), $"Expected transaction id is {partnerTransactionId} but found {powerAppTransactionAudit.TransactionID}");

            Assert.True(powerAppTransactionAudit.TransactionType.Equals("Earn"), $"Expected transaction Type is Earn but found {powerAppTransactionAudit.TransactionType}");

            DateTime expectedActivityDate = DateTime.ParseExact(activityDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DateTime actualActivityDate = DateTime.ParseExact(powerAppTransactionAudit.ActivityDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            Assert.True(actualActivityDate == expectedActivityDate, $"Expected activity date is {expectedActivityDate:MM/dd/yyyy} but found {powerAppTransactionAudit.ActivityDate}");

            #endregion


        }

        [Trait("Category", Traits.VRedMSRegression)]
        [Fact, RunOrder(2)]
        public async Task CreateVAADutyFreeBurnTransaction_SuccessScenario()
        {
            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.ProductId in ('1-1Z9D','1-1ZWY5ZT') and c.TransactionValue > 0 order by c.CreatedOn desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual");

            Transaction transactionDoc = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(transactionDoc is not null, "Failed to retrieve the Test Data. A Transaction Document from Cosmos DB");

            string customerId = transactionDoc.CustomerId;

            string memberNumber = transactionDoc.MemberNumber;

            string createdBy = "AutomationScript";

            string transactionChannelId = "WEBSITE";

            #endregion

            #region Get Member Point Balance API Call Before Burn Transaction Creation

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceBeforeTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            #endregion

            int retry = 5;

            #region If Member Point Balance is 0 , Creating a Manual Adjustment Transaction 

            if (memberUsableBalanceBeforeTxn <= 0)
            {

                int pointsForManualAdjustment = Convert.ToInt32(DataGenerator.RandomNumber(100, 1000, true));

                string activityDateForAdjustment = DataGenerator.RandomPastDate(+365).ToString("dd-MMM-yyyy");

                HttpResponseMessage response = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, "VS", "Authorised Points Credit", activityDateForAdjustment, "Test txn", pointsForManualAdjustment, 0, "AutomationScript",
                    createdBy, createdBy, customerId).EnsureIsSuccessStatusCode();

                HttpResponseMessage vRedResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse?.Content?.ReadAsStringAsync());

                int memberUsableBalanceAfterAdjustment = string.IsNullOrEmpty(vRedResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(vRedResult?.Usable?.ToString());

                while (retry > 0 && memberUsableBalanceAfterAdjustment == memberUsableBalanceBeforeTxn)
                {
                    await Task.Delay(2000);

                    vRedResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                    vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse?.Content?.ReadAsStringAsync());

                    memberUsableBalanceAfterAdjustment = string.IsNullOrEmpty(vRedResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(vRedResult?.Usable?.ToString());

                    retry--;
                }

                Assert.True(memberUsableBalanceAfterAdjustment == memberUsableBalanceBeforeTxn + pointsForManualAdjustment,
                   $"The Member {memberNumber} Usable balance after the successful Manual Credit Adjustment is not as expected. Expected the Usable balance to be  {memberUsableBalanceBeforeTxn + pointsForManualAdjustment} but  was {memberUsableBalanceAfterAdjustment}.");

                memberUsableBalanceBeforeTxn = memberUsableBalanceAfterAdjustment;
            }

            #endregion

            string partnerCode = "DF";
            string productName = "VAA Duty Free";
            string activityDate = DataGenerator.RandomFutureDate(+365).ToString("dd-MMM-yyyy");
            string comments = "Test Transaction Created through Automation";
            int tierPoints = 0;
            long points = Convert.ToInt64(DataGenerator.RandomNumber(0, (int)memberUsableBalanceBeforeTxn, true));
            string productDescription = "VAA Duty Free shopping";
            string partnerName = "Duty Free";
            string partnerCategory = "Ground Partner";
            string productCategory = "Manual Adjustment";
            string transactionSource = "RetailTherapy";
            string activityType = "Redemption";

            #region Create Burn Transaction API Call

            HttpResponseMessage burnTxnResponse = Microservice.CreateTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, customerId, memberNumber, partnerCode, productName,
                                                activityDate, comments, tierPoints, (int)points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannelId, productDescription, activityType).EnsureIsAcceptedStatusCode();

            string responseUrl = burnTxnResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualBurnTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualBurnTxnResponse?.Content?.ReadAsStringAsync();

            string partnerTransactionId = JsonDocument.Parse(JsonDocument.Parse(content).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCreated = !partnerTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCreated, $"Burn Transaction creation failed for the member {memberNumber}. The response returned is \n {actualBurnTxnResponse}");

            #endregion

            #region Get Member Point Balance API Call After Burn Transaction Creation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            retry = 5;

            while (retry > 0 && memberUsableBalanceAfterTxn <= memberUsableBalanceBeforeTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterTxn == memberUsableBalanceBeforeTxn - points,
                $"The Member- {memberNumber} - Point balance did not match after the successful Burn transaction Creation. \n Expected point balance to be  {memberUsableBalanceBeforeTxn - points} but found {memberUsableBalanceAfterTxn}");

            #endregion

            #region Validation of Transaction in the Member Transaction Statement

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            string statementResponse = await memberTxnStatement?.Content?.ReadAsStringAsync();

            VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(statementResponse);

            bool isTransactionPresent = false;

            IEnumerable<Statement> statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.transactionId == partnerTransactionId)
                {
                    isTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Redemption",
                        $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Redemption but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Product",
                        $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == productDescription,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Processed",
                     $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Processed but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Burn Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    break;
                }
            }

            Assert.True(isTransactionPresent, $"For the TransactionId {partnerTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion


            #region Audit transaction in  avateunsqldbvglctransaction SQLDB Validations


            IEnumerable<PowerAppTransactionAuditInfo> getPowerAppTransactionAudit = AzureVglcTransactionDBHelper.PowerAppTransactionAuditInfo(memberNumber);

            Assert.True(getPowerAppTransactionAudit.Any(), $"No audit records returned from VGLC DB for member number {memberNumber}");

            PowerAppTransactionAuditInfo powerAppTransactionAudit = getPowerAppTransactionAudit.FirstOrDefault(x => x.TransactionType == "Burn") ?? null;

            Assert.True(powerAppTransactionAudit is not null, "Original Burn transaction not found in Audit Info Table");

            Assert.True(!string.IsNullOrEmpty(powerAppTransactionAudit.AuditId.ToString()), $"Audit ID is not present in the SQL DB for the record {powerAppTransactionAudit.AuditId.ToString()}");

            Assert.True(powerAppTransactionAudit.TransactionID.Equals(partnerTransactionId), $"Expected transaction id is {partnerTransactionId} but found {powerAppTransactionAudit.TransactionID}");

            Assert.True(powerAppTransactionAudit.TransactionType.Equals("Burn"), $"Expected transaction Type is Burn but found {powerAppTransactionAudit.TransactionType}");

            DateTime expectedActivityDate = DateTime.ParseExact(activityDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DateTime actualActivityDate = DateTime.ParseExact(powerAppTransactionAudit.ActivityDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            Assert.True(actualActivityDate == expectedActivityDate, $"Expected activity date is {expectedActivityDate:MM/dd/yyyy} but found {powerAppTransactionAudit.ActivityDate}");

            #endregion
        }

        [Trait("Category", Traits.VRedMSRegression)]
        [Fact, RunOrder(3)]
        public async Task CancelVAADutyFreeEarnTransaction_SuccessScenario()
        {
            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.Status = @status  and c.CreatedBy != 'WEB' order by c.CreatedOn desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Customer").WithParameter("@status", "Active");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve the Test Data. Customer Document from Cosmos DB");

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            DateTime randomPastDate = DataGenerator.RandomPastDate(+365);

            string partnerCode = "DF";
            string productName = "VAA Duty Free";
            string activityDate = randomPastDate.ToString("dd-MMM-yyyy");
            string comments = "Test Transaction Created through Automation";
            int tierPoints = 0;
            int points = Convert.ToInt32(DataGenerator.RandomNumber(100, 2000, true));
            string productDescription = "VAA Duty Free shopping";
            string partnerName = "Duty Free";
            string partnerCategory = "Ground Partner";
            string productCategory = "Manual Adjustment";
            string transactionSource = "RetailTherapy";
            string createdBy = "AutomationScript";
            string transactionChannelId = "WEBSITE";
            string activityType = "Accrual";

            #endregion

            #region Get Member Point Balance API Call Before Earn Transaction Creation

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceBeforeTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            #endregion

            #region Create Earn Transaction API Call

            HttpResponseMessage earnTxnResponse = Microservice.CreateTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, customerId, memberNumber, partnerCode, productName,
                                                activityDate, comments, tierPoints, points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannelId, productDescription, activityType).EnsureIsAcceptedStatusCode();

            string responseUrl = earnTxnResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualEarnTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualEarnTxnResponse?.Content?.ReadAsStringAsync();

            string partnerTransactionId = JsonDocument.Parse(JsonDocument.Parse(content).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCreated = !partnerTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCreated, $"Earn Transaction creation failed for the member {memberNumber}. The response returned is \n {actualEarnTxnResponse}");

            #endregion

            #region Get Member Point Balance API Call After Earn Transaction Creation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            int retry = 5;

            while (retry > 0 && memberUsableBalanceAfterTxn <= memberUsableBalanceBeforeTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterTxn == memberUsableBalanceBeforeTxn + points,
                $"The Member- {memberNumber} - Point balance did not match after the successful Earn transaction Creation. \n Expected point balance to be  {memberUsableBalanceBeforeTxn + points} but found {memberUsableBalanceAfterTxn}");

            #endregion          

            #region Cancellation of the Created Transaction

            string cancellationReason = "Product Returned";

            HttpResponseMessage cancelAPIResponse = Microservice.CancelTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber, partnerTransactionId,
                                    randomPastDate.ToString("yyyy/MM/dd"), transactionChannelId, cancellationReason, createdBy).EnsureIsAcceptedStatusCode();

            int retrycount = 5;

            while (retrycount > 0 && (int)cancelAPIResponse.StatusCode != 202)
            {
                await Task.Delay(2000);

                cancelAPIResponse = Microservice.CancelTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber, partnerTransactionId,
                                     randomPastDate.ToString("yyyy/MM/dd"), transactionChannelId, cancellationReason, createdBy).EnsureIsAcceptedStatusCode();

                retrycount--;
            }

            Assert.True((int)cancelAPIResponse.StatusCode == 202, $"Cancel Transaction POST url returned \n {await cancelAPIResponse.Content.ReadAsStringAsync()}");

            string CancelResponseUrl = cancelAPIResponse?.Headers?.Location?.ToString();

            HttpResponseMessage cancelTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, CancelResponseUrl);

            retrycount = 5;

            while (retrycount > 0 && (int)cancelTxnResponse.StatusCode != 200)
            {
                await Task.Delay(2000);

                cancelTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, CancelResponseUrl);

                retrycount--;
            }

            Assert.True(cancelTxnResponse.IsSuccessStatusCode, $"Cancel Transaction Get Response url returned \n {await cancelTxnResponse.Content.ReadAsStringAsync()}");

            string cancelResult = await cancelTxnResponse?.Content?.ReadAsStringAsync();

            string cancelledTransactionId = JsonDocument.Parse(JsonDocument.Parse(cancelResult).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCancelled = !cancelledTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCancelled, $"Earn Transaction -{partnerTransactionId} Cancellation failed for the member {memberNumber}. The response returned is \n {actualEarnTxnResponse}");

            #endregion

            #region Get Member Point Balance API Call After Earn Transaction Cancellation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterCancellation = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            retry = 5;

            while (retry > 0 && memberUsableBalanceAfterCancellation >= memberUsableBalanceAfterTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                memberUsableBalanceAfterCancellation = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterCancellation == memberUsableBalanceBeforeTxn,
                $"The Member- {memberNumber} - Point balance did not match after the successful Earn transaction Cancellation. \n Expected point balance to be  {memberUsableBalanceBeforeTxn} but found {memberUsableBalanceAfterCancellation}");

            #endregion

            #region Validation of both Original and Compensatory Transactions in the Member Transaction Statement

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

            bool isCancelledEarnTransactionPresent = false;

            IEnumerable<Statement> statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

            #region Validation of the Cancelled Accrual/Earn Transaction in the VRed Txn Statement

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.transactionId == partnerTransactionId)
                {
                    isCancelledEarnTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Accrual",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Accrual but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Product",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == productDescription,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Cancelled",
                     $"For Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Cancelled but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    Assert.True(statement.Details.cancellationReason == cancellationReason,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The Cancellation Reason does not match. Expected {cancellationReason} but found {statement.Details.cancellationReason}");

                    break;
                }
            }

            Assert.True(isCancelledEarnTransactionPresent, $"For the Cancelled TransactionId {partnerTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion

            #region Validation of the Compensatory Redemption/Burn Transaction in the VRed Txn Statement

            bool isCompensatoryBurnTransactionPresent = false;

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.cancelledTxnId == partnerTransactionId)
                {
                    isCompensatoryBurnTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Redemption",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Redemption but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Accrual Cancellation",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Accrual Cancellation but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == "Cancelled: " + productDescription,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Processed",
                     $"For Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Processed but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    Assert.True(statement.Details.cancellationReason.IsNullOrEmpty(),
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The Cancellation Reason does not match. Expected  but found {statement.Details.cancellationReason}");

                    break;
                }
            }

            Assert.True(isCompensatoryBurnTransactionPresent, $"For the Compensatory TransactionId {cancelledTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion

            #endregion


            #region Audit transaction in  avateunsqldbvglctransaction SQLDB Validations


            IEnumerable<PowerAppTransactionAuditInfo> getPowerAppTransactionAudit = AzureVglcTransactionDBHelper.PowerAppTransactionAuditInfo(memberNumber);

            Assert.True(getPowerAppTransactionAudit.Any(), $"No audit records returned from VGLC DB for member number {memberNumber}");

            PowerAppTransactionAuditInfo powerAppTransactionAudit = getPowerAppTransactionAudit.FirstOrDefault(x => x.TransactionType == "Earn") ?? null;

            Assert.True(powerAppTransactionAudit is not null, "Original Earn transaction not found in Audit Info Table");

            Assert.True(!string.IsNullOrEmpty(powerAppTransactionAudit.AuditId.ToString()), $"Audit ID is not present in the SQL DB for the record {powerAppTransactionAudit.AuditId.ToString()}");

            Assert.True(powerAppTransactionAudit.TransactionID.Equals(partnerTransactionId), $"Expected transaction id is {partnerTransactionId} but found {powerAppTransactionAudit.TransactionID}");

            Assert.True(powerAppTransactionAudit.Status.Equals("Cancelled"), $"Expected transaction Status to be Cancelled but found {powerAppTransactionAudit.Status}");

            DateTime expectedActivityDate = DateTime.ParseExact(activityDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DateTime actualActivityDate = DateTime.ParseExact(powerAppTransactionAudit.ActivityDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            Assert.True(actualActivityDate == expectedActivityDate, $"Expected activity date is {expectedActivityDate:MM/dd/yyyy} but found {powerAppTransactionAudit.ActivityDate}");

            #endregion
        }

        [Trait("Category", Traits.VRedMSRegression)]
        [Fact, RunOrder(4)]
        public async Task CancelVAADutyFreeBurnTransaction_SuccessScenario()
        {
            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.ProductId in ('1-1Z9D','1-1ZWY5ZT') and c.TransactionValue > 0 order by c.CreatedOn desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual");

            Transaction transactionDoc = _cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(transactionDoc is not null, "Failed to retrieve the Test Data. A Transaction Document from Cosmos DB");

            string customerId = transactionDoc.CustomerId;

            string memberNumber = transactionDoc.MemberNumber;

            string createdBy = "AutomationScript";

            string transactionChannelId = "WEBSITE";

            #endregion

            #region Get Member Point Balance API Call Before Burn Transaction Creation

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

            long memberUsableBalanceBeforeTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            #endregion

            int retry = 5;

            #region If Member Point Balance is 0 , Creating a Manual Adjustment Transaction 

            if (memberUsableBalanceBeforeTxn <= 0)
            {

                int pointsForManualAdjustment = Convert.ToInt32(DataGenerator.RandomNumber(100, 2000, true));

                string activityDateForAdjustment = DataGenerator.RandomPastDate(+365).ToString("dd-MMM-yyyy");

                HttpResponseMessage creditAPIResponse = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, "VS", "Authorised Points Credit", activityDateForAdjustment, "Test txn", pointsForManualAdjustment, 0, "AutomationScript",
                    createdBy, createdBy, customerId).EnsureIsSuccessStatusCode();

                HttpResponseMessage vRedResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                int memberUsableBalanceAfterAdjustment = string.IsNullOrEmpty(vRedResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(vRedResult?.Usable?.ToString());

                while (retry > 0 && memberUsableBalanceAfterAdjustment == memberUsableBalanceBeforeTxn)
                {
                    await Task.Delay(2000);

                    vRedResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                    vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                    memberUsableBalanceAfterAdjustment = string.IsNullOrEmpty(vRedResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(vRedResult?.Usable?.ToString());

                    retry--;
                }

                Assert.True(memberUsableBalanceAfterAdjustment == memberUsableBalanceBeforeTxn + pointsForManualAdjustment,
                   $"The Sender Member {memberNumber} Usable balance after the successful Manual Credit Adjustment is not as expected. Expected the Usable balance to be  {memberUsableBalanceBeforeTxn + pointsForManualAdjustment} but  was {memberUsableBalanceAfterAdjustment}.");

                memberUsableBalanceBeforeTxn = memberUsableBalanceAfterAdjustment;
            }

            #endregion

            DateTime randomFutureDate = DataGenerator.RandomFutureDate(+365);

            string partnerCode = "DF";
            string productName = "VAA Duty Free";
            string activityDate = randomFutureDate.ToString("dd-MMM-yyyy");
            string comments = "Test Transaction Created through Automation";
            int tierPoints = 0;
            long points = Convert.ToInt64(DataGenerator.RandomNumber(0, (int)memberUsableBalanceBeforeTxn, true));
            string productDescription = "VAA Duty Free shopping";
            string partnerName = "Duty Free";
            string partnerCategory = "Ground Partner";
            string productCategory = "Manual Adjustment";
            string transactionSource = "RetailTherapy";
            string activityType = "Redemption";

            #region Create Burn Transaction API Call

            HttpResponseMessage burnTxnResponse = Microservice.CreateTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, customerId, memberNumber, partnerCode, productName,
                                                activityDate, comments, tierPoints, (int)points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannelId, productDescription, activityType).EnsureIsAcceptedStatusCode();

            string responseUrl = burnTxnResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualBurnTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualBurnTxnResponse?.Content?.ReadAsStringAsync();

            string partnerTransactionId = JsonDocument.Parse(JsonDocument.Parse(content).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCreated = !partnerTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCreated, $"Burn Transaction creation failed for the member {memberNumber}. The response returned is \n {actualBurnTxnResponse}");

            #endregion

            #region Get Member Point Balance API Call After Burn Transaction Creation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            retry = 5;

            while (retry > 0 && memberUsableBalanceAfterTxn <= memberUsableBalanceBeforeTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                memberUsableBalanceAfterTxn = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterTxn == memberUsableBalanceBeforeTxn - points,
                $"The Member- {memberNumber} - Point balance did not match after the successful Burn transaction Creation.Expected point balance to be  {memberUsableBalanceBeforeTxn - points} but found {memberUsableBalanceAfterTxn}");

            #endregion

            await Task.Delay(2000);

            #region Cancellation of the Created Transaction

            string cancellationReason = "Payment Failure";

            HttpResponseMessage cancelAPIResponse = Microservice.CancelTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber, partnerTransactionId,
                                    randomFutureDate.ToString("yyyy/MM/dd"), transactionChannelId, cancellationReason, createdBy);

            int retrycount = 5;

            while (retrycount > 0 && (int)cancelAPIResponse.StatusCode != 202)
            {
                await Task.Delay(2000);

                cancelAPIResponse = Microservice.CancelTransaction(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber, partnerTransactionId,
                                     randomFutureDate.ToString("yyyy/MM/dd"), transactionChannelId, cancellationReason, createdBy);

                retrycount--;
            }

            Assert.True((int)cancelAPIResponse.StatusCode == 202, $"Cancel Transaction POST url returned \n {await cancelAPIResponse.Content.ReadAsStringAsync()}");

            string CancelResponseUrl = cancelAPIResponse?.Headers?.Location?.ToString();

            HttpResponseMessage cancelTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, CancelResponseUrl);

            retrycount = 5;

            while (retrycount > 0 && (int)cancelTxnResponse.StatusCode != 200)
            {
                await Task.Delay(2000);

                cancelTxnResponse = Microservice.GetTransactionResponse(_tokenFixture.GetToken("VRedMS"), ocpSubKey, CancelResponseUrl);

                retrycount--;
            }

            Assert.True(cancelTxnResponse.IsSuccessStatusCode, $"Cancel Transaction Get Response url returned \n {await cancelTxnResponse.Content.ReadAsStringAsync()}");

            string cancelResult = await cancelTxnResponse?.Content?.ReadAsStringAsync();

            string cancelledTransactionId = JsonDocument.Parse(JsonDocument.Parse(cancelResult).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

            bool isTransactionCancelled = !cancelledTransactionId.IsNullOrEmpty();

            Assert.True(isTransactionCancelled, $"Burn Transaction -{partnerTransactionId} Cancellation failed for the member {memberNumber}. The response returned is \n {cancelResult}");

            #endregion

            #region Get Member Point Balance API Call After Earn Transaction Cancellation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterCancellation = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

            retry = 5;

            while (retry > 0 && memberUsableBalanceAfterCancellation <= memberUsableBalanceAfterTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

                memberUsableBalanceAfterCancellation = string.IsNullOrEmpty(result?.Usable?.ToString()) ? 0 : Convert.ToInt64(result?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterCancellation == memberUsableBalanceBeforeTxn,
                $"The Member- {memberNumber} - Point balance did not match after the successful Earn transaction Cancellation.Expected point balance to be  {memberUsableBalanceBeforeTxn} but found {memberUsableBalanceAfterCancellation}");

            #endregion

            #region Validation of both Original and Compensatory Transactions in the Member Transaction Statement

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), ocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            string response = await memberTxnStatement?.Content?.ReadAsStringAsync();

            VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(response);

            bool isCancelledBurnTransactionPresent = false;

            IEnumerable<Statement> statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

            #region Validation of the Cancelled Redemption/Burn Transaction in the VRed Txn Statement

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.transactionId == partnerTransactionId)
                {
                    isCancelledBurnTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Redemption",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Redemption but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Product",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == productDescription,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Cancelled",
                     $"For Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Cancelled but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    Assert.True(statement.Details.cancellationReason == cancellationReason,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The Cancellation Reason does not match. Expected {cancellationReason} but found {statement.Details.cancellationReason}");

                    break;
                }
            }

            Assert.True(isCancelledBurnTransactionPresent,
                $"For the Cancelled TransactionId {partnerTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion

            #region Validation of the Compensatory Accrual/Earn Transaction in the VRed Txn Statement

            bool isCompensatoryEarnTransactionPresent = false;

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.cancelledTxnId == partnerTransactionId)
                {
                    isCompensatoryEarnTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == activityDate,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityDate does not match. Expected {activityDate} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activityType == "Accrual",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivityType does not match. Expected Accrual but found {statement.Details.activityType}");

                    Assert.True(statement.Details.activitySubtype == "Redemption Cancellation",
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The ActivitySubType does not match. Expected Redemption Cancellation but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.comments == comments,
                        $"For Txn Id {partnerTransactionId} In the VRed Statement, The Comments does not match. Expected {comments} but found {statement.Details.comments}");

                    Assert.True(statement.Details.createdBy == createdBy,
                       $"For Txn Id {partnerTransactionId} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCategory == partnerCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerCategory does not match. Expected {partnerCategory} but found {statement.Details.partnerCategory}");

                    Assert.True(statement.Details.partnerName == partnerName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The partnerName does not match. Expected {partnerName} but found {statement.Details.partnerName}");

                    Assert.True(statement.Details.productCategory == productCategory,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productCategory does not match. Expected {productCategory} but found {statement.Details.productCategory}");

                    Assert.True(statement.Details.productName == productName,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.points == points,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {points} but found {statement.Details.points}");

                    Assert.True(statement.Details.statementText == "Cancelled: " + productDescription,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The StatementText does not match. Expected {productDescription} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.status == "Processed",
                     $"For Txn Id {partnerTransactionId} In the VRed Statement, The Status does not match. Expected Processed but found {statement.Details.status}");

                    Assert.True(statement.Details.transactionChannelId == transactionChannelId,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The points does not match. Expected {transactionChannelId} but found {statement.Details.transactionChannelId}");

                    Assert.True(statement.Details.cancellationReason.IsNullOrEmpty(),
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The Cancellation Reason does not match. Expected  but found {statement.Details.cancellationReason}");

                    Assert.True(statement.Details.cancelledTxnId == partnerTransactionId,
                      $"For Txn Id {partnerTransactionId} In the VRed Statement, The Cancelled Txn Id (The Burn Txn Id) does not match. Expected {partnerTransactionId} but found {statement.Details.cancelledTxnId}");

                    break;
                }
            }

            Assert.True(isCompensatoryEarnTransactionPresent,
                $"For the Compensatory TransactionId {cancelledTransactionId} for the Member {memberNumber} The transaction details not found in VRed Statement.");

            #endregion

            #endregion


            #region Audit transaction in  avateunsqldbvglctransaction SQLDB Validations


            IEnumerable<PowerAppTransactionAuditInfo> getPowerAppTransactionAudit = AzureVglcTransactionDBHelper.PowerAppTransactionAuditInfo(memberNumber);

            Assert.True(getPowerAppTransactionAudit.Any(), $"No audit records returned from VGLC DB for member number {memberNumber}");

            PowerAppTransactionAuditInfo powerAppTransactionAudit = getPowerAppTransactionAudit.FirstOrDefault(x => x.TransactionType == "Burn") ?? null;

            Assert.True(powerAppTransactionAudit is not null, "Original Burn transaction not found in Audit Info Table");

            Assert.True(!string.IsNullOrEmpty(powerAppTransactionAudit.AuditId.ToString()), $"Audit ID is not present in the SQL DB for the record {powerAppTransactionAudit.AuditId.ToString()}");

            Assert.True(powerAppTransactionAudit.TransactionID.Equals(partnerTransactionId), $"Expected transaction id is {partnerTransactionId} but found {powerAppTransactionAudit.TransactionID}");

            Assert.True(powerAppTransactionAudit.Status.Equals("Cancelled"), $"Expected transaction Status to be Cancelled but found {powerAppTransactionAudit.Status}");

            DateTime expectedActivityDate = DateTime.ParseExact(activityDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DateTime actualActivityDate = DateTime.ParseExact(powerAppTransactionAudit.ActivityDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            Assert.True(actualActivityDate == expectedActivityDate, $"Expected activity date is {expectedActivityDate:MM/dd/yyyy} but found {powerAppTransactionAudit.ActivityDate}");

            #endregion


        }
    }
}

using System.Globalization;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsFrontEndAPITests
{

    [Collection("TransactionFEAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class TransactionFEAPITests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public TransactionFEAPITests(CosmosFixture cosmosFixture)
        {

            this.cosmosFixture = cosmosFixture;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new();

        }

        [Trait("Category", Traits.LOOPSTransactionFEAPIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveTransactionHistoryUsingCustomerId()
        {

            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = txnDoc.CustomerId;

            string memberNumber = txnDoc.MemberNumber;

            string queryForAllTransactionDoc = "SELECT Top 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey= @customerId order by c.TransactionDate desc";

            QueryDefinition parameterizedAllTransactionsQuery = new QueryDefinition(query: queryForAllTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId);

            IEnumerable<Transaction> AllTransactionsInCosmos = cosmosFixture.CosmosQueryItems<Transaction>(parameterizedAllTransactionsQuery).OrderByDescending(x => x.TransactionDate);

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.RetrieveAllTransactions(10, memberNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            IEnumerable<TransactionFrontEndAPI> transactionResponse = JsonConvert.DeserializeObject<List<TransactionFrontEndAPI>>(result);

            TransactionFrontEndAPI[] transactionResp = transactionResponse.Where(x => long.TryParse(x.transactionID, out _)).OrderByDescending(x => x.transactionProcessedDate).ToArray();

            #endregion

            #region Validations

            int i = 0;

            foreach (Transaction transactiondoc in AllTransactionsInCosmos)
            {
                string docId = transactiondoc.Id;

                Assert.True(transactiondoc.TransactionId == transactionResp[i].transactionID,
                 $"For the Doc Id {docId} ,Expected the Transaction from the transaction Doc in cosmos to be {transactiondoc.TransactionId} but found {transactionResp[i].transactionID} ");

                string cosmosActvityDate = transactiondoc.ActivityDate.HasValue ? transactiondoc.ActivityDate.Value.ToString("dd-MMM-yyyy") : "";

                string actualActivityDate = Convert.ToDateTime(transactionResp[i].activityDate).ToString("dd-MMM-yyyy");

                Assert.True(cosmosActvityDate == actualActivityDate,
                   $"For the Doc Id {docId} ,Expected the activity Date from the transaction Doc in cosmos to be {cosmosActvityDate} but found {actualActivityDate} ");

                Assert.True(transactiondoc.TransactionType == transactionResp[i].transactionType,
                   $"For the Doc Id {docId} ,Expected the transactionType from the transaction Doc in cosmos to be {transactiondoc.TransactionType} but found {transactionResp[i].transactionType} ");

                string cosmostransactionDate = transactiondoc.TransactionDate.HasValue ? transactiondoc.TransactionDate.Value.ToString("dd-MMM-yyyy") : "";

                string actualtransactionProcessedDate = Convert.ToDateTime(transactionResp[i].transactionProcessedDate).ToString("dd-MMM-yyyy");

                Assert.True(cosmostransactionDate == actualtransactionProcessedDate,
                               $"For the Doc Id {docId} ,Expected the TransactionDate from the transaction Doc in cosmos to be {cosmostransactionDate} but found {actualtransactionProcessedDate} ");

                Assert.True(transactiondoc.StatementText == transactionResp[i].statementDescription,
                                   $"For the Doc Id {docId} ,Expected the StatementText from the transaction Doc in cosmos to be {transactiondoc.StatementText} but found {transactionResp[i].statementDescription} ");

                Assert.True(transactiondoc.StatusCode == transactionResp[i].transactionStatus,
                                   $"For the Doc Id {docId} ,Expected the transactionStatus from the transaction Doc in cosmos to be {transactiondoc.StatusCode} but found {transactionResp[i].transactionStatus} ");

                i++;
            }
            #endregion
        }


        [Trait("Category", Traits.LOOPSTransactionFEAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_RetrieveTransactionSubsetBasedOnFullHistoryUsingCustomerId()
        {

            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = txnDoc.CustomerId;

            string memberNumber = txnDoc.MemberNumber;

            string queryForAllTransactionDoc = "SELECT Top 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey= @customerId order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedAllTransactionsQuery = new QueryDefinition(query: queryForAllTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId);

            IEnumerable<Transaction> AllTransactionsInCosmos = cosmosFixture.CosmosQueryItems<Transaction>(parameterizedAllTransactionsQuery).OrderByDescending(x => x.TransactionId);

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.RetrieveSubsetOfTransactionUsingPagination(memberNumber, 0, 10, "filter").EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            TransactionSubset transactionResponse = JsonConvert.DeserializeObject<TransactionSubset>(result);

            Assert.True(transactionResponse.items is not null, $"No transaction found for the member {memberNumber} . The response was {result}");

            Item[] transactionsResp = transactionResponse.items.Where(x => !string.IsNullOrEmpty(x.transactionID) && long.TryParse(x.transactionID, out _)).OrderByDescending(x => x.transactionID).ToArray();

            #endregion

            #region Validations

            int i = 0;

            foreach (Transaction transactiondoc in AllTransactionsInCosmos)
            {
                string docId = transactiondoc.Id;

                Assert.True(transactiondoc.TransactionId == transactionsResp[i].transactionID,
                    $"For the Doc Id {docId} ,Expected the transactionID from the transaction Doc in cosmos to be {transactiondoc.TransactionId} but found {transactionsResp[i].transactionID} ");

                string cosmosActvityDate = transactiondoc.ActivityDate.HasValue ? transactiondoc.ActivityDate.Value.ToString("dd-MMM-yyyy") : "";

                string actualActivityDate = Convert.ToDateTime(transactionsResp[i].activityDate).ToString("dd-MMM-yyyy");

                Assert.True(cosmosActvityDate == actualActivityDate,
                   $"For the Doc Id {docId} ,Expected the activity Date from the transaction Doc in cosmos to be {cosmosActvityDate} but found {actualActivityDate} ");

                Assert.True(transactiondoc.TransactionId == transactionsResp[i].transactionID,
                   $"For the Doc Id {docId} ,Expected the member number from the transaction Doc in cosmos to be {transactiondoc.TransactionId} but found {transactionsResp[i].transactionID} ");

                Assert.True(transactiondoc.TransactionType == transactionsResp[i].transactionType,
                   $"For the Doc Id {docId} ,Expected the transactionType from the transaction Doc in cosmos to be {transactiondoc.TransactionType} but found {transactionsResp[i].transactionType} ");

                string cosmostransactionDate = transactiondoc.TransactionDate.HasValue ? transactiondoc.TransactionDate.Value.ToString("dd-MMM-yyyy") : "";

                string actualtransactionProcessedDate = Convert.ToDateTime(transactionsResp[i].transactionProcessedDate).ToString("dd-MMM-yyyy");

                Assert.True(cosmostransactionDate == actualtransactionProcessedDate,
                               $"For the Doc Id {docId} ,Expected the TransactionDate from the transaction Doc in cosmos to be {cosmostransactionDate} but found {actualtransactionProcessedDate} ");

                Assert.True(transactiondoc.StatementText == transactionsResp[i].statementDescription,
                                   $"For the Doc Id {docId} ,Expected the StatementText from the transaction Doc in cosmos to be {transactiondoc.StatementText} but found {transactionsResp[i].statementDescription} ");

                Assert.True(transactiondoc.StatusCode == transactionsResp[i].transactionStatus,
                                   $"For the Doc Id {docId} ,Expected the transactionType from the transaction Doc in cosmos to be {transactiondoc.StatusCode} but found {transactionsResp[i].transactionStatus} ");

                i++;
            }
            #endregion
        }

        [Trait("Category", Traits.LOOPSTransactionFEAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_RetrieveTransactionBasedOnTransactionNumber()
        {

            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = txnDoc.CustomerId;

            string memberNumber = txnDoc.MemberNumber;

            string transactionId = txnDoc.TransactionId;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.RetrieveTransactionWithMemberNumberAndTxnId(memberNumber, transactionId).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            TransactionFrontEndAPI transactionResponse = JsonConvert.DeserializeObject<TransactionFrontEndAPI>(result.ToString());

            #endregion

            #region Validations


            string docId = txnDoc.Id;

            Assert.True(txnDoc.TransactionId == transactionResponse.transactionID,
                $"For the Doc Id {docId} ,Expected the transactionID from the transaction Doc in cosmos to be {txnDoc.TransactionId} but found {transactionResponse.transactionID} ");

            string cosmosActvityDate = txnDoc.ActivityDate.HasValue ? txnDoc.ActivityDate.Value.ToString("dd-MMM-yyyy") : "";

            string actualActivityDate = Convert.ToDateTime(transactionResponse.activityDate).ToString("dd-MMM-yyyy");

            Assert.True(cosmosActvityDate == actualActivityDate,
               $"For the Doc Id {docId} ,Expected the activity Date from the transaction Doc in cosmos to be {cosmosActvityDate} but found {actualActivityDate} ");

            Assert.True(txnDoc.TransactionId == transactionResponse.transactionID,
               $"For the Doc Id {docId} ,Expected the member number from the transaction Doc in cosmos to be {txnDoc.TransactionId} but found {transactionResponse.transactionID} ");

            Assert.True(txnDoc.TransactionType == transactionResponse.transactionType,
               $"For the Doc Id {docId} ,Expected the transactionType from the transaction Doc in cosmos to be {txnDoc.TransactionType} but found {transactionResponse.transactionType} ");

            string cosmostransactionDate = txnDoc.TransactionDate.HasValue ? txnDoc.TransactionDate.Value.ToString("dd-MMM-yyyy") : "";

            string actualtransactionProcessedDate = Convert.ToDateTime(transactionResponse.transactionProcessedDate).ToString("dd-MMM-yyyy");

            Assert.True(cosmostransactionDate == actualtransactionProcessedDate,
                           $"For the Doc Id {docId} ,Expected the TransactionDate from the transaction Doc in cosmos to be {cosmostransactionDate} but found {actualtransactionProcessedDate} ");

            Assert.True(txnDoc.StatementText == transactionResponse.statementDescription,
                               $"For the Doc Id {docId} ,Expected the StatementText from the transaction Doc in cosmos to be {txnDoc.StatementText} but found {transactionResponse.statementDescription} ");

            Assert.True(txnDoc.StatusCode == transactionResponse.transactionStatus,
                               $"For the Doc Id {docId} ,Expected the transactionType from the transaction Doc in cosmos to be {txnDoc.StatusCode} but found {transactionResponse.transactionStatus} ");

            #endregion
        }


        [Trait("Category", Traits.LOOPSTransactionFEAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_CancelTxnSuccessScenario()
        {

            #region Pre-requisite test data with a member with txns synced at VRED end. Creating a credit adjustment via FEAPI to sync at VRED end

            Dictionary<string, string> queryResponse = await preRequisiteTestData.CreateCreditAdjustmentTxnFromFrontEndApi();

            string transactionId = queryResponse.GetValueOrDefault("transactionId");

            string memberNumber = queryResponse.GetValueOrDefault("memberNumber");

            string customerId = queryResponse.GetValueOrDefault("customerId");

            #endregion

            #region API Call to fetch the attributes for creating the txn cancellation request

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.RetrieveTransactionWithMemberNumberAndTxnId(memberNumber, transactionId).EnsureSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            TransactionFrontEndAPI transactionResponse = JsonConvert.DeserializeObject<TransactionFrontEndAPI>(result.ToString());

            int transactionID_AWS;

            bool isTransactionID_AWSNotNull = Int32.TryParse(transactionResponse.transactionID_AWS, out transactionID_AWS);

            int retry = 3;

            while (retry > 0 && !isTransactionID_AWSNotNull)
            {
                await Task.Delay(2000);

                response = LoopsFrontEndAPIEndpoints.RetrieveTransactionWithMemberNumberAndTxnId(memberNumber, transactionId).EnsureSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                transactionResponse = JsonConvert.DeserializeObject<TransactionFrontEndAPI>(result.ToString());

                isTransactionID_AWSNotNull = Int32.TryParse(transactionResponse.transactionID_AWS, out transactionID_AWS);

                retry--;
            }

            string activityDate = transactionResponse.activityDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string transactionProcessedDate = transactionResponse.transactionProcessedDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string points = transactionResponse.points.ToString();

            string? totalMiles = transactionResponse.totalMiles.ToString();

            #endregion

            #region API call for transaction cancellation

            HttpResponseMessage cancellationResponse = LoopsFrontEndAPIEndpoints.CancellationTransaction(customerId, transactionId, transactionID_AWS, memberNumber, activityDate,
                                                                                            transactionProcessedDate, totalMiles, points).EnsureSuccessStatusCode();

            string canceltxnResult = await cancellationResponse.Content.ReadAsStringAsync();

            Assert.True(canceltxnResult == "Request processed successfully",
                $"Txn was not cancelled at vred end and in LOOPS for memberNumber: {memberNumber} and txnID: {transactionId}");

            #endregion

            #region cosmos validations for cancelled tranaction

            string queryForCancelledTransaction = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionId = @transactionId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCancelledTransaction).WithParameter("@docType", "Transaction").WithParameter("@transactionId", transactionId);

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string statusCode = txnDoc.StatusCode;

            Assert.True(statusCode == "Cancelled",
               $"Txn status was not Cancelled at LOOPS end and it shows as {statusCode} for memberNumber: {memberNumber} and txnID: {transactionId}");

            #endregion



        }


    }
}



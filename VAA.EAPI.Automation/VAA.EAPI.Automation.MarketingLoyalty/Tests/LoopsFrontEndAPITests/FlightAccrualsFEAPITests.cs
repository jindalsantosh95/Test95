using System.Globalization;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsFrontEndAPITests
{
    [Collection("FlightAccrualsFEAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class FlightAccrualsFEAPITests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {

        private readonly CosmosFixture cosmosFixture;
        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public FlightAccrualsFEAPITests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture = tokenFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        [Trait("Category", Traits.LOOPSFlightAccrualsFEAPIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("VS", "J", "LHR", "301", "DEL")]
        [InlineData("VS", "A", "LHR", "001", "ATL")]
        public async Task TC01_AirlineFlightAccuralSuccessScenario(string airlineCode, string bookingClass, string originAirport, string flightNumber, string destinationAirport)
        {

            #region Test Data Retrieval of Customer ID

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember FlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(FlyingClubDocument is not null, "Failed to Retrieve Test Data .");

            string customerId = FlyingClubDocument.CustomerId;

            string memberNumber = FlyingClubDocument.MemberNumber;

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            DateTime activityPastDate = ticketIssuePastDate.AddDays(2);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

            string comments = "Testing Only";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode,
                                                                                          ticketIssueDate, originAirport, flightNumber, destinationAirport,
                                                                                          memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

            Assert.True(flightAccrualResponse is not null, $"Failed to flightAccrualResponse from the API endpoint. The Response was {result} ");

            Assert.True(flightAccrualResponse.transactionId is not null, $"TransactionId retrieved from flightAccrualResponse API is null. The Response was {result}");

            string transactionIdInResponse = flightAccrualResponse.transactionId;

            #endregion

            #region Retrieving transaction created from the cosmos DB

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId";

            QueryDefinition txnQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionIdInResponse);

            Transaction CustomerTxnDocument = cosmosFixture.CosmosQueryItem<Transaction>(txnQuery);

            Assert.True(CustomerTxnDocument is not null, $"Failed to Retrieve Flight Accrual txn for txn Id- {transactionIdInResponse} from the cosmos DB .");


            #endregion 

            #region Validations for the Transaction created

            Assert.True(transactionIdInResponse.Equals(CustomerTxnDocument.TransactionId),
                    $"Expected the transaction number {transactionIdInResponse} not found in the cosmos DB");

            Assert.True(flightAccrualResponse.customerId.Equals(CustomerTxnDocument.CustomerId),
              $"Member customerId in the Response {flightAccrualResponse.customerId} not found in the cosmos DB");

            Assert.True(flightAccrualResponse.code is null,
               $"Expected the code attribute in response as null, but returned {flightAccrualResponse.code}");

            Assert.True(flightAccrualResponse.message is null,
               $"Expected the message attribute in response as null, but returned {flightAccrualResponse.message}");

            #endregion
        }


        [Trait("Category", Traits.LOOPSFlightAccrualsFEAPIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("VS", "J", "LHR", "301", "DEL")]
        public async Task TC02_AirlineFlightAccuralDuplicateTxnScenario(string airlineCode, string bookingClass, string originAirport, string flightNumber, string destinationAirport)
        {

            #region Test Data Retrieval of Customer ID

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember FlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(FlyingClubDocument is not null, "Failed to Retrieve Test Data .");

            string customerId = FlyingClubDocument.CustomerId;

            string memberNumber = FlyingClubDocument.MemberNumber;

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            DateTime activityPastDate = ticketIssuePastDate.AddDays(2);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string bookingReference = DataGenerator.RandomString(6, true);

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string ticketNumber = DataGenerator.RandomNumber(10000000, 99999999, false);

            string comments = "Testing Only";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode,
                                                                                          ticketIssueDate, originAirport, flightNumber, destinationAirport,
                                                                                          memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

            Assert.True(flightAccrualResponse is not null, $"Failed to flightAccrualResponse from the API endpoint. message returned: {flightAccrualResponse.message} ");

            string transactionIdInResponse = flightAccrualResponse.transactionId;

            Assert.True(transactionIdInResponse is not null,
                   $"Transaction ID returned in the response is null ");

            #endregion

            #region Retrieving transaction created from the cosmos DB

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId";

            QueryDefinition txnQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionIdInResponse);

            Transaction CustomerTxnDocument = cosmosFixture.CosmosQueryItem<Transaction>(txnQuery);

            Assert.True(CustomerTxnDocument is not null, $"Failed to Retrieve Flight Accrual txn for txn Id- {transactionIdInResponse} from the cosmos DB .");

            #endregion

            #region Creation of duplicate transaction for the same journey

            HttpResponseMessage duplicateResponse = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode,
                                                                              ticketIssueDate, originAirport, flightNumber, destinationAirport,
                                                                              memberNumber, customerId, createdOn, comments, ticketNumber);


            string newResult = await duplicateResponse.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualDuplicateResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(newResult);

            Assert.True(flightAccrualDuplicateResponse is not null,
                $"flightAccrualResponse is null for duplicate scenario. The response returned is {flightAccrualDuplicateResponse}");

            string transactionIdInNewResponse = flightAccrualDuplicateResponse.transactionId;

            #endregion

            #region Validations for the Transaction created

            Assert.True(transactionIdInNewResponse is null,
                   $"Transaction ID returned in the response was expected to be null but it is returned {transactionIdInNewResponse}");

            Assert.True(flightAccrualDuplicateResponse.customerId is null,
              $"Member customerId in the Response {flightAccrualDuplicateResponse.customerId} not found in the cosmos DB");

            Assert.True(flightAccrualDuplicateResponse?.code.Equals("806"),
               $"Expected the code attribute in response as 806, but returned {flightAccrualDuplicateResponse.code}");

            Assert.True(flightAccrualDuplicateResponse?.message.Equals("Duplicate Activity Found"),
               $"Expected the message attribute in response as \"Duplicate Activity Found\" but returned {flightAccrualDuplicateResponse.message}");

            // count of txn documents created in the cosmos db

            string activityDateForCosmosFormat = activityPastDate.ToString("yyyy-MM-dd");

            string queryForTxnDocs = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @custId and c.ActivityDate >= @activityDt and c.FlightDetails.OriginAirportcode = @OrignAirport";

            QueryDefinition parametrizedForAllTxnDocsQuery = new QueryDefinition(query: queryForTxnDocs).WithParameter("@docType", "Transaction").WithParameter("@custId", customerId).WithParameter("@activityDt", activityDateForCosmosFormat).WithParameter("@OrignAirport", originAirport);

            IEnumerable<Transaction> AllTransactionsInCosmos = cosmosFixture.CosmosQueryItems<Transaction>(parametrizedForAllTxnDocsQuery).OrderByDescending(x => x.TransactionId);

            Assert.True(AllTransactionsInCosmos.Count().Equals(1),
                       $"Duplicate txns found as count of txns {transactionIdInNewResponse} for same activity date {activityDateForCosmosFormat} for same Origin Airport {originAirport} retrieved is > 1 ");

            #endregion
        }


        [Trait("Category", Traits.LOOPSFlightAccrualsFEAPIRegression)]
        [Theory, RunOrder(3)]
        [InlineData("VS", "J", "LHR", "301", "DEL")]
        public async Task TC03_AirlineFlightAccuralActivityDateInFutureScenario(string airlineCode, string bookingClass, string originAirport, string flightNumber, string destinationAirport)
        {

            #region Test Data Retrieval of Customer ID

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember FlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(FlyingClubDocument is not null, "Failed to Retrieve Test Data .");

            string customerId = FlyingClubDocument.CustomerId;

            string memberNumber = FlyingClubDocument.MemberNumber;

            string ticketIssueDate = DataGenerator.RandomPastDate(150).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string activityDate = DataGenerator.RandomFutureDate(5).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string bookingReference = DataGenerator.RandomString(6, true);

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

            string comments = "Testing Only";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode,
                                ticketIssueDate, originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

            Assert.True(flightAccrualResponse is not null, $"Failed to flightAccrualResponse from the API endpoint. The response was {result}");

            Assert.True(flightAccrualResponse.transactionId is null, $"For the ActivityDate {activityDate} TransactionId retrieved from flightAccrualResponse API is not null.The response was {result}");

            #endregion


            #region Validations for the Transaction created

            Assert.True(flightAccrualResponse.code?.Equals("6204"),
               $"Expected the code attribute in response as \"6204\", but returned {flightAccrualResponse.code}");

            Assert.True(flightAccrualResponse.message?.Equals("Activity Date in Future"),
               $"Expected the message attribute in response as \"Activity Date in Future\", but returned {flightAccrualResponse.message}");

            #endregion
        }

    }
}

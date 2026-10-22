using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    [Collection("HighFiveRewardTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class HighFiveRewardTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public Random random = new();

        public HighFiveRewardTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture = tokenFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<MemberCustomAttributes>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        [Trait("Category", "HighFiveRewardTests")]
        [Theory, RunOrder(1)]
        [InlineData("VS", "J", "LHR", "301", "DEL")]
        public async Task TC01_ValidationOfVSFlightActivityYearCounter(string airlineCode, string bookingClass, string originAirport, string flightNumber, string destinationAirport)
        {

            #region Test Data Retrieval of Customer ID

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC").WithParameter("@status", "Active");

            FlyingClubMember FlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(FlyingClubDocument is not null, "Failed to Retrieve Test Data .");

            string customerId = FlyingClubDocument.CustomerId;

            string memberNumber = FlyingClubDocument.MemberNumber;

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy");

            DateTime activityPastDate = ticketIssuePastDate.AddDays(2);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

            string comments = "Testing Only";

            #endregion

            #region Validation High Five Reward Year Counter in Member Custom Attributes Document Before Transaction Creation

            string queryForMCAttributesDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition memCusAttQuery = new QueryDefinition(query: queryForMCAttributesDoc).WithParameter("@docType", "MemberCustomAttributes").WithParameter("@customerId", customerId);

            MemberCustomAttributes memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);


            string etagBeforeTxn = memberCustomAttributesDoc?._etag ?? "";


            int activeYearCounterForTenureBenefitsBeforeTxn = int.TryParse(memberCustomAttributesDoc?.ActiveYearCounterForTenureBenefits, out int val) ? val : 0;

            int totalActiveYearBeforeTxn = int.TryParse(memberCustomAttributesDoc?.TotalActiveYear, out int x) ? x : 0;

            MemberLastFiveVSActivityYear[] memberLastFiveVSActivityYearBeforeTxn = memberCustomAttributesDoc?.MemberLastFiveVSActivityYear ?? Array.Empty<MemberLastFiveVSActivityYear>();

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode, ticketIssueDate, originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

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

            #region Validation High Five Reward Year Counter in Member Custom Attributes Document After Transaction Creation

            memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);

            int retries = 10;

            int delayinMilliseconds = 3000;

            while ((retries > 0 && memberCustomAttributesDoc is null) || memberCustomAttributesDoc._etag == etagBeforeTxn)
            {
                await Task.Delay(delayinMilliseconds); // Wait for 3 seconds before retrying
                memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);
                retries--;
            }

            Assert.True(memberCustomAttributesDoc is not null, $"Failed to Retrieve Member Custom Attributes Document for Customer Id- {customerId} from the cosmos DB after {retries * delayinMilliseconds} milliSeconds.");


            Assert.True(activeYearCounterForTenureBenefitsBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits),
                $"ActiveYearCounterForTenureBenefits expected value is {activeYearCounterForTenureBenefitsBeforeTxn + 1} but found {memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits} after the transaction creation.");

            Assert.True(totalActiveYearBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.TotalActiveYear), $"totalActiveYearCounter expected value is {totalActiveYearBeforeTxn + 1} but found {memberCustomAttributesDoc.TotalActiveYear} after the transaction creation.");

            Assert.True(memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Length == memberLastFiveVSActivityYearBeforeTxn.Length + 1,
                $"MemberLastFiveVSActivityYear expected count is {memberLastFiveVSActivityYearBeforeTxn.Length + 1} but found {memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Length} after the transaction creation.");

            Assert.True(memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Any(x => x.ActivityYear == activityPastDate.Year.ToString()) && memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits == "5" ? memberCustomAttributesDoc.MemberLastFiveVSActivityYear.First(x => x.ActivityYear == activityPastDate.Year.ToString()).BenefitGiven == "True" : memberCustomAttributesDoc.MemberLastFiveVSActivityYear.First(x => x.ActivityYear == activityPastDate.Year.ToString()).BenefitGiven == "False",
                $"MemberLastFiveVSActivityYear does not contain the Activity Year {activityPastDate.Year} with expected BenefitGiven after the transaction creation.");


            #endregion
        }

        [Trait("Category", "HighFiveRewardTests")]
        [Theory, RunOrder(2)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task TC02_ValidationOfVSFlightActivityYearCounter(int noOfActiveYears)
        {

            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string memberNumber = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            int[] years = { 2021, 2022, 2023, 2024, 2025 };
            int active = years.Length;

            for (int i = 0; i < noOfActiveYears && active > 0; i++)
            {
                int index = random.Next(active);  // random index in [0, active)
                int pickedYear = years[index];

                string airlineCode = "VS";
                string bookingClass = "B";
                string originAirport = Application.Origins[random.Next(0, Application.Origins.Length)];
                string destinationAirport = Application.Destinations[random.Next(0, Application.Destinations.Length)];
                string flightNumber = DataGenerator.RandomNumber(1, 999, true);

                DateTime randomDate = DataGenerator.GetRandomDateFromYear(pickedYear);

                string ticketIssueDate = randomDate.ToString("MM/dd/yyyy");

                DateTime activityPastDate = randomDate.AddDays(2);

                string activityDate = activityPastDate.ToString("MM/dd/yyyy");

                string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

                string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

                string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

                string comments = "Testing Only";

                #region Validation High Five Reward Year Counter in Member Custom Attributes Document Before Transaction Creation

                string queryForMCAttributesDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

                QueryDefinition memCusAttQuery = new QueryDefinition(query: queryForMCAttributesDoc).WithParameter("@docType", "MemberCustomAttributes").WithParameter("@customerId", customerId);

                MemberCustomAttributes memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);


                string etagBeforeTxn = memberCustomAttributesDoc?._etag ?? "";


                int activeYearCounterForTenureBenefitsBeforeTxn = int.TryParse(memberCustomAttributesDoc?.ActiveYearCounterForTenureBenefits, out int val) ? val : 0;

                int totalActiveYearBeforeTxn = int.TryParse(memberCustomAttributesDoc?.TotalActiveYear, out int x) ? x : 0;

                MemberLastFiveVSActivityYear[] memberLastFiveVSActivityYearBeforeTxn = memberCustomAttributesDoc?.MemberLastFiveVSActivityYear ?? Array.Empty<MemberLastFiveVSActivityYear>();

                #endregion

                #region API Call

                HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode, ticketIssueDate, originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

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

                #region Validation High Five Reward Year Counter in Member Custom Attributes Document After Transaction Creation

                memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);

                int retries = 10;

                int delayinMilliseconds = 3000;

                while ((retries > 0 && memberCustomAttributesDoc is null) || memberCustomAttributesDoc?._etag == etagBeforeTxn)
                {
                    await Task.Delay(delayinMilliseconds); // Wait for 3 seconds before retrying
                    memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);
                    retries--;
                }

                Assert.True(memberCustomAttributesDoc is not null, $"Failed to Retrieve Member Custom Attributes Document for Customer Id- {customerId} from the cosmos DB after {retries * delayinMilliseconds} milliSeconds.");


                Assert.True(activeYearCounterForTenureBenefitsBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits),
                    $"ActiveYearCounterForTenureBenefits expected value is {activeYearCounterForTenureBenefitsBeforeTxn + 1} but found {memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits} after the transaction creation.");

                Assert.True(totalActiveYearBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.TotalActiveYear), $"totalActiveYearCounter expected value is {totalActiveYearBeforeTxn + 1} but found {memberCustomAttributesDoc.TotalActiveYear} after the transaction creation.");

                Assert.True(memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Length == memberLastFiveVSActivityYearBeforeTxn.Length + 1,
                    $"MemberLastFiveVSActivityYear expected count is {memberLastFiveVSActivityYearBeforeTxn.Length + 1} but found {memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Length} after the transaction creation.");

                Assert.True(memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Any(x => x.ActivityYear == activityPastDate.Year.ToString()) && memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits == "5" ? memberCustomAttributesDoc.MemberLastFiveVSActivityYear.First(x => x.ActivityYear == activityPastDate.Year.ToString()).BenefitGiven == "True" : memberCustomAttributesDoc.MemberLastFiveVSActivityYear.First(x => x.ActivityYear == activityPastDate.Year.ToString()).BenefitGiven == "False",
                    $"MemberLastFiveVSActivityYear does not contain the Activity Year {activityPastDate.Year} with expected BenefitGiven after the transaction creation.");

                #endregion

                // Swap-remove: replace picked slot with last active item, then shrink
                years[index] = years[active - 1];

                active--;

            }
        }
    }
}

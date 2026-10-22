using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("LoyaltyHighFiveRewardsDataCreation")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class LoyaltyHighFiveRewardsDataCreation : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;

        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        private readonly string customerTransactionCosmosDb;

        public LoyaltyHighFiveRewardsDataCreation(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
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

        [Trait("Category", Traits.LoyaltyHighFiveDataCreation)]
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task CreationOfHighFiveRewardTestData(int noOfActiveYears)
        {

            (string emailId, string memberNumber, string customerId, string firstName, string lastName) = await EnrolmentHelper.GraphQLEnrolment("WEB-MYA", "WEB");

            Assert.True(!string.IsNullOrEmpty(customerId), $"Expected a populated customer ID from enrolment, but found '{(string.IsNullOrEmpty(customerId) ? "<empty>" : customerId)}'.");

            DateTime today = DateTime.UtcNow.Date;
            const int firstHighFiveActivityYear = 2021;

            // High Five activity can use any unique year from the feature start through the current calendar year.
            List<int> availableYears = Enumerable.Range(firstHighFiveActivityYear, today.Year - firstHighFiveActivityYear + 1).ToList();

            List<int> years = new();

            for (int i = 0; i < noOfActiveYears; i++)
            {
                int index = Random.Shared.Next(availableYears.Count);

                years.Add(availableYears[index]);
                availableYears.RemoveAt(index);
            }

            MemberCustomAttributes memberCustomAttributesDoc = null;

            foreach (int pickedYear in years)
            {
                string airlineCode = "VS";
                string bookingClass = "B";
                string originAirport = Application.Origins[0];
                string destinationAirport = Application.Destinations.First(airport => !airport.Is(originAirport));
                string flightNumber = DataGenerator.RandomNumber(1, 999, true);

                // A selected current year uses today; earlier years use a stable mid-year date.
                DateTime activityPastDate = pickedYear == today.Year
                    ? today
                    : new DateTime(pickedYear, 6, 15);

                DateTime ticketIssueDateTime = activityPastDate.AddDays(-2);

                string ticketIssueDate = ticketIssueDateTime.ToString("MM/dd/yyyy");
                string activityDate = activityPastDate.ToString("MM/dd/yyyy");

                string bookingReference = DataGenerator.RandomString(6, true).ToUpperInvariant();

                string createdOn = today.ToString("MM/dd/yyyy");

                string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

                string comments = "Testing Only";

                #region Validation High Five Reward Year Counter in Member Custom Attributes Document Before Transaction Creation

                string queryForMCAttributesDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

                QueryDefinition memCusAttQuery = new QueryDefinition(query: queryForMCAttributesDoc).WithParameter("@docType", "MemberCustomAttributes").WithParameter("@customerId", customerId);

                memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);

                // A newly enrolled member may not have this document until the first accrual is processed.
                // Treat that as the zero-state; the post-transaction retry still requires the document to exist.
                string etagBeforeTxn = memberCustomAttributesDoc?._etag ?? string.Empty;

                int activeYearCounterForTenureBenefitsBeforeTxn = int.TryParse(memberCustomAttributesDoc?.ActiveYearCounterForTenureBenefits, out int activeYearCounter) ? activeYearCounter : 0;

                int totalActiveYearBeforeTxn = int.TryParse(memberCustomAttributesDoc?.TotalActiveYear, out int totalActiveYear) ? totalActiveYear : 0;

                #endregion

                #region API Call

                System.Net.Http.HttpResponseMessage response = Automation.Core.Helpers.Requests.Endpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate, airlineCode, ticketIssueDate, originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();

                FlightAccrualResponse? flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

                Assert.True(flightAccrualResponse is not null, $"Expected a deserializable flight-accrual response, but found '{(flightAccrualResponse is null ? "<null>" : "<populated>")}'.");

                string transactionIdInResponse = flightAccrualResponse?.transactionId ?? string.Empty;

                Assert.True(!string.IsNullOrEmpty(transactionIdInResponse), $"Expected a populated transaction ID from the flight-accrual response, but found '{(string.IsNullOrEmpty(transactionIdInResponse) ? "<empty>" : transactionIdInResponse)}'.");

                #endregion

                #region Retrieving transaction created from the cosmos DB

                string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId";

                QueryDefinition txnQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionIdInResponse);

                Transaction CustomerTxnDocument = null;

                // Cosmos is eventually consistent, so retry only this read until the API-created document is visible.
                RetryHelper.RunWithRetry(() =>
                {
                    CustomerTxnDocument = cosmosFixture.CosmosQueryItem<Transaction>(txnQuery);

                    Assert.True(CustomerTxnDocument is not null, $"Expected flight-accrual transaction '{transactionIdInResponse}' in Cosmos DB, but found no document.");
                }, 5, 2000);


                #endregion

                #region Validations for the Transaction created

                Assert.True(transactionIdInResponse.Equals(CustomerTxnDocument.TransactionId),
                        $"Expected transaction number '{transactionIdInResponse}', but found '{CustomerTxnDocument.TransactionId}'.");

                Assert.Equal(customerId, flightAccrualResponse?.customerId);

                Assert.True(flightAccrualResponse.code is null,
                   $"Expected a null response code, but found '{flightAccrualResponse.code ?? "<null>"}'.");

                Assert.True(flightAccrualResponse.message is null,
                   $"Expected a null response message, but found '{flightAccrualResponse.message ?? "<null>"}'.");

                #endregion

                #region Validation High Five Reward Year Counter in Member Custom Attributes Document After Transaction Creation

                RetryHelper.RunWithRetry(() =>
                {
                    memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);
                    Assert.True(memberCustomAttributesDoc is not null && memberCustomAttributesDoc._etag != etagBeforeTxn,
                        $"Expected an updated Member Custom Attributes document for customer ID '{customerId}', but found document present '{memberCustomAttributesDoc is not null}' with unchanged ETag '{memberCustomAttributesDoc?._etag == etagBeforeTxn}'.");
                }, 10, 3000);

                Assert.True(memberCustomAttributesDoc is not null, $"Expected a Member Custom Attributes document for customer ID '{customerId}', but found no document.");

                if (activeYearCounterForTenureBenefitsBeforeTxn < 5)

                    Assert.True(activeYearCounterForTenureBenefitsBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits),
                        $"ActiveYearCounterForTenureBenefits expected value is {activeYearCounterForTenureBenefitsBeforeTxn + 1} but found {memberCustomAttributesDoc.ActiveYearCounterForTenureBenefits} after the transaction creation for customerId \n {customerId}");

                Assert.True(totalActiveYearBeforeTxn + 1 == int.Parse(memberCustomAttributesDoc.TotalActiveYear), $"totalActiveYearCounter expected value is {totalActiveYearBeforeTxn + 1} but found {memberCustomAttributesDoc.TotalActiveYear} after the transaction creation.");

                Assert.True(memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Any(x => x.ActivityYear == activityPastDate.Year.ToString()), $"Expected activity year '{activityPastDate.Year}' in the active-year list, but found no matching year for customer ID '{customerId}'.");

                string expectedBenefitGiven = memberCustomAttributesDoc.TotalActiveYear == "5" ? "True" : "False";

                // The reward is asynchronous; poll only when this transaction is expected to earn it.
                if (expectedBenefitGiven == "True")
                {
                    RetryHelper.RunWithRetry(() =>
                    {
                        memberCustomAttributesDoc = cosmosFixture.CosmosQueryItem<MemberCustomAttributes>(memCusAttQuery);

                        MemberLastFiveVSActivityYear rewardedActivityYear = memberCustomAttributesDoc.MemberLastFiveVSActivityYear.OrderByDescending(x => int.Parse(x.ActivityYear)).First();

                        Assert.True(expectedBenefitGiven == rewardedActivityYear.BenefitGiven, $" For Member {memberNumber} , Expected the benefit given for the year {rewardedActivityYear.ActivityYear} to be {expectedBenefitGiven} but found {rewardedActivityYear.BenefitGiven}");
                    }, 5, 4000);
                }

                string benefitActivityYear = expectedBenefitGiven == "True"
                    ? memberCustomAttributesDoc.MemberLastFiveVSActivityYear.OrderByDescending(x => int.Parse(x.ActivityYear)).First().ActivityYear
                    : activityPastDate.Year.ToString();

                MemberLastFiveVSActivityYear activityYear = memberCustomAttributesDoc.MemberLastFiveVSActivityYear.Single(x => x.ActivityYear == benefitActivityYear);

                Assert.Equal(expectedBenefitGiven, activityYear.BenefitGiven);

                #endregion

            }

            // Store the final activity years in ascending order for deterministic downstream retrieval.
            string activeYears = string.Join(" ", memberCustomAttributesDoc.MemberLastFiveVSActivityYear
                .Select(x => x.ActivityYear)
                .OrderBy(activityYear => activityYear));

            await AddHighFiveAccountsToStorageTable(env, memberNumber, activeYears);
        }

        private static async Task AddHighFiveAccountsToStorageTable(string env, string MemberNumber, string activeYears)
        {
            TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "LoyaltyHouseHoldAccounts";
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new LoyaltyHouseHoldandHighFiveAccountsTableEntity()
            {
                PartitionKey = env.ToUpper(),
                RowKey = $"HighFive -  {MemberNumber}",
                ActiveYears = activeYears,
            });
        }
    }
}

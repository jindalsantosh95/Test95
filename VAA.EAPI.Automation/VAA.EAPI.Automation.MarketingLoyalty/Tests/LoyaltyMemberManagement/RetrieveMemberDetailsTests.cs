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

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetrieveMemberDetailsTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public RetrieveMemberDetailsTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndPointForCustTxnDB = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndPointForCustTxnDB, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndPointForCustTxnDB, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Award>(cosmosEndPointForCustTxnDB, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndPointForCustTxnDB, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }


        public static string wingsMemberProductId = "1634561747";

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(18)]
        public async Task A_RetrieveMemberAccountDetails_SuccessScenario()
        {

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode and c.TransactionValue > 0 order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null, "Failed to Retrieve Test Data for Credit Transaction.");

            string customerId = txnDoc.CustomerId;

            string memberNumber = txnDoc.MemberNumber;

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId";

            QueryDefinition query = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            Assert.True(flyingClubMemberDoc is not null, $"Failed to Retrieve FCM Document for the member {memberNumber}.");

            HttpResponseMessage memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
              $" but was {retrieveMemberAccountResult?.ToString()}");

            long actualRollingPointTotalBalanceBeforeAccrual = long.Parse(retrieveMemberAccountResult.data?.loyaltyMemberAccount?.rollingPointTotal?.ToString());

            string partnerCode = "VS";

            string productName = "Tier Points Adjustment";

            string activityDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

            int tierPoints = Convert.ToInt32(DataGenerator.RandomNumber(100, 200, true));

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #region Manual Adjustment API Call

            HttpResponseMessage manualCreditResponse = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, partnerCode, productName, activityDate, "Test txn", 0,
                tierPoints, "AutomationScript", createdByEmailId, createdByUserName, customerId);

            #endregion

            memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

            long actualRollingPointTotalBalanceAfterAccrual = long.Parse(retrieveMemberAccountResult.data?.loyaltyMemberAccount?.rollingPointTotal?.ToString());

            int retry = 5;

            while (manualCreditResponse.IsSuccessStatusCode && actualRollingPointTotalBalanceAfterAccrual <= actualRollingPointTotalBalanceBeforeAccrual && retry > 0)
            {
                await Task.Delay(1000);

                memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

                retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

                Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
                     $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
                     $" but was {retrieveMemberAccountResult?.ToString()}");

                actualRollingPointTotalBalanceAfterAccrual = long.Parse(retrieveMemberAccountResult.data?.loyaltyMemberAccount?.rollingPointTotal?.ToString());

                retry--;
            }

            if (manualCreditResponse.IsSuccessStatusCode)
                Assert.True(actualRollingPointTotalBalanceAfterAccrual == actualRollingPointTotalBalanceBeforeAccrual + tierPoints,
                    $"Expected the tier Point value to be {actualRollingPointTotalBalanceBeforeAccrual + tierPoints} but found {actualRollingPointTotalBalanceAfterAccrual} ");
            else
                Assert.True(actualRollingPointTotalBalanceAfterAccrual == actualRollingPointTotalBalanceBeforeAccrual,
                $"Expected the tier Point value to be equal if no points were added but found unequal");

            string? goldStartDateStr = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.goldStartDate;

            DateTime actualGoldStartDate = DateTime.TryParse(goldStartDateStr, out DateTime parsedDate) ? parsedDate
                : DateTime.MinValue;

            DateTime expectedGoldStartDate = flyingClubMemberDoc.GoldStartDate.HasValue ? flyingClubMemberDoc.GoldStartDate.Value.DateTime : DateTime.MinValue;

            Assert.True(actualGoldStartDate == expectedGoldStartDate, $"The Gold start date values does not match for the member {memberNumber}. Expected {expectedGoldStartDate} but found {actualGoldStartDate}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task B_RetrieveMemberAccountDetails_ShouldThrowErrorWhenCustomerIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"),
                "D971F4AE-68F9-84D1-E053", "");

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Customer Id.";

            Assert.True(retrieveMemberAccountResult?.ToString().Contains(expectedResponse),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {expectedResponse} " +
              $" but was {retrieveMemberAccountResult?.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task C_RetrieveMemberAccountDetails_ShouldThrowMandatoryFieldMissingErrorWhenCustomerIdIsNull()
        {
            HttpResponseMessage response = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), "null", "");

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Customer Id.";

            Assert.True(retrieveMemberAccountResult?.ToString().Contains(expectedResponse),
                $"The Retrieve Member Tier Acc Details response is not as expected. Expected {expectedResponse} " +
                $" but was {retrieveMemberAccountResult?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task D_ValidationOfWingsMemberDetails_SuccessScenario()
        {

            DateTime currentDate = DateTime.Now;

            string startDate = currentDate.AddDays(-1).ToString("yyyy-MM-dd");

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId = @productId and c.StartDate <= @startDate and c.ExpiryDate = @expiryDate order by c.StartDate desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@productId", wingsMemberProductId).WithParameter("@startDate", startDate).WithParameter("@expiryDate", "");

            Core.DB_Model.Entities.Award awardDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

            Assert.True(awardDoc is not null,
              "Failed to Retrieve Wings Member for test execution.");

            string customerId = awardDoc.CustomerId;

            string wingsStartDate = awardDoc.StartDate.ToString("dd-MMM-yyyy");

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc.MemberNumber;

            #region member account details API Call before creating any accrual transaction

            HttpResponseMessage memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
              $" but was {retrieveMemberAccountResult?.ToString()}");

            string totalTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalTierPointsSinceBecomingWings?.ToString() ?? "0";

            string totalVSTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalVSTierPointsSinceBecomingWings?.ToString() ?? "0";

            long totalTierPointsAfterBecomingWingsMemberBeforeAccrual = long.Parse(totalTierPointsSinceBecomingWings);

            long totalVSTierPointsAfterBecomingWingsMemberBeforeAccrual = long.Parse(totalVSTierPointsSinceBecomingWings);

            #endregion

            string partnerCode = "VS";

            string productName = "Tier Points Adjustment";

            string activityDate = currentDate.ToString("dd-MMM-yyyy");

            int tierPoints = Convert.ToInt32(DataGenerator.RandomNumber(100, 200, true));

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = createdByUserName + "@testemail.co.ic";

            #region Manual Adjustment API Call

            HttpResponseMessage manualCreditResponse = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, partnerCode, productName, activityDate, "Test txn", 0, tierPoints, "AutomationScript",
                                                                    createdByEmailId, createdByUserName, customerId);
            #endregion

            #region member account details API Call after creating manual adjustment transaction

            memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
              $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
              $" but was {retrieveMemberAccountResult?.ToString()}");

            totalTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalTierPointsSinceBecomingWings?.ToString() ?? "0";

            long totalTierPointsAfterBecomingWingsMemberAfterAccrual = long.Parse(totalTierPointsSinceBecomingWings);

            int retry = 5;

            while (manualCreditResponse.IsSuccessStatusCode && totalTierPointsAfterBecomingWingsMemberAfterAccrual <= totalTierPointsAfterBecomingWingsMemberBeforeAccrual && retry > 0)
            {
                await Task.Delay(1000);

                memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

                retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

                Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
                    $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
                    $" but was {retrieveMemberAccountResult?.ToString()}");

                totalTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalTierPointsSinceBecomingWings?.ToString() ?? "0";

                totalTierPointsAfterBecomingWingsMemberAfterAccrual = long.Parse(totalTierPointsSinceBecomingWings);

                retry--;
            }

            #endregion

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy");

            string txnActivityDate = DateTime.Now.ToString("MM/dd/yyyy");

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

            string comments = "Testing Only";

            string bookingClass = "J";

            string originAirport = "LHR";

            string destinationAirport = "ATL";

            string flightNumber = DataGenerator.RandomNumber(100, 300, true);

            #region Flight Accrual API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, txnActivityDate, partnerCode, ticketIssueDate, originAirport, flightNumber,
                                         destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber);

            string result = await response.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

            int faTierPoints = 200;

            if (flightAccrualResponse != null && flightAccrualResponse.message == "Duplicate Activity Found")
            {
                faTierPoints = 0;
            }

            #endregion

            #region member account details API Call after creating flight accrual transaction

            memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveMemberAccountResult.ToString().Contains(customerId),
             $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response" +
             $" but was {retrieveMemberAccountResult?.ToString()}");

            totalVSTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalVSTierPointsSinceBecomingWings?.ToString() ?? "0";

            long totalVSTierPointsAfterBecomingWingsMemberAfterAccrual = long.Parse(totalVSTierPointsSinceBecomingWings);

            retry = 5;

            while (response.IsSuccessStatusCode && totalVSTierPointsAfterBecomingWingsMemberAfterAccrual <= totalVSTierPointsAfterBecomingWingsMemberBeforeAccrual && retry > 0)
            {
                await Task.Delay(1000);

                memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

                retrieveMemberAccountResult = JsonConvert.DeserializeObject<dynamic>(await memberAccountDetailsResponse.Content.ReadAsStringAsync());

                totalVSTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalVSTierPointsSinceBecomingWings?.ToString() ?? "0";

                totalVSTierPointsAfterBecomingWingsMemberAfterAccrual = long.Parse(totalVSTierPointsSinceBecomingWings);

                retry--;
            }

            #endregion

            string actualWingsStartDate = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.wingsStartDate?.ToString();

            Assert.True(!string.IsNullOrEmpty(actualWingsStartDate), $"The Wings StartDate in the response is found null. Expected the value to be {wingsStartDate}");

            actualWingsStartDate = DateTime.Parse(actualWingsStartDate).ToString("dd-MMM-yyyy");

            totalTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalTierPointsSinceBecomingWings?.ToString() ?? "0";

            totalVSTierPointsSinceBecomingWings = retrieveMemberAccountResult.data?.loyaltyMemberAccount?.totalVSTierPointsSinceBecomingWings?.ToString() ?? "0";

            long actualTotalTierPointsAfterBecomingWingsMember = long.Parse(totalTierPointsSinceBecomingWings);

            long actualTotalVSTierPointsAfterBecomingWingsMember = long.Parse(totalVSTierPointsSinceBecomingWings);

            Assert.True(actualWingsStartDate == wingsStartDate,
                $"Expected the wings Start Date in the response to be {wingsStartDate}  but found {actualWingsStartDate} ");

            if (manualCreditResponse.IsSuccessStatusCode && response.IsSuccessStatusCode)
            {
                Assert.True(actualTotalTierPointsAfterBecomingWingsMember == totalTierPointsAfterBecomingWingsMemberBeforeAccrual + tierPoints + faTierPoints,
                    $"Expected the total TierPoints After Becoming WingsMember in the response to be {totalTierPointsAfterBecomingWingsMemberBeforeAccrual + tierPoints + faTierPoints}  but found {actualTotalTierPointsAfterBecomingWingsMember} ");
            }
            else
            {
                if (manualCreditResponse.IsSuccessStatusCode)
                    Assert.True(actualTotalTierPointsAfterBecomingWingsMember == totalTierPointsAfterBecomingWingsMemberBeforeAccrual + tierPoints,
                        $"Expected the total TierPoints After Becoming WingsMember in the response to be {totalTierPointsAfterBecomingWingsMemberBeforeAccrual + tierPoints}  but found {actualTotalTierPointsAfterBecomingWingsMember} ");

                else if (response.IsSuccessStatusCode)
                    Assert.True(actualTotalTierPointsAfterBecomingWingsMember == totalTierPointsAfterBecomingWingsMemberBeforeAccrual + faTierPoints,
                       $"Expected the total TierPoints After Becoming WingsMember in the response to be {totalTierPointsAfterBecomingWingsMemberBeforeAccrual + faTierPoints}  but found {actualTotalTierPointsAfterBecomingWingsMember} ");
                else
                    Assert.True(actualTotalTierPointsAfterBecomingWingsMember == totalTierPointsAfterBecomingWingsMemberBeforeAccrual,
                       $"Expected the total TierPoints After Becoming WingsMember in the response to be {totalTierPointsAfterBecomingWingsMemberBeforeAccrual}  but found {actualTotalTierPointsAfterBecomingWingsMember} ");

            }
            if (response.IsSuccessStatusCode)
                Assert.True(actualTotalVSTierPointsAfterBecomingWingsMember == totalVSTierPointsAfterBecomingWingsMemberBeforeAccrual + faTierPoints,
                    $"Expected the total VSTierPoints After Becoming WingsMember in the response to be {totalTierPointsAfterBecomingWingsMemberBeforeAccrual + faTierPoints}  but found {actualTotalTierPointsAfterBecomingWingsMember} ");

        }
    }
}

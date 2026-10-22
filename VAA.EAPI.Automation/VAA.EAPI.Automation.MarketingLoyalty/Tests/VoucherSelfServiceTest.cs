using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    [Collection("VoucherSelfService")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class VoucherSelfServiceTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private const string WingsAwardProductId = "1634561747";

        private readonly CosmosFixture cosmosFixture;
        private readonly TokenFixture tokenFixture;

        private static readonly string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private readonly string ocpSubKey;

        public VoucherSelfServiceTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerTransactionCosmosDb = env.Is("uat")
                ? Application.CustomerTransactionCosmosDbUAT
                : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<TransactionDocuments>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            this.tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            this.tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

        }

        [Trait("Category", "VoucherSelfServiceTests")]
        [Theory, RunOrder(1)]
        [InlineData("Flying Club reward voucher - Gold 2000 Tier Points", "1-2A1P")]
        [InlineData("Flying Club reward voucher - UK Reward credit card", "1-2MG5VE1")]
        [InlineData("Flying Club reward voucher - US BoA credit card", "1-41G2YIN")]
        public async Task TC01_RetrieveVoucher_SuccessScenario(string productName, string productId)
        {
            // Arrange: retrieve an available Award document for this product.
            string queryForAwardDocument = "SELECT * FROM c where c.DocType = @docType and c.ProductId = @productId and c.Status = 'Available' order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@productId", productId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDoc is not null, $"No Award document found for product: {productName} (ProductId: {productId}).");

            // Act: retrieve vouchers for the customer associated with that award document.
            string customerId = awardDoc.CustomerId;

            HttpResponseMessage retrieveVoucherResponse = Microservice
                .RetrieveVouchers(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId)
                .EnsureIsSuccessStatusCode();

            string content = await retrieveVoucherResponse.Content.ReadAsStringAsync();

            Assert.False(string.IsNullOrWhiteSpace(content), "RetrieveVouchers returned an empty response body.");

            using JsonDocument vouchersDoc = JsonDocument.Parse(content);

            JsonElement vouchersArray = vouchersDoc.RootElement;

            Assert.Equal(JsonValueKind.Array, vouchersArray.ValueKind);
            Assert.True(vouchersArray.GetArrayLength() > 0, "No vouchers found in the response.");

            // Arrange: retrieve customer tier and any Wings flags for tier-based validation.
            string queryForFlyingClubMemberDocument = "SELECT * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c.CreatedOn desc";

            QueryDefinition parameterizedFCMQuery = new QueryDefinition(query: queryForFlyingClubMemberDocument)
                .WithParameter("@docType", "FlyingClubMember")
                .WithParameter("@partitionKey", customerId);

            FlyingClubMember flyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedFCMQuery);

            Assert.True(flyingClubMemberDocument is not null, $"No FlyingClubMember document found for customerId {customerId}.");

            string customerTier = flyingClubMemberDocument.TierCode ?? string.Empty;

            string queryForWingsFlagCheck = "SELECT * FROM c where c.DocType = @docType and c.PartitionKey = @customerId and c.ProductId = @productId";

            QueryDefinition wingsFlagQuery = new QueryDefinition(query: queryForWingsFlagCheck)
                .WithParameter("@docType", "Award")
                .WithParameter("@customerId", customerId)
                .WithParameter("@productId", WingsAwardProductId);

            Award wingsAwardDoc = cosmosFixture.CosmosQueryItem<Award>(wingsFlagQuery);
            bool wingsFlag = wingsAwardDoc is not null;

            // Assert: validate each voucher returned by the API against the corresponding Cosmos Award document.
            string queryForAwardDocsByNumber = "SELECT * FROM c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardNumber = @awardNumber order by c.CreatedOn desc";

            foreach (JsonElement voucherElement in vouchersArray.EnumerateArray())
            {
                string voucherNumber = voucherElement.GetProperty("voucherNumber").GetString() ?? string.Empty;
                string status = voucherElement.GetProperty("status").GetString() ?? string.Empty;
                string startDate = DateTimeOffset.Parse(voucherElement.GetProperty("startDate").GetString()).ToString("yyyy-MM-dd");
                string expiryDate = DateTimeOffset.Parse(voucherElement.GetProperty("expiryDate").GetString()).ToString("yyyy-MM-dd");

                Assert.False(string.IsNullOrWhiteSpace(voucherNumber), "Voucher number is missing from the response.");

                int? maxValue = null;
                if (voucherElement.TryGetProperty("maxValue", out JsonElement maxValProp) && maxValProp.ValueKind != JsonValueKind.Null)
                {
                    maxValue = maxValProp.GetInt32();
                }

                QueryDefinition awardQuery = new QueryDefinition(query: queryForAwardDocsByNumber)
                    .WithParameter("@docType", "Award")
                    .WithParameter("@customerId", customerId)
                    .WithParameter("@awardNumber", voucherNumber);

                Award awardCosmosDoc = cosmosFixture.CosmosQueryItem<Award>(awardQuery);

                for (int attempt = 0; awardCosmosDoc is null && attempt < 5; attempt++)
                {
                    await Task.Delay(1000);
                    awardCosmosDoc = cosmosFixture.CosmosQueryItem<Award>(awardQuery);
                }

                Assert.True(awardCosmosDoc is not null, $"No Award document found in Cosmos for voucher number {voucherNumber}");

                Assert.True(
                    string.Equals(awardCosmosDoc.AwardNumber, voucherNumber, StringComparison.Ordinal),
                    $"For voucher {voucherNumber}, expected AwardNumber {voucherNumber} but found {awardCosmosDoc.AwardNumber}");

                string expectedStartDate = awardCosmosDoc.StartDate.ToString("yyyy-MM-dd");
                Assert.Equal(expectedStartDate, startDate);

                if (awardCosmosDoc.ExpiryDate != default(DateTime))
                {
                    string expectedExpiryDate = awardCosmosDoc.ExpiryDate.ToString("yyyy-MM-dd");
                    Assert.Equal(expectedExpiryDate, expiryDate);
                }

                int expectedValue;
                if (wingsFlag)
                {
                    expectedValue = 0;
                    if (status.Is("Available"))
                    {
                        expectedValue = 200000;
                    }
                    else if (status.Is("Partially Used"))
                    {
                        expectedValue = 100000;
                    }
                }
                else if (customerTier.Is("Red"))
                {
                    expectedValue = 0;
                    if (status.Is("Available"))
                    {
                        expectedValue = 75000;
                    }
                    else if (status.Is("Partially Used"))
                    {
                        expectedValue = 37500;
                    }
                }
                else if (customerTier.Is("Silver") || customerTier.Is("Gold"))
                {
                    expectedValue = 0;
                    if (status.Is("Available"))
                    {
                        expectedValue = 150000;
                    }
                    else if (status.Is("Partially Used"))
                    {
                        expectedValue = 75000;
                    }
                }
                else
                {
                    expectedValue = 0;
                }

                if (maxValue is not null)
                {
                    Assert.Equal(expectedValue, maxValue.Value);
                }
                else
                {
                    Assert.True(
                        expectedValue == 0,
                        $"For voucher {voucherNumber}, expected maxValue {expectedValue} but response omitted maxValue.");
                }
            }
        }

        [Trait("Category", "VoucherSelfServiceTests")]
        [Theory, RunOrder(2)]
        [InlineData("Reward voucher redemption - Companion")]
        [InlineData("Reward voucher redemption - Upgrade")]
        public async Task TC02_RedeemVoucher_SuccessScenario(string redemptionType)
        {
            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureIsSuccessStatusCode();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string memberNumber = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId from dbo.Product where ProductType='Electronic Voucher' and active='1' and RowId NOT IN ('1-1ZKH', '1-29QH') order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            //Voucher which have expiry date period of 2 years
            List<string> productIds = new() { "1-2CJWLQ4", "1-2CJWLQZ", "1-2CJWLRS", "1-2MG5VE1", "1-41G2YIN" };

            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            string endDate = date.AddYears(productIds.Contains(voucherProductId) ? 2 : 1).ToString("yyyy-MM-dd");

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher rewarded successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse), $"Expected {expectedResponse} but was {actualResponse} .");

            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@customerId", customerId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDoc is not null, "No available Award document found for redemption.");

            string voucherNumber = awardDoc.AwardNumber;

            DateTime futuredate = DataGenerator.RandomFutureDate(+180);
            string activityDate = futuredate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            string bookingClass = "Y";
            string originAirport = "LHR";
            string destinationAirport = "JFK";
            string bookingReference = $"{DataGenerator.RandomString(1, false).ToUpper()}{DataGenerator.RandomString(5, true).ToUpper()}";

            Random random = new Random();
            int points = random.Next(1, 75001);

            HttpResponseMessage redeemVoucherResponse = Microservice
                .RedeemVoucher(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, memberNumber, redemptionType, activityDate, bookingClass, originAirport, destinationAirport, bookingReference, voucherNumber, voucherName, points)
                .EnsureIsSuccessStatusCode();

            string headerLocation = redeemVoucherResponse.Headers.Location?.ToString();

            Assert.False(string.IsNullOrWhiteSpace(headerLocation), "The redemption response did not include a Location header.");

            string headerId = headerLocation.TrimEnd('/').Split('/').LastOrDefault();

            HttpResponseMessage redemptionResponse = Microservice.CreateTransactionResponse(tokenFixture.GetToken("VRedMS"), ocpSubKey, headerId);

            string content = await redemptionResponse.Content.ReadAsStringAsync();

            Assert.True(redemptionResponse is not null && redemptionResponse.IsSuccessStatusCode,
                $"The redemption transaction response did not succeed. Status: {(int?)redemptionResponse?.StatusCode} {redemptionResponse?.StatusCode}. Body: {content}");

            JObject redemptionResult = JsonConvert.DeserializeObject<JObject>(content);
            Assert.NotNull(redemptionResult);

            JToken valueToken = redemptionResult["value"];
            Assert.NotNull(valueToken);

            JObject inner = JObject.Parse(valueToken.ToString());

            Assert.True(memberNumber == inner["partnerUserId"]?.ToString(), "The partnerUserId in the redemption response does not match the expected member number.");

            string queryForUpdatedAwardDocument = "SELECT Top 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardNumber = @awardNumber ";

            QueryDefinition updatedAwardQuery = new QueryDefinition(query: queryForUpdatedAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardNumber", voucherNumber);

            Award updatedAwardDoc = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedAwardDoc = cosmosFixture.CosmosQueryItem<Award>(updatedAwardQuery);

                if (updatedAwardDoc is not null && updatedAwardDoc.Status == "Used" && updatedAwardDoc.UsedDate != default(DateTime))
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedAwardDoc is not null, $"No updated Award document found for voucher {voucherNumber} after redemption.");
            Assert.Equal("Used", updatedAwardDoc.Status);
            Assert.True(updatedAwardDoc.UsedDate != default(DateTime), "UsedDate was not populated after redemption.");
        }

        [Trait("Category", "VoucherSelfServiceTests")]
        [Fact, RunOrder(3)]
        public async Task TC03_VoucherRollback_SuccessScenario()
        {
            // Arrange: retrieve the most recent used Award document for any customer.
            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.Status = 'Used' order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument)
                .WithParameter("@docType", "Award");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDoc is not null, $"No Award document found");

            // Act: retrieve vouchers for the customer associated with that award document.
            string customerId = awardDoc.CustomerId;
            string voucherNumber = awardDoc.AwardNumber;
            string voucherId = awardDoc.AwardId;

            string queryForAccrualTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType order by c.CreatedOn desc";

            //Act: retrieve the Accrual and Redemption transaction documents for the voucher.
            QueryDefinition parameterizedAccrualTransactionQuery = new QueryDefinition(query: queryForAccrualTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Accrual");

            TransactionDocuments accrualTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(parameterizedAccrualTransactionQuery);

            Assert.True(accrualTransactionDocument is not null, $"No Accrual Transaction document found for voucher {voucherNumber} before rollback.");

            Assert.False(string.IsNullOrEmpty(accrualTransactionDocument.transactionId), $"Accrual transactionId was null/empty for voucher {voucherNumber} before rollback.");

            string accrualTransactionId = accrualTransactionDocument?.transactionId;

            string queryForRedemptionTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType order by c.CreatedOn desc";

            QueryDefinition parameterizedRedemptionTransactionQuery = new QueryDefinition(query: queryForRedemptionTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Redemption");

            TransactionDocuments redemptionTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(parameterizedRedemptionTransactionQuery);

            Assert.True(redemptionTransactionDocument is not null, $"No Redemption Transaction document found for voucher {voucherNumber} before rollback.");

            Assert.False(string.IsNullOrEmpty(redemptionTransactionDocument.transactionId), $"Redemption transactionId was null/empty for voucher {voucherNumber} before rollback.");

            string redemptionTransactionId = redemptionTransactionDocument?.transactionId;

            //Act: retrieve the FlyingClubMember document for the customer.
            string queryForFlyingClubMemberDocument = "SELECT Top 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c.CreatedOn desc";

            QueryDefinition parameterizedFCMQuery = new QueryDefinition(query: queryForFlyingClubMemberDocument)
                .WithParameter("@docType", "FlyingClubMember")
                .WithParameter("@partitionKey", customerId);

            FlyingClubMember flyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedFCMQuery);

            Assert.True(flyingClubMemberDocument is not null, $"No FlyingClubMember document found for customerId {customerId}.");

            string memberNumber = flyingClubMemberDocument.MemberNumber;

            HttpResponseMessage voucherRollbackResponse = Microservice
               .VoucherRollback(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, memberNumber, voucherNumber)
               .EnsureIsSuccessStatusCode();

            // Act: retrieve the updated Award document and transaction documents after the rollback.
            string queryForUpdatedAwardDocument = "SELECT Top 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardNumber = @awardNumber ";
            QueryDefinition updatedAwardQuery = new QueryDefinition(query: queryForUpdatedAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardNumber", voucherNumber);

            Award updatedAwardDoc = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedAwardDoc = cosmosFixture.CosmosQueryItem<Award>(updatedAwardQuery);

                if (updatedAwardDoc is not null && updatedAwardDoc.Status == "Available")
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedAwardDoc is not null, $"No updated Award document found for voucher {voucherNumber} after redemption.");
            Assert.Equal("Available", updatedAwardDoc.Status);
            DateTime parsedLastUpdatedOn;
            bool isLastUpdatedOnValid = DateTime.TryParse(updatedAwardDoc.LastUpdatedOn, out parsedLastUpdatedOn);
            Assert.True(isLastUpdatedOnValid, $"LastUpdatedOn was empty or invalid after rollback. Value: '{updatedAwardDoc.LastUpdatedOn}'.");
            Assert.True(parsedLastUpdatedOn != default(DateTime), "LastUpdatedOn was not populated after rollback.");

            //Act: retrieve the updated Accrual transaction documents after the rollback.
            string queryForUpdatedAccrualTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.TransactionId = @transactionId order by c.CreatedOn desc";

            QueryDefinition updatedAccrualTransactionQuery = new QueryDefinition(query: queryForUpdatedAccrualTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@transactionId", accrualTransactionId);

            TransactionDocuments updatedAccrualTransactionDocument = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedAccrualTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(updatedAccrualTransactionQuery);

                if (updatedAccrualTransactionDocument?.statusCode == "Cancelled")
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedAccrualTransactionDocument is not null, $"No updated Accrual Transaction document found for transactionId {accrualTransactionId} after rollback.");
            Assert.Equal("Cancelled", updatedAccrualTransactionDocument.statusCode);

            //Act: retrieve the updated Redemption transaction documents after the rollback.
            string queryForUpdatedRedemptionTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.TransactionId = @transactionId order by c.CreatedOn desc";

            QueryDefinition updatedRedemptionTransactionQuery = new QueryDefinition(query: queryForUpdatedRedemptionTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@transactionId", redemptionTransactionId);

            TransactionDocuments updatedRedemptionTransactionDocument = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedRedemptionTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(updatedRedemptionTransactionQuery);

                if (updatedRedemptionTransactionDocument?.statusCode == "Cancelled")
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedRedemptionTransactionDocument is not null, $"No updated Redemption Transaction document found for transactionId {redemptionTransactionId} after rollback.");
            Assert.Equal("Cancelled", updatedRedemptionTransactionDocument.statusCode);

            //Act: retrieve the compensatory Redemption transaction documents created as a result of the rollback.
            string queryForCompensatoryRedemptionTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition compensatoryRedemptionTransactionQuery = new QueryDefinition(query: queryForCompensatoryRedemptionTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Redemption")
                .WithParameter("@statusCode", "Processed");

            TransactionDocuments compensatoryRedemptionTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(compensatoryRedemptionTransactionQuery);

            Assert.True(compensatoryRedemptionTransactionDocument is not null, $"No compensatory Redemption Transaction document found for voucher {voucherNumber} after rollback.");
            Assert.Equal("Accrual Cancellation", compensatoryRedemptionTransactionDocument.transactionSubType);
            Assert.Equal(accrualTransactionId, compensatoryRedemptionTransactionDocument.cancelledTransactionId);

            //Act: retrieve the compensatory Accrual transaction documents created as a result of the rollback.
            string queryForCompensatoryAccrualTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition compensatoryAccrualTransactionQuery = new QueryDefinition(query: queryForCompensatoryAccrualTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Accrual")
                .WithParameter("@statusCode", "Processed");

            TransactionDocuments compensatoryAccrualTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(compensatoryAccrualTransactionQuery);

            Assert.True(compensatoryAccrualTransactionDocument is not null, $"No compensatory Accrual Transaction document found for voucher {voucherNumber} after rollback.");
            Assert.Equal("Redemption Cancellation", compensatoryAccrualTransactionDocument.transactionSubType);
            Assert.Equal(redemptionTransactionId, compensatoryAccrualTransactionDocument.cancelledTransactionId);
        }

        [Trait("Category", "VoucherSelfServiceTests")]
        [Fact, RunOrder(4)]
        public async Task TC04_UpdatePnr_SuccessScenario()
        {
            //Act: Enrolling a Test member
            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureIsSuccessStatusCode();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string memberNumber = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            //Act: awarding a voucher to the customer.
            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId from dbo.Product where ProductType='Electronic Voucher' and active='1' and RowId NOT IN ('1-1ZKH', '1-29QH') order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            //Voucher which have expiry date period of 2 years
            List<string> productIds = new() { "1-2CJWLQ4", "1-2CJWLQZ", "1-2CJWLRS", "1-2MG5VE1", "1-41G2YIN" };

            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            string endDate = date.AddYears(productIds.Contains(voucherProductId) ? 2 : 1).ToString("yyyy-MM-dd");

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            //Act: retrieve the most recent Award document for the customer to get the voucherNumber and voucherId.
            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@customerId", customerId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDoc is not null, "No available Award document found for redemption.");

            string voucherNumber = awardDoc.AwardNumber;
            string voucherId = awardDoc.AwardId;

            DateTime futuredate = DataGenerator.RandomFutureDate(+180);
            string activityDate = futuredate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            string bookingClass = "Y";
            string originAirport = "LHR";
            string destinationAirport = "JFK";
            string bookingReference = "";
            string redemptionType = "Reward voucher redemption - Upgrade";

            Random random = new Random();
            int points = random.Next(1, 75001);

            //Act: redeeming the voucher.
            HttpResponseMessage redeemVoucherResponse = Microservice
                .RedeemVoucher(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, memberNumber, redemptionType, activityDate, bookingClass, originAirport, destinationAirport, bookingReference, voucherNumber, voucherName, points)
                .EnsureIsSuccessStatusCode();

            //Act: retrieve the Accrual and Redemption transaction documents for the voucher.
            string queryForAccrualTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType order by c.CreatedOn desc";

            QueryDefinition parameterizedAccrualTransactionQuery = new QueryDefinition(query: queryForAccrualTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Accrual");

            TransactionDocuments accrualTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(parameterizedAccrualTransactionQuery);

            Assert.True(accrualTransactionDocument is not null, $"No Accrual Transaction document found for voucher {voucherNumber} after redemption.");
            Assert.False(string.IsNullOrEmpty(accrualTransactionDocument.transactionId), $"Accrual transactionId was null/empty for voucher {voucherNumber} after redemption.");

            string accrualTransactionId = accrualTransactionDocument?.transactionId;

            Assert.NotNull(accrualTransactionDocument.flightDetails);
            Assert.Equal(bookingReference, accrualTransactionDocument.flightDetails.externalReservationNumber);

            string queryForRedemptionTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @awardId and c.TransactionType = @transactionType order by c.CreatedOn desc";

            QueryDefinition parameterizedRedemptionTransactionQuery = new QueryDefinition(query: queryForRedemptionTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@awardId", voucherId)
                .WithParameter("@transactionType", "Redemption");

            TransactionDocuments redemptionTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(parameterizedRedemptionTransactionQuery);

            Assert.True(redemptionTransactionDocument is not null, $"No Redemption Transaction document found for voucher {voucherNumber} after redemption.");
            Assert.False(string.IsNullOrEmpty(redemptionTransactionDocument.transactionId), $"Redemption transactionId was null/empty for voucher {voucherNumber} after redemption.");

            string redemptionTransactionId = redemptionTransactionDocument?.transactionId;

            Assert.NotNull(redemptionTransactionDocument.flightDetails);
            Assert.Equal(bookingReference, redemptionTransactionDocument.flightDetails.externalReservationNumber);

            string newBookingReference = $"{DataGenerator.RandomString(1, false).ToUpper()}{DataGenerator.RandomString(5, true).ToUpper()}";

            //Act: updating the PNR for the voucher.
            HttpResponseMessage updatePnrResponse = Microservice
                .UpdatePnr(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId, memberNumber, voucherNumber, newBookingReference)
                .EnsureIsSuccessStatusCode();

            //Act: retrieve the updated Accrual transaction documents for the voucher after the PNR update.
            string queryForUpdatedAccrualTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.TransactionId = @transactionId order by c.CreatedOn desc";

            QueryDefinition updatedAccrualTransactionQuery = new QueryDefinition(query: queryForUpdatedAccrualTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@transactionId", accrualTransactionId);

            TransactionDocuments updatedAccrualTransactionDocument = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedAccrualTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(updatedAccrualTransactionQuery);

                if (updatedAccrualTransactionDocument?.flightDetails?.externalReservationNumber == newBookingReference)
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedAccrualTransactionDocument is not null, $"No updated Accrual Transaction document found for voucher {voucherNumber} after PNR update.");
            Assert.NotNull(updatedAccrualTransactionDocument.flightDetails);
            Assert.Equal(newBookingReference, updatedAccrualTransactionDocument.flightDetails.externalReservationNumber);

            //Act: retrieve the updated Redemption transaction documents for the voucher after the PNR update.
            string queryForUpdatedRedemptionTransactionDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.TransactionId = @transactionId order by c.CreatedOn desc";

            QueryDefinition updatedRedemptionTransactionQuery = new QueryDefinition(query: queryForUpdatedRedemptionTransactionDocument)
                .WithParameter("@docType", "Transaction")
                .WithParameter("@partitionKey", customerId)
                .WithParameter("@transactionId", redemptionTransactionId);

            TransactionDocuments updatedRedemptionTransactionDocument = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                updatedRedemptionTransactionDocument = cosmosFixture.CosmosQueryItem<TransactionDocuments>(updatedRedemptionTransactionQuery);

                if (updatedRedemptionTransactionDocument?.flightDetails?.externalReservationNumber == newBookingReference)
                {
                    break;
                }

                await Task.Delay(1000);
            }

            Assert.True(updatedRedemptionTransactionDocument is not null, $"No updated Redemption Transaction document found for voucher {voucherNumber} after PNR update.");
            Assert.NotNull(updatedRedemptionTransactionDocument.flightDetails);
            Assert.Equal(newBookingReference, updatedRedemptionTransactionDocument.flightDetails.externalReservationNumber);
        }
    }
}

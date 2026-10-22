using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsBackendAPITests
{
    [Collection("AwardAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class AwardAPITests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public AwardAPITests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }


        [Trait("Category", Traits.LOOPSAwardAPIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_CreateAwardOrVoucher_SuccessScenario()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId from dbo.Product where ProductType='Electronic Voucher' and active='1' order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            //Voucher which have expiry date period of 2 years
            List<string> productIds = new() { "1-2CJWLQ4", "1-2CJWLQZ", "1-2CJWLRS", "1-2MG5VE1", "1-41G2YIN" };

            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            string endDate = date.AddYears(productIds.Contains(voucherProductId) ? 2 : 1).ToString("yyyy-MM-dd");

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher rewarded successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc.ProductId.Equals(voucherProductId),
                $"Expected the Voucher ProductId to be {voucherProductId} but was {awardDoc.ProductId} .");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate),
                $"Expected the Voucher EndDate to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"),
                $"Expected the Voucher Status to be Available but was {awardDoc.Status} .");

            #endregion
        }

        [Trait("Category", Traits.LOOPSAwardAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_UpdateAwardOrVoucher_SuccessScenario()
        {
            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId not in ('1634561592','1634561747','1634561772',null,'')  order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string voucherStatus = awardDoc.Status;

            string[] validStatuses = { "Available", "Expired", "Used", "Cancelled", "Gifted", "Partially Used" };

            string newStatus = validStatuses[0];

            Random random = new();

            while (newStatus == voucherStatus)
            {
                newStatus = validStatuses[random.Next(validStatuses.Length)];
            }

            string startDate = awardDoc.StartDate.AddDays(-1).ToString("yyyy-MM-dd");

            string endDate = awardDoc.ExpiryDate.AddDays(+1).ToString("yyyy-MM-dd");

            string usedDate = newStatus.Equals("Used") ? DateTime.UtcNow.ToString("yyyy-MM-dd") : "";


            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.UpdateAward(tokenFixture.GetToken("award"), customerId, voucherId, newStatus, startDate, endDate, usedDate).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher updated successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId order by c._ts desc";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId);

            awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            string actualUsedDate = awardDoc.UsedDate.Equals(DateTime.MinValue) ? "" : awardDoc.UsedDate.ToString("yyyy-MM-dd");

            Assert.True(actualUsedDate.Equals(usedDate),
                $"Expected the Voucher UsedDate to be {usedDate} but was {actualUsedDate} .");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate),
                $"Expected the Voucher EndDate to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals(newStatus),
                $"Expected the Voucher Status to be {newStatus} but was {awardDoc.Status} .");

            #endregion
        }

        [Trait("Category", Traits.LOOPSAwardAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_RetrieveAwardOrVoucher_SuccessScenario()
        {
            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId not in ('1634561592','1634561747','1634561772',null,'') order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            string customerId = awardDoc.CustomerId;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.RetrieveAward(tokenFixture.GetToken("award"), customerId).EnsureIsSuccessStatusCode();

            IEnumerable<RetrieveAwardDetails> actualAwardDetails = JsonConvert.DeserializeObject<IEnumerable<RetrieveAwardDetails>>(await response.Content.ReadAsStringAsync()).OrderByDescending(c => c.VoucherID);

            Assert.True(actualAwardDetails is not null, $"Failed to Retrieve Award details for the customer {customerId}," +
                $"The API Response was {await response.Content.ReadAsStringAsync()}");

            #endregion

            #region Validations

            queryForAwardDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId);

            IEnumerable<Award> expectedAwardDoc = cosmosFixture.CosmosQueryItems<Award>(parameterizedQuery).OrderByDescending(c => c.AwardId);

            Assert.True(actualAwardDetails.Count() == expectedAwardDoc.Count(),
                   $"Award Documents Count Does Not Match For the Customer Id {customerId} " +
                   $"Expected Count to be {expectedAwardDoc.Count()} but was {actualAwardDetails.Count()}.");

            int i = 0;

            foreach (Award expectedDetails in expectedAwardDoc)
            {

                Assert.True(expectedDetails.AwardId.Equals(actualAwardDetails.ElementAt(i).VoucherID),
                $"For the Customer Id {customerId} , Expected the Award Id to be {expectedDetails.AwardId} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherID}");

                string queryForVoucherName = $"Select ProductName from dbo.Product where RowId='{expectedDetails.ProductId}'";

                string voucherName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForVoucherName);

                Assert.True(voucherName.Equals(actualAwardDetails.ElementAt(i).VoucherDetails.VoucherName),
                $"For the Award Id {expectedDetails.AwardId} , Expected the Award/Voucher Name to be {voucherName} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherDetails.VoucherName}");

                Assert.True(expectedDetails.AwardNumber.Equals(actualAwardDetails.ElementAt(i).VoucherDetails.VoucherNumber),
                $"For the Award Id {expectedDetails.AwardId} , Expected the Award/Voucher Number to be {expectedDetails.AwardNumber} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherDetails.VoucherNumber}");

                Assert.True(expectedDetails.Status.Equals(actualAwardDetails.ElementAt(i).VoucherDetails.Status),
                $"For the Award Id {expectedDetails.AwardId} , Expected the Award/Voucher Status to be {expectedDetails.Status} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherDetails.Status}");

                Assert.True(expectedDetails.StartDate.ToString("yyyy-MM-dd").Equals(DateTime.Parse(actualAwardDetails.ElementAt(i).VoucherDetails.StartDate).ToString("yyyy-MM-dd")),
                $"For the Award Id {expectedDetails.AwardId} , Expected the Award/Voucher StartDate to be {expectedDetails.StartDate.ToString("yyyy-MM-dd")} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherDetails.StartDate}");

                Assert.True(expectedDetails.ExpiryDate.ToString("yyyy-MM-dd").Equals(DateTime.Parse(actualAwardDetails.ElementAt(i).VoucherDetails.ExpiryDate).ToString("yyyy-MM-dd")),
                $"For the Award Id {expectedDetails.AwardId} , Expected the Award/Voucher EndDate to be {expectedDetails.ExpiryDate.ToString("yyyy-MM-dd")} " +
                $"but was {actualAwardDetails.ElementAt(i).VoucherDetails.ExpiryDate}");

                i++;
            }

            #endregion
        }

        [Trait("Category", Traits.LOOPSAwardAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_GiftAwardOrVoucher_SuccessScenario()
        {
            #region Pre Requisite

            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureGQLResponseSuccess();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            string customerId = enrolmentResult.SelectToken("data.accountEnrolMember.customerId")?.ToString();
            string memberNumber = enrolmentResult.SelectToken("data.accountEnrolMember.memberDetails.memberNumber")?.ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), "Enrolment response did not contain a customer ID.");
            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Enrolment response did not contain a member number.");

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId,ExpirationPeriod from dbo.Product where ProductType='Electronic Voucher' and active='1' and RowId NOT IN ('1-1ZKH', '1-29QH') order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            Assert.True(voucherDetails.Length > 0 && voucherDetails[0].Length == 3, "No eligible voucher product was returned from the reference database.");

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            Assert.False(voucherName.IsNullOrWhiteSpace(), "Voucher product name was not returned from the reference database.");
            Assert.False(voucherProductId.IsNullOrWhiteSpace(), "Voucher product ID was not returned from the reference database.");
            Assert.True(int.TryParse(voucherDetails[0][2]?.ToString(), out int expiryMonths) && expiryMonths > 0,
                "Voucher expiration period was not a positive number of months.");

            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            string endDate = date.AddMonths(expiryMonths).ToString("yyyy-MM-dd");

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            string createAwardResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Voucher rewarded successfully", createAwardResponse);

            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.PartitionKey = @customerId and c.ProductId = @productId and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument)
                .WithParameter("@docType", "Award")
                .WithParameter("@customerId", customerId)
                .WithParameter("@productId", voucherProductId)
                .WithParameter("@status", "Available");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            int retry = 5;

            while (retry > 0 && awardDoc is null)
            {
                await Task.Delay(1000);

                awardDoc = cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

                retry--;
            }

            Assert.True(awardDoc is not null, "No newly created available Award document was found for gifting.");
            Assert.False(awardDoc.AwardNumber.IsNullOrWhiteSpace(), "Newly created Award document has no award number.");

            string voucherId = awardDoc.AwardId;
            string recipientAwardNumber = $"{awardDoc.AwardNumber}-2";

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey <> @partitionKey and c.Status = @status and IS_DEFINED(c.LoyaltyMemberNumber) and c.LoyaltyMemberNumber != '' order by c.CreatedOn desc";

            QueryDefinition parameterizedQueryForRecipient = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@status", "Active");

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQueryForRecipient);

            Assert.True(customerDoc is not null, "No active recipient with a loyalty member number was found for gifting.");

            string recipientCustomerId = customerDoc.CustomerId;

            string recipientMemberNumber = customerDoc.LoyaltyMemberNumber;

            #endregion

            #region API Call

            HttpResponseMessage voucherGiftResponse = LoopsBackendAPIEndpoints.Award.GiftAward(tokenFixture.GetToken("award"), customerId, voucherId, recipientMemberNumber, recipientCustomerId).EnsureIsSuccessStatusCode();

            string giftAwardResponse = await voucherGiftResponse.Content.ReadAsStringAsync();
            Assert.Contains("Voucher gifted successfully", giftAwardResponse);

            #endregion

            #region Validation

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId);

            awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, "Sender Award document was not found after gifting.");
            Assert.True(awardDoc.Status.Equals("Gifted"),
                $"After the Gift API Call, Expected the Voucher Status in the Sender Doc to be Gifted but was {awardDoc.Status}.");

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardNumber = @recipientAwardNumber order by c._ts desc";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", recipientCustomerId).WithParameter("@recipientAwardNumber", recipientAwardNumber);

            awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, $"Gifted Award {recipientAwardNumber} was not found for recipient {recipientCustomerId}.");
            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate),
                $"After the Gift API Call ,Expected the Voucher StartDate in the recipient Doc to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate),
                $"After the Gift API Call ,Expected the Voucher EndDate in the recipient Doc to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"),
                $"After the Gift API Call, Expected the Voucher Status in the recipient Doc to be Available but was {awardDoc.Status} .");

            Assert.True(awardDoc.ProductId.Equals(voucherProductId),
               $"After the Gift API Call, Expected the Voucher ProductId in the recipient Doc to be {voucherProductId} but was {awardDoc.ProductId} .");

            #endregion
        }

        [Trait("Category", Traits.LOOPSAwardAPIRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_RedeemAwardOrVoucher_SuccessScenario()
        {
            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.ProductId not in ('1634561592','1634561747','1634561772',null,'')  order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Available");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string productId = awardDoc.ProductId;

            string startDate = awardDoc.StartDate.ToString("yyyy-MM-dd");

            string expiryDate = awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.RedeemAward(tokenFixture.GetToken("award"), customerId, voucherId).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher redeemed successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Validation

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardId = @voucherId order by c._ts desc";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherId", voucherId);

            awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc.Status.Equals("Used"),
                $"After the Redeem API Call, Expected the Voucher Status to be Used but was {awardDoc.Status}.");

            Assert.True(awardDoc.UsedDate.ToString("yyyy-MM-dd").Equals(DateTime.UtcNow.ToString("yyyy-MM-dd")),
                $"After the Redeem API Call, Expected the Voucher Used Date to be {DateTime.UtcNow.ToString("yyyy-MM-dd")}" +
                $" but was {awardDoc.UsedDate.ToString("yyyy-MM-dd")}.");

            #endregion
        }
    }
}

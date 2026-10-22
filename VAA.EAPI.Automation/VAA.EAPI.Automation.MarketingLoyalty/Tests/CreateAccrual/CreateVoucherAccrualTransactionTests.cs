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
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CreateAccrual
{
    [Collection("VoucherAssociatedTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CreateVoucherAccrualTransactionTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIOcpSubKey = string.Empty;
        private static string vRedMSOcpSubKey = string.Empty;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public CreateVoucherAccrualTransactionTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;
            _tokenFixture = tokenFixture;

            if (!backendAPIEnv.Is("Project-Dev"))
                _tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            _cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            vRedMSOcpSubKey = Vault.Default.GetSecretValue("TestingVRedMSSubscriptionKey");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [InlineData("Reward voucher redemption - Companion")]
        [InlineData("Reward voucher redemption - Upgrade")]
        [Theory, RunOrder(1)]
        public async Task CreateVoucherAccrualTransaction_SuccessScenario(string redemptionProduct)
        {
            #region Member and Voucher Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status in ('Active' ,'Active Pending' , 'Inactive' ,'Frozen' ,'Expired') order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve a valid member whose status is valid for Create Accrual Transaction");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId,ExpirationPeriod from dbo.Product where ProductType ='Electronic Voucher' and active = '1' and RowId not in ( '1-1ZKH','1-29QH') order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            string voucherExpiryPeriod = voucherDetails[0][2]?.ToString();

            DateTime currentDate = DateTime.UtcNow;

            string startDate = currentDate.ToString("yyyy-MM-dd");

            string endDate = currentDate.AddYears(voucherExpiryPeriod == "24" ? 2 : 1).ToString("yyyy-MM-dd");

            #endregion

            #region Create Voucher API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(_tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            string expectedResponse = "Voucher rewarded successfully";

            Assert.True(result.Has(expectedResponse), $"Expected {expectedResponse} but was {result} .");

            #endregion

            #region Award Document Validation in Cosmos Database

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@productId", voucherProductId);

            Award awardDoc = _cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, $"Award Document not found after successful API Response for the member {memberNumber} for the Voucher type {voucherName} ");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate), $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate), $"Expected the Voucher EndDate to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"), $"Expected the Voucher Status to be Available but was {awardDoc.Status} .");

            #endregion

            #region Get Member Point Balance API Call Before Voucher Accrual Transaction Creation

            HttpResponseMessage memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            dynamic memberBalanceResult = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceBeforeVoucherAccrualTxn = string.IsNullOrEmpty(memberBalanceResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(memberBalanceResult?.Usable?.ToString());

            #endregion

            #region Create Voucher Accrual Transaction GraphQL Endpoint Call

            string activityDate = currentDate.ToString("yyyy-MM-dd");
            string createdBy = "TestAutomationScript";
            string bookingClass = "J";
            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();
            string originAirportCode = "LHR";
            string destinationAirportCode = "DEL";
            string ticketNumber = "9323" + DataGenerator.RandomNumber(100000000, 999999999);
            int noOfPassengers = Convert.ToInt32(DataGenerator.RandomNumber(1, 5, true));
            int noOfSegments = Convert.ToInt32(DataGenerator.RandomNumber(1, 3, true));
            string partnerCode = "VS";
            string productName = "Voucher Points";
            int points = Convert.ToInt32(fcmDoc.TierCode.Is("Red") ? DataGenerator.RandomNumber(1, 75000, true) : DataGenerator.RandomNumber(1, 150000, true));
            string transactionSource = "PAUSER";
            string voucherNumber = awardDoc.AwardNumber;
            string voucherUsage = "Used";

            HttpResponseMessage createAccrualResponse = GraphQL.CreateVoucherAccrualTransaction(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber, fcmDoc.Status, activityDate, "", createdBy, bookingClass, bookingReference, originAirportCode, destinationAirportCode, ticketNumber, noOfPassengers, noOfSegments, partnerCode, productName, points, 0, transactionSource, voucherNumber, voucherUsage).EnsureIsSuccessStatusCode();

            JObject accrualTxnResult = (JObject)JsonConvert.DeserializeObject(await createAccrualResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(accrualTxnResult, "extension"), $"Expected a Valid response from GraphQL but found {accrualTxnResult}");

            string transactionId = accrualTxnResult["data"]?["loyaltyCreateAccrual"]?["transactionId"]?.ToString() ?? "";

            string transactionStatus = accrualTxnResult["data"]?["loyaltyCreateAccrual"]?["transactionStatus"]?.ToString() ?? "";

            Assert.True(!transactionId.IsNullOrEmpty(), $"Failed to Create Voucher Accrual Transaction for the Member {memberNumber}.The Response returned is \n {accrualTxnResult} ");

            Assert.True(transactionStatus.Is("Processed"), $"Failed to Create Voucher Accrual Transaction for the Member {memberNumber}. The Response returned is \n {accrualTxnResult} ");

            #endregion

            #region Get Member Point Balance API Call After Voucher Accrual Transaction Creation

            memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            memberBalanceResult = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse?.Content?.ReadAsStringAsync());

            long memberUsableBalanceAfterVoucherAccrualTxn = string.IsNullOrEmpty(memberBalanceResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(memberBalanceResult?.Usable?.ToString());

            int retry = 5;

            while (retry > 0 && memberUsableBalanceAfterVoucherAccrualTxn <= memberUsableBalanceBeforeVoucherAccrualTxn)
            {
                await Task.Delay(2000);

                memberBalanceResponse = Microservice.GetMemberPointBalance(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                memberBalanceResult = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

                memberUsableBalanceAfterVoucherAccrualTxn = string.IsNullOrEmpty(memberBalanceResult?.Usable?.ToString()) ? 0 : Convert.ToInt64(memberBalanceResult?.Usable?.ToString());

                retry--;
            }

            Assert.True(memberUsableBalanceAfterVoucherAccrualTxn == memberUsableBalanceBeforeVoucherAccrualTxn + points,
                $"The Member- {memberNumber} - Point balance did not match after the successful Voucher Accrual transaction Creation.Expected point balance to be  {memberUsableBalanceBeforeVoucherAccrualTxn + points} but found {memberUsableBalanceAfterVoucherAccrualTxn}");

            #endregion

            #region Create Redemption Transaction using Frontend API            

            string comments = "Test Transaction Created for Voucher Redemption";
            string pointsForRedemption = memberUsableBalanceBeforeVoucherAccrualTxn > 0 ? new Random().Next(1, (int)memberUsableBalanceBeforeVoucherAccrualTxn).ToString() : "0";
            string redemptionActivityDate = DataGenerator.RandomFutureDate(330).ToString("dd-MMM-yyyy");

            HttpResponseMessage manualRedemptionResponse = LoopsFrontEndAPIEndpoints.ProcessManualRedemption(partnerCode, redemptionProduct, redemptionActivityDate, pointsForRedemption, comments, customerId, memberNumber, bookingReference, ticketNumber, originAirportCode, destinationAirportCode, bookingClass, noOfPassengers, noOfSegments, voucherNumber, voucherUsage).EnsureIsSuccessStatusCode();

            #endregion            

            string expectedAccrualTxnStatementText = "Point credit for reward voucher redemption";

            string[] splitVoucherName = voucherName.IsNullOrEmpty() ? Array.Empty<string>() : voucherName?.Split('-');

            string value = splitVoucherName.Length > 1 ? splitVoucherName[1] : "";

            string expectedRedemptionTxnStatementText = $"{redemptionProduct},{value}, Booking ref {bookingReference}, {originAirportCode}-{destinationAirportCode}";

            #region Validation of Transactions into VRed Statement

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

            IEnumerable<Statement> statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

            retry = 10;

            while (retry > 0 && !statements.Any(x => x.type.Is("CREDIT") && x.DetailsJson.Has(bookingReference)))
            {
                await Task.Delay(1000);

                memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

                statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

                retry--;
            }

            #region Voucher Accrual Transaction Validation

            bool isVoucherAccrualTransactionPresent = false;

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.bookingReference == bookingReference && statement.Details.activityType == "Accrual")
                {
                    isVoucherAccrualTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == DateTime.Parse(activityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Accrual Booking Reference {bookingReference}, In the VRed Statement, The ActivityDate does not match. Expected {DateTime.Parse(activityDate).ToString("dd-MMM-yyyy")} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy") == DateTime.Parse(activityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Accrual Booking Reference {bookingReference}, In the VRed Statement, The TransactionDate does not match. Expected {DateTime.Parse(activityDate).ToString("dd-MMM-yyyy")} but found {statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activitySubtype == "Product", $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.createdBy == createdBy, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The CreatedBy does not match. Expected {createdBy} but found {statement.Details.createdBy}");

                    Assert.True(statement.Details.partnerCode == partnerCode, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The PartnerCode does not match. Expected {partnerCode} but found {statement.Details.partnerCode}");

                    Assert.True(statement.Details.productName == productName, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The productName does not match. Expected {productName} but found {statement.Details.productName}");

                    Assert.True(statement.Details.sectors.First()?.originAirport == originAirportCode, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The OriginAirportCode does not match. Expected {originAirportCode} but found {statement.Details.sectors.First()?.originAirport}");

                    Assert.True(statement.Details.sectors.First()?.destinationAirport == destinationAirportCode, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The DestinationAirportCode does not match. Expected {destinationAirportCode} but found {statement.Details.sectors.First()?.destinationAirport}");

                    Assert.True(statement.Details.sectors.First()?.ticketNumber == ticketNumber, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The TicketNumber does not match. Expected {ticketNumber} but found {statement.Details.sectors.First()?.ticketNumber}");

                    Assert.True(statement.Details.sectors.First()?.flownBookingClass == bookingClass, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The BookingClass does not match. Expected {bookingClass} but found {statement.Details.sectors.First()?.flownBookingClass}");

                    Assert.True(statement.Details.statementText == expectedAccrualTxnStatementText, $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The BookingClass does not match. Expected {expectedAccrualTxnStatementText} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.transactionStatus == "Processed", $"For the Voucher Accrual Booking Reference {bookingReference} In the VRed Statement, The TransactionStatus does not match. Expected Processed but found {statement.Details.transactionStatus}");

                    break;
                }

            }

            Assert.True(isVoucherAccrualTransactionPresent, $"For the Booking Reference {bookingReference} for the Member {memberNumber} The Voucher Accrual transaction details not found in VRed Statement.");

            #endregion

            memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

            vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

            statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

            retry = 10;

            while (retry > 0 && !statements.Any(x => x.type.Is("DEBIT") && x.DetailsJson.Has(bookingReference)))
            {
                await Task.Delay(1000);

                memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

                statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

                retry--;
            }

            #region Voucher Redemption Transaction Validation

            bool isVoucherRedemptionTransactionPresent = false;

            foreach (Statement statement in statements)
            {
                statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                if (statement.Details.bookingReference == bookingReference && statement.Details.activityType == "Redemption")
                {
                    isVoucherRedemptionTransactionPresent = true;

                    Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == DateTime.Parse(redemptionActivityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber}, In the VRed Statement, The ActivityDate does not match. Expected {DateTime.Parse(redemptionActivityDate).ToString("dd-MMM-yyyy")} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"), $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber}, In the VRed Statement, The TransactionDate does not match. Expected {currentDate.ToString("dd-MMM-yyyy")} but found {statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy")}");

                    Assert.True(statement.Details.activitySubtype == "Product", $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                    Assert.True(statement.Details.partnerCode == partnerCode, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The PartnerCode does not match. Expected {partnerCode} but found {statement.Details.partnerCode}");

                    Assert.True(statement.Details.productName == redemptionProduct, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The productName does not match. Expected {redemptionProduct} but found {statement.Details.productName}");

                    Assert.True(statement.Details.sectors.First()?.originAirport == originAirportCode, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The OriginAirportCode does not match. Expected {originAirportCode} but found {statement.Details.sectors.First()?.originAirport}");

                    Assert.True(statement.Details.sectors.First()?.destinationAirport == destinationAirportCode, $"For the Voucher Redemption Booking Reference {bookingReference}  for Member {memberNumber}In the VRed Statement, The DestinationAirportCode does not match. Expected {destinationAirportCode} but found {statement.Details.sectors.First()?.destinationAirport}");

                    Assert.True(statement.Details.sectors.First()?.ticketNumber == ticketNumber, $"For the Voucher Redemption Booking Reference {bookingReference}  for Member {memberNumber}In the VRed Statement, The TicketNumber does not match. Expected {ticketNumber} but found {statement.Details.sectors.First()?.ticketNumber}");

                    Assert.True(statement.Details.sectors.First()?.flownBookingClass == bookingClass, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The BookingClass does not match. Expected {bookingClass} but found {statement.Details.sectors.First()?.flownBookingClass}");

                    Assert.True(statement.Details.statementText == expectedRedemptionTxnStatementText, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The Statement does not match. Expected {expectedRedemptionTxnStatementText} but found {statement.Details.statementText}");

                    Assert.True(statement.Details.transactionStatus == "Processed", $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The TransactionStatus does not match. Expected Processed but found {statement.Details.transactionStatus}");

                    break;
                }

            }

            Assert.True(isVoucherRedemptionTransactionPresent, $"For the Booking Reference {bookingReference} for the Member {memberNumber} The Voucher Redemption transaction details not found in VRed Statement.");

            #endregion

            #endregion

            #region Voucher Status Validation in Award Cosmos Document

            queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.AwardNumber = @voucherNumber order by c.CreatedOn desc";

            paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@voucherNumber", voucherNumber);

            awardDoc = _cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, $"Award Document not found after successful API Response for the member {memberNumber} for the Voucher type {voucherName} ");

            Assert.True(awardDoc.UsedDate.ToString("yyyy-MM-dd").Equals(currentDate.ToString("yyyy-MM-dd")), $"Expected the Voucher EndDate to be {currentDate.ToString("yyyy-MM-dd")} but was {awardDoc.UsedDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Used"), $"For the Member {memberNumber}, Expected the Voucher Status of Voucher with Award Number {voucherNumber} to be Used after the Voucher Successful Redemption but was {awardDoc.Status} .");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task CreateVoucherAccrualTransactionShouldFailForInvalidMemberStatus()
        {
            #region Member and Voucher Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) and c.Status not in ('Active' ,'Active Pending' , 'Inactive' ,'Frozen' ,'Expired') order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve a valid member whose status is valid for Create Accrual Transaction");

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            string queryForVoucherDetails = "Select TOP 1 ProductName,RowId,ExpirationPeriod from dbo.Product where ProductType ='Electronic Voucher' and active = '1' and RowId != '1-1ZKH' order by NEWID()";

            object[][] voucherDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForVoucherDetails);

            string voucherName = voucherDetails[0][0]?.ToString();

            string voucherProductId = voucherDetails[0][1]?.ToString();

            string voucherExpiryPeriod = voucherDetails[0][2]?.ToString();

            DateTime currentDate = DateTime.UtcNow;

            string startDate = currentDate.ToString("yyyy-MM-dd");

            string endDate = currentDate.AddYears(voucherExpiryPeriod == "24" ? 2 : 1).ToString("yyyy-MM-dd");

            #endregion

            #region Create Voucher API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(_tokenFixture.GetToken("award"), customerId, voucherName, startDate, endDate).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            string expectedResponse = "Voucher rewarded successfully";

            Assert.True(result.Has(expectedResponse), $"Expected {expectedResponse} but was {result} .");

            #endregion

            #region Award Document Validation in Cosmos Database

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@productId", voucherProductId);

            Award awardDoc = _cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, $"Award Document not found after successful API Response for the member {memberNumber} for the Voucher type {voucherName} ");

            Assert.True(awardDoc.StartDate.ToString("yyyy-MM-dd").Equals(startDate), $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.ExpiryDate.ToString("yyyy-MM-dd").Equals(endDate), $"Expected the Voucher EndDate to be {endDate} but was {awardDoc.ExpiryDate.ToString("yyyy-MM-dd")} .");

            Assert.True(awardDoc.Status.Equals("Available"), $"Expected the Voucher Status to be Available but was {awardDoc.Status} .");

            #endregion

            #region Create Voucher Accrual Transaction GraphQL Endpoint Call

            string activityDate = currentDate.ToString("MM/dd/yyyy");
            string createdBy = "TestAutomationScript";
            string bookingClass = "J";
            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();
            string originAirportCode = "LHR";
            string destinationAirportCode = "DEL";
            string ticketNumber = "9323" + DataGenerator.RandomNumber(100000000, 999999999);
            int noOfPassengers = Convert.ToInt32(DataGenerator.RandomNumber(1, 5, true));
            int noOfSegments = Convert.ToInt32(DataGenerator.RandomNumber(1, 3, true));
            string partnerCode = "VS";
            string productName = "Voucher Points";
            int points = Convert.ToInt32(fcmDoc.TierCode.Is("Red") ? DataGenerator.RandomNumber(1, 75000, true) : DataGenerator.RandomNumber(1, 150000, true));
            string transactionSource = "PAUSER";
            string voucherNumber = awardDoc.AwardNumber;
            string voucherUsage = "Used";

            HttpResponseMessage createAccrualResponse = GraphQL.CreateVoucherAccrualTransaction(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber, fcmDoc.Status, activityDate, "", createdBy, bookingClass, bookingReference, originAirportCode, destinationAirportCode, ticketNumber, noOfPassengers, noOfSegments, partnerCode, productName, points, 0, transactionSource, voucherNumber, voucherUsage).EnsureIsSuccessStatusCode();

            JObject accrualTxnResult = (JObject)JsonConvert.DeserializeObject(await createAccrualResponse.Content.ReadAsStringAsync());

            string expectedErrorMessage = "Member status is not valid for creation of accrual transaction";

            string actualErrorMessage = accrualTxnResult["errors"]?[0]?["message"]?.ToString() ?? "";

            Assert.Equal(expectedErrorMessage, actualErrorMessage);

            #endregion
        }
    }
}

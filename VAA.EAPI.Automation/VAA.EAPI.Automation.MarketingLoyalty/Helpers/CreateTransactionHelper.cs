using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;
using static VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public class CreateTransactionHelper
    {
        private readonly TokenFixture _tokenFixture = new();
        private readonly CosmosFixture _cosmosFixture = new();
        private readonly PreRequisiteTestData preRequisiteTestData = new();
        Random random = new();

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIOcpSubKey = string.Empty;
        private static string vRedMSOcpSubKey = string.Empty;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public CreateTransactionHelper()
        {

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
            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            vRedMSOcpSubKey = Vault.Default.GetSecretValue("TestingVRedMSSubscriptionKey");
        }


        public async Task<CreateTransactionResponse> CreateTransaction(TransactionProduct product, string partnerCode, string bookingReference, DateTime activityDate, DateTime ticketIssueDate, int points, int tierPoints = 0, string memberNumber = "", string customerId = "")
        {

            string transactionProduct = product.ToString().Replace("-", " ");

            if (product is TransactionProduct.Reward_voucher_redemption)
                transactionProduct += " - Companion";

            string txnId = string.Empty;

            string txnStatus = string.Empty;

            string queryForFCMDoc = string.Empty;

            QueryDefinition parameterizedQuery = null;

            if (!memberNumber.IsNullOrEmpty() && customerId.IsNullOrEmpty())
            {
                queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.MemberNumber = @memberNumber";

                parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@memberNumber", memberNumber);

                customerId = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery)?.CustomerId;
            }
            if (!customerId.IsNullOrEmpty() && memberNumber.IsNullOrEmpty())
            {
                queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId";

                parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

                memberNumber = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery)?.MemberNumber;
            }

            if (memberNumber.IsNullOrEmpty() && customerId.IsNullOrEmpty())
                (memberNumber, customerId) = await RetrieveValidFlyingClubMember();

            string txnType = product is TransactionProduct.Flight_Redemption or TransactionProduct.Reward_voucher_redemption ? "Redemption" : "Accrual";

            string partnerID = (string)AzureReferenceDBHelper.RefDbQueryResponse($"Select RowId from DBO.PartnerProfile where PartnerCode='{partnerCode}' and active=1 ");

            bool isTransactionCreationSuccessful = false;

            switch (product)
            {
                case TransactionProduct.Reward_voucher_redemption:
                    {

                        #region Member and Voucher Test Data                    


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

                        Core.DB_Model.Entities.Award awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

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

                        string createdBy = "TestAutomationScript";
                        string bookingClass = "J";
                        string originAirportCode = "LHR";
                        string destinationAirportCode = "DEL";
                        string ticketNumber = "9323" + DataGenerator.RandomNumber(100000000, 999999999);
                        int noOfPassengers = Convert.ToInt32(DataGenerator.RandomNumber(1, 5, true));
                        int noOfSegments = Convert.ToInt32(DataGenerator.RandomNumber(1, 3, true));
                        string productName = "Voucher Points";
                        string transactionSource = "PAUSER";
                        string voucherNumber = awardDoc.AwardNumber;
                        string voucherUsage = "Used";

                        HttpResponseMessage createAccrualResponse = GraphQL.CreateVoucherAccrualTransaction(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber, "Active", activityDate.ToString("MM/dd/yyyy"), "", createdBy, bookingClass, bookingReference, originAirportCode, destinationAirportCode, ticketNumber, noOfPassengers, noOfSegments, partnerCode, productName, points, 0, transactionSource, voucherNumber, voucherUsage).EnsureIsSuccessStatusCode();

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

                        HttpResponseMessage manualRedemptionResponse = LoopsFrontEndAPIEndpoints.ProcessManualRedemption(partnerCode, transactionProduct, redemptionActivityDate, pointsForRedemption, comments, customerId, memberNumber, bookingReference, ticketNumber, originAirportCode, destinationAirportCode, bookingClass, noOfPassengers, noOfSegments, voucherNumber, voucherUsage).EnsureIsSuccessStatusCode();

                        #endregion

                        string expectedAccrualTxnStatementText = "Point credit for reward voucher redemption";

                        string[] splitVoucherName = voucherName.IsNullOrEmpty() ? Array.Empty<string>() : voucherName?.Split('-');

                        string value = splitVoucherName.Length > 1 ? splitVoucherName[1] : "";

                        string expectedRedemptionTxnStatementText = $"{transactionProduct},{value}, Booking ref {bookingReference}, {originAirportCode}-{destinationAirportCode}";

                        #region Validation of Transactions into VRed Statement

                        HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                        VRedStatementResponse vRedStatementResponse = JsonConvert.DeserializeObject<VRedStatementResponse>(await memberTxnStatement?.Content?.ReadAsStringAsync());

                        IEnumerable<Requests.Models.Microservices.Statement> statements = vRedStatementResponse.statements.OrderByDescending(x => x.transactionDate);

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

                        foreach (Requests.Models.Microservices.Statement statement in statements)
                        {
                            statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                            if (statement.Details.bookingReference == bookingReference && statement.Details.activityType == "Accrual")
                            {
                                isVoucherAccrualTransactionPresent = true;

                                Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == (activityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Accrual Booking Reference {bookingReference}, In the VRed Statement, The ActivityDate does not match. Expected {(activityDate).ToString("dd-MMM-yyyy")} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                                Assert.True(statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy") == (activityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Accrual Booking Reference {bookingReference}, In the VRed Statement, The TransactionDate does not match. Expected {activityDate.ToString("dd-MMM-yyyy")} but found {statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy")}");

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

                        foreach (Requests.Models.Microservices.Statement statement in statements)
                        {
                            statement.Details = JsonConvert.DeserializeObject<Details>(statement.DetailsJson);

                            if (statement.Details.bookingReference == bookingReference && statement.Details.activityType == "Redemption")
                            {
                                isVoucherRedemptionTransactionPresent = true;

                                Assert.True(statement.Details.activityDatetime.ToString("dd-MMM-yyyy") == DateTime.Parse(redemptionActivityDate).ToString("dd-MMM-yyyy"), $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber}, In the VRed Statement, The ActivityDate does not match. Expected {DateTime.Parse(redemptionActivityDate).ToString("dd-MMM-yyyy")} but found {statement.Details.activityDatetime.ToString("dd-MMM-yyyy")}");

                                Assert.True(statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy") == currentDate.ToString("dd-MMM-yyyy"), $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber}, In the VRed Statement, The TransactionDate does not match. Expected {currentDate.ToString("dd-MMM-yyyy")} but found {statement.Details.transactionDatetime.Value.ToString("dd-MMM-yyyy")}");

                                Assert.True(statement.Details.activitySubtype == "Product", $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The ActivitySubType does not match. Expected Product but found {statement.Details.activitySubtype}");

                                Assert.True(statement.Details.partnerCode == partnerCode, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The PartnerCode does not match. Expected {partnerCode} but found {statement.Details.partnerCode}");

                                Assert.True(statement.Details.productName == transactionProduct, $"For the Voucher Redemption Booking Reference {bookingReference} for Member {memberNumber} In the VRed Statement, The productName does not match. Expected {transactionProduct} but found {statement.Details.productName}");

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

                        awardDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Award>(paramQuery);

                        Assert.True(awardDoc is not null, $"Award Document not found after successful API Response for the member {memberNumber} for the Voucher type {voucherName} ");

                        Assert.True(awardDoc.UsedDate.ToString("yyyy-MM-dd").Equals(currentDate.ToString("yyyy-MM-dd")), $"Expected the Voucher EndDate to be {currentDate.ToString("yyyy-MM-dd")} but was {awardDoc.UsedDate.ToString("yyyy-MM-dd")} .");

                        Assert.True(awardDoc.Status.Equals("Used"), $"For the Member {memberNumber}, Expected the Voucher Status of Voucher with Award Number {voucherNumber} to be Used after the Voucher Successful Redemption but was {awardDoc.Status} .");

                        #endregion

                        isTransactionCreationSuccessful = true;

                        break;
                    }

                case TransactionProduct.Flight:
                    {

                        #region Test Data Retrieval of Customer ID

                        DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

                        DateTime activityPastDate = ticketIssuePastDate.AddDays(2);

                        string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

                        string ticketNumber = "93232" + DataGenerator.RandomNumber(10000000, 99999999, false);

                        string comments = "Testing Only";

                        string airlineCode = "VS";
                        string bookingClass = "J";
                        string originAirport = Origins[random.Next(0, Origins.Length)];
                        string destinationAirport = Destinations[random.Next(0, Destinations.Length)];
                        string flightNumber = DataGenerator.RandomNumber(1, 999, true);

                        #endregion

                        #region API Call

                        HttpResponseMessage response = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(bookingReference, bookingClass, activityDate.ToString("MM/dd/yyyy"), airlineCode,
                        ticketIssueDate.ToString("MM/dd/yyyy"), originAirport, flightNumber, destinationAirport, memberNumber, customerId, createdOn, comments, ticketNumber).EnsureIsSuccessStatusCode();

                        string result = await response.Content.ReadAsStringAsync();

                        FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(result);

                        Assert.True(flightAccrualResponse is not null, $"Failed to flightAccrualResponse from the API endpoint. The Response was {result} ");

                        Assert.True(flightAccrualResponse.transactionId is not null, $"TransactionId retrieved from flightAccrualResponse API is null. The Response was {result}");

                        string transactionIdInResponse = flightAccrualResponse.transactionId;

                        txnId = transactionIdInResponse;

                        #endregion

                        #region Retrieving transaction created from the cosmos DB


                        string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.TransactionId = @transactionId";

                        QueryDefinition txnQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@customerId", customerId).WithParameter("@transactionId", transactionIdInResponse);

                        Transaction CustomerTxnDocument = _cosmosFixture.CosmosQueryItem<Transaction>(txnQuery);


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

                        isTransactionCreationSuccessful = true;

                        break;

                    }

                case TransactionProduct.Authorised_Points_Credit or TransactionProduct.Points_Misquote:
                    {
                        #region Test Data                        

                        string createdByUserName = DataGenerator.RandomFirstName();

                        string createdByEmailId = createdByUserName + "@testemail.co.ic";

                        #endregion

                        #region VRed Balance API Call for Member Before Manual Credit Txn

                        HttpResponseMessage vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                        dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                        int usableBalance = Convert.ToInt32(vRedResult.usable.ToString());

                        #endregion

                        #region API Call


                        HttpResponseMessage response = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(memberNumber, partnerCode, transactionProduct, activityDate.ToString("dd-MMM-yyyy"), "Test txn", points, 0, "AutomationScript", createdByEmailId, createdByUserName, customerId, bookingReference).EnsureIsSuccessStatusCode();


                        #endregion

                        #region VRed Balance API Call for Member After Manual Credit Txn

                        vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                        vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                        int memberUsableBalanceAfterAdjustment = Convert.ToInt32(vRedResult.usable.ToString());

                        int retry = 5;

                        while (retry > 0 && memberUsableBalanceAfterAdjustment == usableBalance)
                        {
                            await Task.Delay(1000);

                            vRedResponse = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                            vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

                            memberUsableBalanceAfterAdjustment = Convert.ToInt32(vRedResult.usable.ToString());

                            retry--;
                        }

                        Assert.True(memberUsableBalanceAfterAdjustment == usableBalance + points,
                           $"The Sender Member {memberNumber} Usable balance after the successful Manual Credit Adjustment is not as expected. Expected the Usable balance to be  {usableBalance + points} but  was {memberUsableBalanceAfterAdjustment}.");

                        #endregion

                        isTransactionCreationSuccessful = true;

                        break;
                    }

                default:
                    Assert.Fail("Please Provide a Valid Transaction Product.");
                    break;
            }

            if (isTransactionCreationSuccessful)
                txnStatus = "Processed";

            return new CreateTransactionResponse
            {
                memberNumber = memberNumber,
                customerId = customerId,
                transactionStatus = txnStatus,
                transactionId = txnId,
                transactionProduct = product,
                transactionType = txnType,
                partnerId = partnerID

            };

        }


        public async Task<(string memberNumber, string customerId)> RetrieveValidFlyingClubMember()
        {

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1, @tierCode2) and c.Status in ('Active') order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve a valid member whose status is valid for Create Accrual Transaction");

            HttpResponseMessage memberTxnStatement = Microservice.GetMemberTransactionStatement(_tokenFixture.GetToken("VRedMS"), vRedMSOcpSubKey, fcmDoc.MemberNumber);

            dynamic vRedStatementResponse = null;

            try
            {
                vRedStatementResponse = JsonConvert.DeserializeObject<dynamic>(await memberTxnStatement?.Content?.ReadAsStringAsync());
            }
            catch (Exception)
            {
                vRedStatementResponse = null;
            }

            string memberNumber = fcmDoc.MemberNumber;

            string customerId = fcmDoc.CustomerId;

            if (vRedStatementResponse is null || !DynamicHelper.HasProperty(vRedStatementResponse, "statements"))
            {
                string[] titles = { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
                string postCode = "RH11 9LG";
                string country = "GB";
                string channelID = "POWERAPP";

                #region PreRequisite to create an enrolment before validations for the enrolment

                Random random = new();
                string title = titles[random.Next(titles.Length)];
                string firstName = DataGenerator.RandomFirstName();
                string lastName = DataGenerator.RandomLastName();
                string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";

                string enroledBy = "LOOPSScript@fly.virgin.com";

                DateTime endDate = DateTime.Now.AddDays(-6600);

                string dateOfBirth = DataGenerator.RandomDateBetweenTwoDates(endDate.AddDays(-7000), endDate).ToString("yyyy-MM-dd");

                (memberNumber, customerId) = await preRequisiteTestData.MemberEnrolment(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

                Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolmentresponse is null/empty.");

                #endregion

            }

            return (memberNumber, customerId);
        }
    }
}

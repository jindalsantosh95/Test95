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
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.RetailTherapy
{
    [Collection("RetailTherapyTransactionTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetailTherapyTransactionTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string memberPassword = Application.LogInPassword;

        public RetailTherapyTransactionTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;
            preRequisiteTestData = new();

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, "CustomerDB_Acc", "CustomerSearch");
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
            this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_CreateEarnTransaction_SuccessScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomNumber(1, 10000, true);

            int points = int.Parse(DataGenerator.RandomNumber(0, 99, true));

            HttpResponseMessage response = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.Value is not null, $"The Response is not as expected , The response returned is {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.ToString().Equals(orderNumber), $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber}  , but was {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.vaaTransactionId?.ToString().StartsWith("AZ"), $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number but was {createTransactionResult}");

            #endregion


            #region Retreiving VRED Balance for the member           

            HttpResponseMessage memberBalanceResponse = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

            dynamic accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

            long memberPointBalance = accountMemberDetailsResponse?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

            int retry = 10;

            while (retry > 0 && !memberPointBalance.Equals(points))
            {
                await Task.Delay(2000);

                memberBalanceResponse = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

                accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

                memberPointBalance = accountMemberDetailsResponse?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

                retry--;
            }

            Assert.True(memberPointBalance.Equals(points), $"The accountMemberDetailsResponse returned the total virgin point balance {memberPointBalance} and the expected point balance is :{points}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_CreateEarnTransactionWithoutUserTokenInHeader_SuccessScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureGQLResponseSuccess();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            string customerId = enrolmentResult.RequireToken("data.accountEnrolMember.customerId").ToString();
            string memberNumber = enrolmentResult.RequireToken("data.accountEnrolMember.memberDetails.memberNumber").ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), "Enrolment response did not contain a customer ID.");
            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Enrolment response did not contain a member number.");

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = null;

            #endregion

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage response = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();


            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.Value is not null, $"The Response is not as expected , The response returned is {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.ToString().Equals(orderNumber), $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber}  , but was {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.vaaTransactionId?.ToString().StartsWith("AZ"), $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number but was {createTransactionResult}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_EarnTransaction_InvalidFutureDateErrorScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityFutureDate = DataGenerator.RandomFutureDate(10);

            string activityDate = activityFutureDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage response = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected error response from Graphql for future activityDate {activityDate} but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.errors[0]?.message?.ToString().Equals("Invalid Activity Date"),
                $"The CreateEarnTransactionForRetailTherapy was a future created as expected. Expected the response to be 'Invalid Activity Date' but was {createTransactionResult}");

            Assert.True(createTransactionResult?.errors[0]?.extensions?.validationRule?.ToString().Contains("Invalid Activity Date"),
                $"The CreateEarnTransactionForRetailTherapy was a future created as expected. Expected the response to be 'Invalid Activity Date but was {createTransactionResult}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_CreateBurnTransaction_SuccessScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region awarding sufficient points to enable redemptions

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy");

            DateTime activityPastDate = ticketIssuePastDate.AddDays(1);

            string activityDt = activityPastDate.ToString("MM/dd/yyyy");

            string createdOn = DateTime.Now.ToString("MM/dd/yyyy");

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            HttpResponseMessage txnresponse = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(pnr, "J", activityDt, "VS", ticketIssueDate, "LHR", "301", "JFK", objCustomer.LoyaltyMemberNumber, customerId, createdOn, "testingComments", "99999990000").EnsureIsSuccessStatusCode();

            string txnResult = await txnresponse.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(txnResult);

            Assert.True(flightAccrualResponse is not null, $"Pre-requsite Transaction for awarding pts to the member not processed with message {flightAccrualResponse.message}");

            Assert.True(flightAccrualResponse.transactionId is not null, $"Pre-requsite Transaction for awarding pts to the member not processed with message {flightAccrualResponse.message}");

            #endregion

            #region Validating member point balance 

            HttpResponseMessage memberBalanceResponse = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

            dynamic accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

            long memberPointBalance = accountMemberDetailsResponse?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

            int retry = 5;

            while (retry > 0 && memberPointBalance == 0)
            {
                await Task.Delay(2000);

                memberBalanceResponse = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

                accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

                memberPointBalance = accountMemberDetailsResponse?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

                retry--;
            }

            Assert.True(memberPointBalance >= 0, "The member point balance is 0. Cannot creat the Burn transaction. Issue with FA Txn syncc with Vred");

            #endregion

            #region Creating - CreateBurnTransactionForRetailTherapy for loyalty accounts

            DateTime activityFutureDate = DataGenerator.RandomFutureDate(20);

            string activityDate = activityFutureDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 50));

            HttpResponseMessage response = GraphQL.CreateBurnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateBurnTransaction?.orderId?.Value is not null, $"The Response is not as expected , The response returned is {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateBurnTransaction?.orderId?.ToString().Equals(orderNumber), $"The CreateBurnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber}  , but was {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateBurnTransaction?.vaaTransactionId?.ToString().StartsWith("AZ"), $"The CreateBurnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number but was {createTransactionResult}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_CreateBurnTransaction_ValidateInsufficientPointBalanceScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region Creating - CreateBurnTransactionForRetailTherapy for loyalty accounts

            DateTime activityFutureDate = DataGenerator.RandomFutureDate(20);

            string activityDate = activityFutureDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 50));

            HttpResponseMessage response = GraphQL.CreateBurnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.errors[0]?.message?.Value?.Equals("Insufficient Point balance"), $"The CreateBurnTransactionForRetailTherapy didnot throw error message - Insufficient Point balance as expected and was {createTransactionResult}");

            Assert.True(createTransactionResult?.errors[0]?.extensions?.validationRule?.ToString().Equals("Insufficient Point balance"), $"The CreateBurnTransactionForRetailTherapy didnot throw error message - Insufficient Point balance as expected and was {createTransactionResult}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task TC06_CancelRetailTherapyTransaction_SuccessScenario()
        {
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion


            #region Retreiving VRED Balance for the member before earn txn is created

            HttpResponseMessage memberBalanceResponse = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

            dynamic accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await memberBalanceResponse.Content.ReadAsStringAsync());

            long memberCurrentPointBalance = accountMemberDetailsResponse?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage responseEarnTxn = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await responseEarnTxn.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.ToString().Equals(orderNumber), $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber}  , but was {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.vaaTransactionId?.ToString().StartsWith("AZ"), $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number but was {createTransactionResult}");

            #endregion

            #endregion

            #region cancel the earn txn created and Validations

            string cancellationReason = "Incorrect order placed";

            string transactionId = createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString();

            HttpResponseMessage cancelResponse = GraphQL.CancelTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, cancellationReason, "RetailTherapy", objCustomer.LoyaltyMemberNumber, transactionId).EnsureIsSuccessStatusCode();

            dynamic cancelTransactionResult = JsonConvert.DeserializeObject<dynamic>(await cancelResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            int retry = 3;

            while (retry > 0 && cancelTransactionResult.ToString().Contains("Invalid Transaction Id"))
            {
                await Task.Delay(3000);

                cancelResponse = GraphQL.CancelTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, cancellationReason, "RetailTherapy", objCustomer.LoyaltyMemberNumber, transactionId).EnsureIsSuccessStatusCode();

                cancelTransactionResult = JsonConvert.DeserializeObject<dynamic>(await cancelResponse.Content.ReadAsStringAsync());

                retry--;
            }

            Assert.True(cancelTransactionResult?.data?.loyaltyCancelTransaction?.message?.ToString().Contains("Request processed successfully"),
                $"The loyaltyCancelTransactionForRetailTherapy was not created as expected. Expected the message to contain -  Request processed successfully  , but was {createTransactionResult.ToString()}");

            #endregion

            #region Retreiving VRED Balance for the member after cancellation of the txn

            HttpResponseMessage responseAfterCancellation = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

            dynamic accountMemberDetailsResponseAfterCancellation = JsonConvert.DeserializeObject<dynamic>(await responseAfterCancellation.Content.ReadAsStringAsync());

            long newMemberPointBalance = accountMemberDetailsResponseAfterCancellation?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

            retry = 10;

            while (retry > 0 && newMemberPointBalance != memberCurrentPointBalance)
            {
                await Task.Delay(2000);

                responseAfterCancellation = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

                accountMemberDetailsResponseAfterCancellation = JsonConvert.DeserializeObject<dynamic>(await responseAfterCancellation.Content.ReadAsStringAsync());

                newMemberPointBalance = accountMemberDetailsResponseAfterCancellation?.data?.accountMemberDetails?.flyingClubMemberDetails?.totalVirginPoints ?? 0;

                retry--;
            }

            Assert.True(newMemberPointBalance == memberCurrentPointBalance,
                $"The member Virgin Point balance did not match. The Virgin Point balance before earn txn and cancellation was:                {memberCurrentPointBalance} after the cancellation of the txn the Virgin point balance is : {newMemberPointBalance}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task TC07_CancelRetailTherapyTransaction_IncorrectTransactionIdScenario()
        {
            #region PreRequisite to include Enrolment of a new member, and creating an earn txn before we cancel the same 

            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region Retreiving VRED Balance for the member before earn txn is created

            HttpResponseMessage response = GraphQL.RetrieveMemberBalanceForRetailTherapy(tokenForMemberAccountDetails).EnsureIsSuccessStatusCode();

            dynamic accountMemberDetailsResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(accountMemberDetailsResponse.data.accountMemberDetails.flyingClubMemberDetails.totalVirginPoints.ToString() is not null,
                $"The accountMemberDetailsResponse didnot return the total virgin point balance for the member as expected. " +
                $" and was {accountMemberDetailsResponse}");

            #endregion

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = int.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage responseEarnTxn = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(await responseEarnTxn.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(createTransactionResult, "errors"), $"Expected a valid response from Graphql but found \n {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.orderId?.ToString().Equals(orderNumber),
                $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be" +
                $" {orderNumber}  , but was {createTransactionResult}");

            Assert.True(createTransactionResult?.data?.loyaltyCreateEarnTransaction?.vaaTransactionId?.ToString().StartsWith("AZ"),
                $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number" +
                $" but was {createTransactionResult}");

            #endregion

            #endregion

            #region try to cancel the earn txn with incorrect txn id and Validations

            string cancellationReason = "Incorrect txn ID sceanario tested";

            string transactionId = createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString();

            transactionId += "INCORRECT";

            HttpResponseMessage cancelResponse = GraphQL.CancelTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, cancellationReason, "RetailTherapy", objCustomer.LoyaltyMemberNumber, transactionId).EnsureIsSuccessStatusCode();

            dynamic cancelTransactionResult = JsonConvert.DeserializeObject<dynamic>(await cancelResponse.Content.ReadAsStringAsync());

            Assert.True(cancelTransactionResult?.errors[0]?.message?.ToString().Contains("Invalid Transaction Id"), $"The loyaltyCancelTransactionForRetailTherapy result was not as as expected. Expected the message to contain - Invalid Transaction Id  , but was {createTransactionResult}");

            Assert.True(cancelTransactionResult?.errors[0]?.extensions?.code?.ToString().Contains("400"), $"The loyaltyCancelTransactionForRetailTherapy result code was not as as expected. Expected the code  to contain -  400  , but was {createTransactionResult}");

            Assert.True(cancelTransactionResult?.errors[0]?.extensions?.validationRule?.ToString().Contains("Invalid Transaction Id"),
               $"The loyaltyCancelTransactionForRetailTherapy result was not as as expected. Expected the validationRule to contain - Invalid Transaction Id  , but was {createTransactionResult}");

            #endregion
        }

        private void GetCustomerDetailsFromFcNumber(string customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue)
        {
            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{customerId}' and c.DocType='Customer'";

            objCustomer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(strInputQueryForCustomerSearch);
            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                Thread.Sleep(5000);
                objCustomer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            expiryValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds();
        }

        private void TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode)
        {
            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            title = titles[random.Next(titles.Length)];
            firstName = DataGenerator.RandomFirstName();
            lastName = DataGenerator.RandomFirstName();
            emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            postCode = "RH11 9LG";
            country = "GB";
            channelId = null;
            enroledBy = Application.AutomationScriptCreatedBy;
            dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");
            enrollmentChannel = "WEB-MYA";
            mediaCode = "Web";
        }
    }
}

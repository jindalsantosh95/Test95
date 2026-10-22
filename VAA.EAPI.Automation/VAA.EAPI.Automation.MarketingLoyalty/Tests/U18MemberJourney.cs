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
using CustomerEntity = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests
{
    [Collection("U18Journey")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class U18Journey : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>, IDisposable
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly TokenFixture tokenFixture;
        private static readonly string env = Config.Environment;
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string ocpSubKey;
        private U18JourneyState createdHousehold;
        private bool householdLinkingRequested;
        public U18Journey(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerTransactionCosmosDb = env.Is("uat")
                ? Application.CustomerTransactionCosmosDbUAT
                : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat")
                ? Application.CustomerCosmosDbUAT
                : Application.CustomerCosmosDbSIT;

            string cosmosEndpointForCustomerDB = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerDB, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerEntity>(cosmosEndpointForCustomerDB, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<ContactHistory>(cosmosEndpointForCustomerDB, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            this.tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            this.tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            this.tokenFixture.AddTokenForLoyaltyMemberMicroserviceFromKeyVault("LoyaltyMbrMS");

            this.tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

        }

        // Covers the legacy PowerApp minor-member enrolment flow.
        private static string CreateLegacyMinorMember()
        {
            bool isMale = Random.Shared.Next(2) == 0;
            string title = isMale ? "Mstr" : "Miss";
            string gender = isMale ? "M" : "F";
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = DataGenerator.GenerateRandomEmail(12, "u18.virgin.com");
            string addressType = "Home";
            string addressLine1 = $"Test AddressLine1 {DataGenerator.RandomString(5)}";
            string city = $"Test City {DataGenerator.RandomString(5)}";
            string postCode = "RH11 9LG";
            string country = "GB";
            string dateOfBirth = DateTime.UtcNow.AddYears(-17).ToString("yyyy-MM-dd");
            string phoneCountryCode = "44";
            string phoneNumber = DataGenerator.RandomNumber(1000000000, 9999999999, trimLeadingZeros: true);
            string guardianName = $"{DataGenerator.RandomFirstName()} {DataGenerator.RandomLastName()}";
            string guardianDateOfBirth = DataGenerator.RandomDateOfBirth().ToString("yyyy-MM-dd");
            string guardianPhoneCountryCode = "44";
            string guardianPhoneNumber = DataGenerator.RandomNumber(1000000000, 9999999999, trimLeadingZeros: true);
            string guardianEmailId = DataGenerator.GenerateRandomEmail(12, "test.com");

            using HttpResponseMessage response = LoopsFrontEndAPIEndpoints.MemberEnrolment(
                title, gender, firstName, lastName, emailId, addressType, addressLine1, city, postCode, country, dateOfBirth, phoneCountryCode, phoneNumber, guardianName, guardianDateOfBirth, guardianPhoneCountryCode, guardianPhoneNumber, guardianEmailId).EnsureSuccessStatusCode();

            return emailId;
        }

        private async Task<U18JourneyState> CreateLinkedU18HouseholdAsync()
        {
            string childEmailId = CreateLegacyMinorMember();
            const int maxAttempts = 10;
            const string queryForCustomer =
                "SELECT TOP 1 * FROM CustomerCollection c WHERE c.DocType = @docType AND EXISTS (SELECT VALUE contact FROM contact IN c.ContactDetails WHERE contact.ContactType = @contactType AND contact.ContactValue = @emailId)";

            QueryDefinition childCustomerQuery = new QueryDefinition(queryForCustomer)
                .WithParameter("@docType", "Customer")
                .WithParameter("@contactType", "Email")
                .WithParameter("@emailId", childEmailId);

            CustomerEntity childCustomer = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity candidate = cosmosFixture.CosmosQueryItem<CustomerEntity>(childCustomerQuery);

                if (candidate is not null &&
                    !candidate.CustomerId.IsNullOrWhiteSpace() &&
                    !candidate.LoyaltyMemberNumber.IsNullOrWhiteSpace())
                {
                    childCustomer = candidate;
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childCustomer);
            string childAccountCustomerId = childCustomer.CustomerId;
            string childAccountMemberNumber = childCustomer.LoyaltyMemberNumber;

            Assert.False(childAccountCustomerId.IsNullOrWhiteSpace(),
                "Minor enrolment did not persist a customer ID.");
            Assert.False(childAccountMemberNumber.IsNullOrWhiteSpace(),
                "Minor enrolment did not persist a member number.");

            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out string enrollmentChannel, out string mediaCode);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureGQLResponseSuccess();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            JObject enrolmentData = enrolmentResult["data"] as JObject;
            Assert.NotNull(enrolmentData);

            JObject accountEnrolMember = enrolmentData["accountEnrolMember"] as JObject;
            Assert.NotNull(accountEnrolMember);

            string hOHCustomerId = enrolmentResult.RequireToken("data.accountEnrolMember.customerId").ToString();
            string hOHMemberNumber = enrolmentResult.RequireToken("data.accountEnrolMember.memberDetails.memberNumber").ToString();

            Assert.False(hOHCustomerId.IsNullOrWhiteSpace(), "customerId returned from enrolment response is null/empty.");
            Assert.False(hOHMemberNumber.IsNullOrWhiteSpace(), "memberNumber returned from enrolment response is null/empty.");

            createdHousehold = new U18JourneyState
            {
                ChildAccountCustomerId = childAccountCustomerId,
                ChildAccountMemberNumber = childAccountMemberNumber,
                HeadOfHouseholdCustomerId = hOHCustomerId,
                HeadOfHouseholdMemberNumber = hOHMemberNumber
            };

            const string queryForFlyingClubMember =
                "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType";

            QueryDefinition childFlyingClubMemberQuery = new QueryDefinition(queryForFlyingClubMember)
                .WithParameter("@customerId", childAccountCustomerId)
                .WithParameter("@docType", VAA.EAPI.Automation.Core.Constants.Application.DocTypeFlyingClubMember);

            bool childFlyingClubMemberExists = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (cosmosFixture.CosmosQueryItem<FlyingClubMember>(childFlyingClubMemberQuery) is not null)
                {
                    childFlyingClubMemberExists = true;
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.True(childFlyingClubMemberExists,
                $"Timed out waiting for the Flying Club member record for {childAccountCustomerId}.");

            QueryDefinition headOfHouseholdFlyingClubMemberQuery = new QueryDefinition(queryForFlyingClubMember)
                .WithParameter("@customerId", hOHCustomerId)
                .WithParameter("@docType", VAA.EAPI.Automation.Core.Constants.Application.DocTypeFlyingClubMember);

            bool headOfHouseholdFlyingClubMemberExists = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (cosmosFixture.CosmosQueryItem<FlyingClubMember>(headOfHouseholdFlyingClubMemberQuery) is not null)
                {
                    headOfHouseholdFlyingClubMemberExists = true;
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.True(headOfHouseholdFlyingClubMemberExists,
                $"Timed out waiting for the Flying Club member record for {hOHCustomerId}.");

            string initiatedBy = "automationScript@fly.virgin.com";

            householdLinkingRequested = true;
            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), childAccountCustomerId, childAccountMemberNumber, hOHCustomerId, hOHMemberNumber, initiatedBy).EnsureGQLResponseSuccess();

            JObject linkingResult = JObject.Parse(await response.Content.ReadAsStringAsync());

            string responseContent = linkingResult.RequireToken("data.loyaltyHouseholdLinking").ToString();
            JObject responseMessage = JObject.Parse(responseContent);

            Assert.Equal("Request successfully initiated", responseMessage["message"]?.Value<string>());

            U18JourneyState journeyState = new U18JourneyState
            {
                ChildAccountCustomerId = childAccountCustomerId,
                ChildAccountMemberNumber = childAccountMemberNumber,
                HeadOfHouseholdCustomerId = hOHCustomerId,
                HeadOfHouseholdMemberNumber = hOHMemberNumber,
                Message = responseMessage["message"]?.Value<string>() ?? string.Empty
            };

            return journeyState;
        }

        public void Dispose()
        {
            if (createdHousehold is null || !householdLinkingRequested)
            {
                return;
            }

            using HttpResponseMessage response = GraphQL.HouseHoldDelinking(
                tokenFixture.GetToken("graphqlv2"),
                createdHousehold.ChildAccountCustomerId,
                createdHousehold.ChildAccountMemberNumber,
                createdHousehold.HeadOfHouseholdCustomerId,
                createdHousehold.HeadOfHouseholdMemberNumber,
                "U18JourneyCleanup").EnsureGQLResponseSuccess();

            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject delinkingResponse = JObject.Parse(result.RequireToken("data.loyaltyHouseholdDelinking").ToString());
            Assert.Equal(
                "Request successfully initiated",
                delinkingResponse.RequireToken("message").Value<string>());
        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_LinkToHouseHold_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            const string queryForFlyingClubMember =
                "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType";

            QueryDefinition childFlyingClubMemberQuery = new QueryDefinition(queryForFlyingClubMember)
                .WithParameter("@customerId", journeyState.ChildAccountCustomerId)
                .WithParameter("@docType", "FlyingClubMember");

            bool childLinkedToHeadOfHousehold = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                FlyingClubMember member = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childFlyingClubMemberQuery);

                if (member is not null &&
                    member.HeadOfGroupMemberNumber == journeyState.HeadOfHouseholdMemberNumber)
                {
                    childLinkedToHeadOfHousehold = true;
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.True(childLinkedToHeadOfHousehold,
                $"Timed out waiting for child {journeyState.ChildAccountCustomerId} to be linked to household head {journeyState.HeadOfHouseholdMemberNumber}.");

            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(journeyState);
            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");
            Assert.Equal("Request successfully initiated", journeyState.Message);
        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_RetrieveHouseHold_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_RetrieveHouseHoldAfterEmailUpdate_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

            const string queryForCustomerDoc =
                "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition customerQuery = new QueryDefinition(queryForCustomerDoc)
                .WithParameter("@docType", "Customer")
                .WithParameter("@partitionKey", journeyState.ChildAccountCustomerId);

            ContactDetails emailDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(customerQuery);
                emailDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Email" && contact.Primary == "Y");

                if (emailDetails is not null)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(emailDetails);
            string emailContactId = emailDetails.ContactId;

            string updatedBy = DataGenerator.RandomFirstName();

            string newEmailAddress = DataGenerator.RandomString(10).ToString().ToLower() + "@testmail.noreply.co.in";

            HttpResponseMessage emailUpdateResponse = LoopsBackendAPIEndpoints.Customer.UpdateMemberEmail(tokenFixture.GetToken("customer"), journeyState.ChildAccountCustomerId, newEmailAddress, emailContactId, updatedBy).EnsureSuccessStatusCode();

            string emailUpdateResult = await emailUpdateResponse.Content.ReadAsStringAsync();
            const string expectedEmailUpdateMessage = "Details have been saved successfully";
            Assert.True(emailUpdateResult.Contains(expectedEmailUpdateMessage),
                $"Expected the response to be {expectedEmailUpdateMessage} but was {emailUpdateResult}.");

            ContactDetails persistedEmailDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(customerQuery);
                persistedEmailDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Email" && contact.Primary == "Y");

                if (persistedEmailDetails is not null &&
                    persistedEmailDetails.ContactValue.Is(newEmailAddress))
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(persistedEmailDetails);
            Assert.True(persistedEmailDetails.ContactValue.Is(newEmailAddress),
                $"Expected the primary email to be {newEmailAddress} but was {persistedEmailDetails.ContactValue}.");

            JObject childHouseholdMemberAfterEmailUpdate = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMemberAfterEmailUpdate = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMemberAfterEmailUpdate?["relationshipType"]?.Value<string>() == "Linked Member")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMemberAfterEmailUpdate);
            Assert.Equal("Linked Member", childHouseholdMemberAfterEmailUpdate.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMemberAfterEmailUpdate.Value<string>("customerId")),
                "Child household member not found");

        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_UpdatingHoHPhoneNumberUpdatesLinkedU18ChildPhoneNumberAlso_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

            const string queryForCustomerDoc =
                "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition customerQuery = new QueryDefinition(queryForCustomerDoc)
                .WithParameter("@docType", "Customer")
                .WithParameter("@partitionKey", journeyState.HeadOfHouseholdCustomerId);

            ContactDetails phoneDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(customerQuery);
                phoneDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Phone" && contact.Primary == "Y");

                if (phoneDetails is not null)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(phoneDetails);
            string phoneContactId = phoneDetails.ContactId;

            string newPhoneNumber = DataGenerator.RandomPhoneNumber().ToString();

            string startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string updatedBy = "Test" + DataGenerator.RandomFirstName();

            HttpResponseMessage contactNumberResponse = LoopsBackendAPIEndpoints.Customer.UpdateMemberPhone(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId, newPhoneNumber, startDate, phoneContactId, updatedBy).EnsureIsSuccessStatusCode();

            string contactNumberResponseContent = await contactNumberResponse.Content.ReadAsStringAsync();

            Assert.NotNull(contactNumberResponseContent);

            string expectedResponse = "Details have been saved successfully";

            Assert.True(contactNumberResponseContent.Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {contactNumberResponseContent}");

            HttpResponseMessage headOfHouseholdRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId).EnsureSuccessStatusCode();

            JObject headOfHouseholdRetrievalContent = JObject.Parse(await headOfHouseholdRetrieval.Content.ReadAsStringAsync());

            JObject hOHPhoneDetails = headOfHouseholdRetrievalContent
                .SelectToken("customerDetails.contactDetails.phoneDetails")
                ?.Children<JObject>()
                .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

            Assert.NotNull(hOHPhoneDetails);
            Assert.Equal(newPhoneNumber, hOHPhoneDetails.Value<string>("phoneNumber"));

            JObject childPrimaryPhone = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage childAccountRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(
                    tokenFixture.GetToken("customer"),
                    journeyState.ChildAccountCustomerId).EnsureSuccessStatusCode();

                JObject childAccountRetrievalResponse = JObject.Parse(
                    await childAccountRetrieval.Content.ReadAsStringAsync());

                childPrimaryPhone = childAccountRetrievalResponse
                    .SelectToken("customerDetails.contactDetails.phoneDetails")
                    ?.Children<JObject>()
                    .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

                if (childPrimaryPhone?.Value<string>("phoneNumber") == newPhoneNumber)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childPrimaryPhone);
            Assert.Equal(newPhoneNumber, childPrimaryPhone.Value<string>("phoneNumber"));

            Assert.Equal(
                hOHPhoneDetails.Value<string>("countryCode"),
                childPrimaryPhone.Value<string>("countryCode"));
            Assert.Equal(
                hOHPhoneDetails.Value<string>("phoneNumber"),
                childPrimaryPhone.Value<string>("phoneNumber"));
        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_UpdateHoHPhoneNumberAfterTransitionToNonU18Domain_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

            const string queryForCustomerDoc =
                "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition customerQuery = new QueryDefinition(queryForCustomerDoc)
                .WithParameter("@docType", "Customer")
                .WithParameter("@partitionKey", journeyState.ChildAccountCustomerId);

            ContactDetails emailDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(customerQuery);
                emailDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Email" && contact.Primary == "Y");

                if (emailDetails is not null)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(emailDetails);
            string emailContactId = emailDetails.ContactId;

            string newEmailAddress = DataGenerator.RandomString(10).ToString().ToLower() + "@testmail.noreply.co.in";

            string updatedBy = "Test" + DataGenerator.RandomFirstName();

            HttpResponseMessage updateChildEmail = LoopsBackendAPIEndpoints.Customer.UpdateMemberEmail(tokenFixture.GetToken("customer"), journeyState.ChildAccountCustomerId, newEmailAddress, emailContactId, updatedBy).EnsureIsSuccessStatusCode();

            JObject childHouseholdMemberAfterEmailUpdate = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMemberAfterEmailUpdate = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMemberAfterEmailUpdate?["relationshipType"]?.Value<string>() == "Linked Member")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMemberAfterEmailUpdate);
            Assert.Equal("Linked Member", childHouseholdMemberAfterEmailUpdate.Value<string>("relationshipType"));

            HttpResponseMessage childAccountRetrievalBeforePhoneUpdate = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(
                tokenFixture.GetToken("customer"),
                journeyState.ChildAccountCustomerId).EnsureSuccessStatusCode();

            JObject childAccountBeforePhoneUpdate = JObject.Parse(
                await childAccountRetrievalBeforePhoneUpdate.Content.ReadAsStringAsync());

            JObject childPrimaryPhoneBeforePhoneUpdate = childAccountBeforePhoneUpdate
                .SelectToken("customerDetails.contactDetails.phoneDetails")
                ?.Children<JObject>()
                .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

            Assert.NotNull(childPrimaryPhoneBeforePhoneUpdate);
            string originalChildPhoneNumber = childPrimaryPhoneBeforePhoneUpdate.Value<string>("phoneNumber");

            const string queryForHOHCustomerDoc =
                "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition hOHCustomerQuery = new QueryDefinition(queryForHOHCustomerDoc)
                .WithParameter("@docType", "Customer")
                .WithParameter("@partitionKey", journeyState.HeadOfHouseholdCustomerId);

            ContactDetails phoneDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(hOHCustomerQuery);
                phoneDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Phone" && contact.Primary == "Y");

                if (phoneDetails is not null)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(phoneDetails);
            string phoneContactId = phoneDetails.ContactId;

            string newPhoneNumber = DataGenerator.RandomPhoneNumber().ToString();

            string startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string phoneUpdatedBy = "Test" + DataGenerator.RandomFirstName();

            HttpResponseMessage contactNumberResponse = LoopsBackendAPIEndpoints.Customer.UpdateMemberPhone(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId, newPhoneNumber, startDate, phoneContactId, phoneUpdatedBy).EnsureIsSuccessStatusCode();

            string contactNumberResponseContent = await contactNumberResponse.Content.ReadAsStringAsync();

            Assert.NotNull(contactNumberResponseContent);

            string expectedResponse = "Details have been saved successfully";

            Assert.True(contactNumberResponseContent.Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {contactNumberResponseContent}");

            HttpResponseMessage headOfHouseholdRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId).EnsureSuccessStatusCode();

            JObject headOfHouseholdRetrievalContent = JObject.Parse(await headOfHouseholdRetrieval.Content.ReadAsStringAsync());

            JObject hOHPhoneDetails = headOfHouseholdRetrievalContent
                .SelectToken("customerDetails.contactDetails.phoneDetails")
                ?.Children<JObject>()
                .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

            Assert.NotNull(hOHPhoneDetails);
            Assert.Equal(newPhoneNumber, hOHPhoneDetails.Value<string>("phoneNumber"));

            JObject childPrimaryPhone = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage childAccountRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(
                    tokenFixture.GetToken("customer"),
                    journeyState.ChildAccountCustomerId).EnsureSuccessStatusCode();

                JObject childAccountRetrievalResponse = JObject.Parse(
                    await childAccountRetrieval.Content.ReadAsStringAsync());

                childPrimaryPhone = childAccountRetrievalResponse
                    .SelectToken("customerDetails.contactDetails.phoneDetails")
                    ?.Children<JObject>()
                    .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

                if (childPrimaryPhone?.Value<string>("phoneNumber") == newPhoneNumber)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childPrimaryPhone);

            Assert.Equal(
                originalChildPhoneNumber,
                childPrimaryPhone.Value<string>("phoneNumber"));

        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(6)]
        public async Task TC06_RetrieveU18MemberContactAudit_SuccessScenario()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

            const string queryForCustomerDoc =
                "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition customerQuery = new QueryDefinition(queryForCustomerDoc)
                .WithParameter("@docType", "Customer")
                .WithParameter("@partitionKey", journeyState.HeadOfHouseholdCustomerId);

            ContactDetails phoneDetails = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                CustomerEntity customerDoc = cosmosFixture.CosmosQueryItem<CustomerEntity>(customerQuery);
                phoneDetails = customerDoc?.ContactDetails?
                    .FirstOrDefault(contact => contact.ContactType == "Phone" && contact.Primary == "Y");

                if (phoneDetails is not null)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(phoneDetails);
            string phoneContactId = phoneDetails.ContactId;

            string newPhoneNumber = DataGenerator.RandomPhoneNumber().ToString();

            string startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string updatedBy = "Test" + DataGenerator.RandomFirstName();

            HttpResponseMessage contactNumberResponse = LoopsBackendAPIEndpoints.Customer.UpdateMemberPhone(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId, newPhoneNumber, startDate, phoneContactId, updatedBy).EnsureIsSuccessStatusCode();

            string contactNumberResponseContent = await contactNumberResponse.Content.ReadAsStringAsync();

            Assert.NotNull(contactNumberResponseContent);

            string expectedResponse = "Details have been saved successfully";

            Assert.True(contactNumberResponseContent.Contains(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {contactNumberResponseContent}");

            HttpResponseMessage headOfHouseholdRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(tokenFixture.GetToken("customer"), journeyState.HeadOfHouseholdCustomerId).EnsureSuccessStatusCode();

            JObject headOfHouseholdRetrievalContent = JObject.Parse(await headOfHouseholdRetrieval.Content.ReadAsStringAsync());

            JObject hOHPhoneDetails = headOfHouseholdRetrievalContent
                .SelectToken("customerDetails.contactDetails.phoneDetails")
                ?.Children<JObject>()
                .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

            Assert.NotNull(hOHPhoneDetails);
            Assert.Equal(newPhoneNumber, hOHPhoneDetails.Value<string>("phoneNumber"));

            JObject childPrimaryPhone = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage childAccountRetrieval = LoopsBackendAPIEndpoints.Customer.CustomerRetrieve(
                    tokenFixture.GetToken("customer"),
                    journeyState.ChildAccountCustomerId).EnsureSuccessStatusCode();

                JObject childAccountRetrievalResponse = JObject.Parse(
                    await childAccountRetrieval.Content.ReadAsStringAsync());

                childPrimaryPhone = childAccountRetrievalResponse
                    .SelectToken("customerDetails.contactDetails.phoneDetails")
                    ?.Children<JObject>()
                    .FirstOrDefault(phone => phone.Value<string>("primary") == "Y");

                if (childPrimaryPhone?.Value<string>("phoneNumber") == newPhoneNumber)
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childPrimaryPhone);
            Assert.Equal(newPhoneNumber, childPrimaryPhone.Value<string>("phoneNumber"));

            const string queryForContactHistory =
                "SELECT * FROM CustomerCollection c WHERE c.DocType = @docType AND c.PartitionKey = @partitionKey";

            QueryDefinition childContactHistoryQuery = new QueryDefinition(queryForContactHistory)
                .WithParameter("@docType", "ContactHistory")
                .WithParameter("@partitionKey", journeyState.ChildAccountCustomerId);

            ContactHistory[] phoneHistoryDocs = cosmosFixture
                .CosmosQueryItems<ContactHistory>(childContactHistoryQuery)
                .Where(contact => contact.ContactType == "Phone")
                .OrderByDescending(contact => contact.LastUpdatedOn)
                .ToArray();

            HttpResponseMessage contactAuditResponse = GraphQL.RetrieveContactAudit(
                tokenFixture.GetToken("graphqlv2"),
                journeyState.ChildAccountCustomerId,
                journeyState.ChildAccountMemberNumber).EnsureGQLResponseSuccess();

            JObject contactAuditResult = JObject.Parse(await contactAuditResponse.Content.ReadAsStringAsync());
            Assert.False(
                contactAuditResult.ToString().Contains("Request failed"),
                "Contact-audit operation reported a failed request.");

            IEnumerable<ContactHistory> contactHistories = contactAuditResult
                .RequireToken("data.retrieveContactHistory.contactHistoryAudit")
                .ToObject<IEnumerable<ContactHistory>>();

            Assert.NotNull(contactHistories);
            Assert.NotEmpty(contactHistories);

            ContactHistory[] phoneContactHistories = contactHistories
                .Where(contact => contact.ContactType == "Phone")
                .OrderByDescending(contact => contact.LastUpdatedOn)
                .ToArray();

            Assert.Equal(phoneHistoryDocs.Length + 1, phoneContactHistories.Length);

            ContactHistory updatedChildPhoneAudit = phoneContactHistories.FirstOrDefault(contact =>
                contact.ContactValue == newPhoneNumber && contact.Primary == "Y");

            Assert.NotNull(updatedChildPhoneAudit);

            ContactHistory[] auditHistoryEntries = phoneContactHistories
                .Where(contact => contact.ContactValue != newPhoneNumber)
                .ToArray();

            foreach (ContactHistory phoneHistoryDoc in phoneHistoryDocs)
            {
                ContactHistory auditHistory = auditHistoryEntries.FirstOrDefault(contact =>
                    contact.ContactId == phoneHistoryDoc.ContactId);

                Assert.NotNull(auditHistory);
                Assert.Equal(phoneHistoryDoc.ContactSubType, auditHistory.ContactSubType);
                Assert.Equal(phoneHistoryDoc.ContactValue, auditHistory.ContactValue);
            }

        }

        [Trait("Category", Traits.LoyaltyMemberManagementRegression)]
        [Fact, RunOrder(7)]
        public async Task TC07_RetrieveLinkedMemberTierPoints()
        {
            U18JourneyState journeyState = await CreateLinkedU18HouseholdAsync();
            const int maxAttempts = 10;
            JObject childHouseholdMember = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                HttpResponseMessage response = Microservice.RetrieveHouseholdMemberDetails(
                    tokenFixture.GetToken("LoyaltyMbrMS"),
                    ocpSubKey,
                    journeyState.HeadOfHouseholdCustomerId).EnsureIsSuccessStatusCode();

                JObject household = JObject.Parse(await response.Content.ReadAsStringAsync());
                childHouseholdMember = household["householdMembers"]
                    ?.Children<JObject>()
                    .FirstOrDefault(member =>
                        member["customerId"]?.Value<string>() == journeyState.ChildAccountCustomerId);

                if (childHouseholdMember?["relationshipType"]?.Value<string>() == "Linked Member-Child")
                {
                    break;
                }

                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Assert.NotNull(childHouseholdMember);
            Assert.Equal("Linked Member-Child", childHouseholdMember.Value<string>("relationshipType"));
            Assert.False(string.IsNullOrEmpty(childHouseholdMember.Value<string>("customerId")),
                "Child household member not found");

            HttpResponseMessage responseMessage = GraphQL.RetrieveMemberAccountDetails(tokenFixture.GetToken("graphqlv2"), journeyState.ChildAccountCustomerId, "").EnsureIsSuccessStatusCode();
            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await responseMessage.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JObject loyaltyMemberAccountDetails = result["data"]["loyaltyMemberAccount"];

            Assert.Equal(
                (string)loyaltyMemberAccountDetails["retainPointTotal"],
                (string)childHouseholdMember["retainPointTotal"]);

            Assert.Equal(
                (string)loyaltyMemberAccountDetails["rollingPointTotal"],
                (string)childHouseholdMember["rollingPointTotal"]);
        }
    }

    public sealed class U18JourneyState
    {
        public string ChildAccountCustomerId { get; set; } = string.Empty;
        public string ChildAccountMemberNumber { get; set; } = string.Empty;
        public string HeadOfHouseholdCustomerId { get; set; } = string.Empty;
        public string HeadOfHouseholdMemberNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

}

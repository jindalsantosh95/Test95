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

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{

    [Collection("AuditManagement")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class ContactAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public ContactAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            _tokenFixture = tokenFixture;
            _cosmosFixture = cosmosFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            if (!backendAPIEnv.Is("Project-DEV"))
                _tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<ContactDetails>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<ContactHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(9)]
        [InlineData("customerId")]
        [InlineData("memberNumber")]
        public async Task TC01_ValidateContactAuditHistory_Email_SuccessScenario(string field)
        {

            #region Test Data            

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LastUpdatedBy <> @channel order by c.LastUpdatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve Customer Document");

            //Current Email Details from Customer Document
            ContactDetails currentEmailDetails = customerDoc.ContactDetails.FirstOrDefault(x => x.ContactType.Equals("Email"));

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string queryForContactHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForContactHistoryDoc).WithParameter("@docType", "ContactHistory").WithParameter("@partitionKey", customerId);

            //Contact History Documents  - Email docs
            ContactHistory[] emailHistoryDocs = _cosmosFixture.CosmosQueryItems<ContactHistory>(query).Where(x => x.ContactType.Equals("Email")).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            switch (field)
            {
                case "customerId":
                    memberNumber = "";
                    break;

                case "memberNumber":
                    customerId = "";
                    break;

                default:
                    break;
            }

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveContactAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();


            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve contact audit graphql operation but found {result.ToString()}");

            IEnumerable<ContactHistory> contactHistories = JsonConvert.DeserializeObject<IEnumerable<ContactHistory>>(result.data.retrieveContactHistory.contactHistoryAudit.ToString());

            Assert.True(contactHistories.Any(),
                "Expected atleast one ContactHistory (Current Contact Details) in the response but found null.");

            contactHistories = contactHistories.Where(x => x.ContactType.Equals("Email")).OrderByDescending(x => x.LastUpdatedOn);

            Assert.True(contactHistories.Count() == emailHistoryDocs.Count() + 1,
                $"Expected the total docs in the response to be {emailHistoryDocs.Count() + 1} but found {contactHistories.Count()}");

            #endregion

            #region Validation

            if (currentEmailDetails is not null && currentEmailDetails.Primary.Equals("Y"))
            {

                Assert.True(currentEmailDetails.ContactSubType == contactHistories.First().ContactSubType,
                   $"Expected the Current Email Subtype to be {currentEmailDetails.ContactSubType} but found {contactHistories.First().ContactSubType} in the response");

                Assert.True(currentEmailDetails.ContactValue == contactHistories.First().ContactValue,
                  $"Expected the Current Email Value to be {currentEmailDetails.ContactValue} but found {contactHistories.First().ContactValue} in the response");

                contactHistories = contactHistories.Skip(1); // Skip Current Email Details from the List after validation 

            }

            int i = 0;

            foreach (ContactHistory contactHistory in contactHistories) // Validation of only history documents
            {

                Assert.True(emailHistoryDocs[i].ContactSubType == contactHistory.ContactSubType,
                     $"For the Contact History doc Id : {emailHistoryDocs[i].id},Expected the Email Subtype to be {emailHistoryDocs[i].ContactSubType} but found {contactHistory.ContactSubType} in the response");

                Assert.True(emailHistoryDocs[i].ContactValue == contactHistory.ContactValue,
                    $"For the Contact History doc Id : {emailHistoryDocs[i].id},Expected the Email Value to be {emailHistoryDocs[i].ContactValue} but found {contactHistory.ContactValue} in the response");

                i++;
            }

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(10)]
        [InlineData("customerId")]
        [InlineData("memberNumber")]
        public async Task TC02_ValidateContactAuditHistory_Phone_SuccessScenario(string field)
        {

            #region Test Data

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LastUpdatedBy <> @channel order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve Customer Document");

            //Current Phone Details from Customer Document
            IEnumerable<ContactDetails> currentPhoneDetails = customerDoc.ContactDetails.Where(x => x.ContactType.Equals("Phone"));

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string queryForContactHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForContactHistoryDoc).WithParameter("@docType", "ContactHistory").WithParameter("@partitionKey", customerId);

            //Contact History Documents  - Phone docs
            ContactHistory[] phoneHistoryDocs = _cosmosFixture.CosmosQueryItems<ContactHistory>(query).Where(x => x.ContactType.Equals("Phone")).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            switch (field)
            {
                case "customerId":
                    memberNumber = "";
                    break;

                case "memberNumber":
                    customerId = "";
                    break;

                default:
                    break;
            }

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveContactAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
                $"Expected a valid response for retrieve contact audit graphql operation but found {result.ToString()}");

            IEnumerable<ContactHistory> contactHistories = JsonConvert.DeserializeObject<IEnumerable<ContactHistory>>(result.data.retrieveContactHistory.contactHistoryAudit.ToString());

            Assert.True(contactHistories.Any(),
                "Expected atleast one ContactHistory (Current Contact Details) in the response but found null.");

            contactHistories = contactHistories.Where(x => x.ContactType.Equals("Phone")).OrderByDescending(x => x.LastUpdatedOn);

            Assert.True(contactHistories.Count() == phoneHistoryDocs.Count() + 1,
                $"Expected the total docs in the response to be {phoneHistoryDocs.Count() + 1} but found {contactHistories.Count()}");

            #endregion

            #region Validation

            if (currentPhoneDetails.Any(x => x.Primary.Equals("Y")))
            {

                Assert.True(currentPhoneDetails.FirstOrDefault(x => x.Primary.Equals("Y")).ContactSubType == contactHistories.First().ContactSubType,
                   $"Expected the Current Phone Subtype to be {currentPhoneDetails.FirstOrDefault(x => x.Primary.Equals("Y")).ContactSubType} but found {contactHistories.First().ContactSubType} in the response");

                Assert.True(currentPhoneDetails.FirstOrDefault(x => x.Primary.Equals("Y")).ContactValue == contactHistories.First().ContactValue,
                  $"Expected the Current Phone Value to be {currentPhoneDetails.FirstOrDefault(x => x.Primary.Equals("Y")).ContactValue} but found {contactHistories.First().ContactValue} in the response");

                contactHistories = contactHistories.Skip(1); // Skip Current Phone Details from the List after validation 

            }

            int i = 0;

            foreach (ContactHistory contactHistory in contactHistories) // Validation of only history documents
            {

                Assert.True(phoneHistoryDocs[i].ContactSubType == contactHistory.ContactSubType,
                     $"For the Contact History doc Id : {phoneHistoryDocs[i].id},Expected the Email Subtype to be {phoneHistoryDocs[i].ContactSubType} but found {contactHistory.ContactSubType} in the response");

                Assert.True(phoneHistoryDocs[i].ContactValue == contactHistory.ContactValue,
                    $"For the Contact History doc Id : {phoneHistoryDocs[i].id},Expected the Email Value to be {phoneHistoryDocs[i].ContactValue} but found {contactHistory.ContactValue} in the response");

                i++;
            }

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(11)]
        public async Task TC03_ValidateContactAuditHistory_AfterEmailUpdate_SuccessScenario()
        {

            #region Test Data            

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LastUpdatedBy <> @channel order by c.LastUpdatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve Customer Document");

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            //Current Email Details from Customer Document before update
            ContactDetails currentPhoneDetailsBeforeUpdate = customerDoc.ContactDetails?.FirstOrDefault(x => x.ContactType.Equals("Email") && x.Primary == "Y");

            Assert.True(currentPhoneDetailsBeforeUpdate is not null, $"No Primary Email Details found for the customer {customerId}");

            string emailContactId = currentPhoneDetailsBeforeUpdate.ContactId;

            string emailAddress = DataGenerator.RandomString(10).ToLower() + "@testemailnoreply.com";

            string updatedBy = DataGenerator.RandomFirstName();

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberEmail(_tokenFixture.GetToken("customer"), customerId, emailAddress, emailContactId, updatedBy).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.Has(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {result}");

            #endregion

            #region Email Validation After the Update

            queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @lastUpdatedBy ";

            paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@lastUpdatedBy", updatedBy);

            customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            int retry = 5;

            while (retry > 0 && customerDoc is null)
            {
                await Task.Delay(1500);

                customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

                retry--;

            }

            Assert.True(customerDoc is not null, $" Failed to retrieve Primary Email for the Member {customerId} after the Email update");

            ContactDetails currentPhoneDetailsAfterUpdate = customerDoc.ContactDetails?.FirstOrDefault(c => c.ContactType == "Email" && c.Primary == "Y");

            Assert.True(currentPhoneDetailsAfterUpdate.ContactValue.Equals(emailAddress),
                $"Expected the EmailAddress to be {emailAddress} but was {currentPhoneDetailsAfterUpdate.ContactValue}");

            Assert.True(currentPhoneDetailsAfterUpdate.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the EmailAddress Update Date to be {DateTime.UtcNow.Date} but was {currentPhoneDetailsAfterUpdate.LastUpdatedOn.Value.Date}");

            #endregion

            #endregion

            #region Audit API Call

            HttpResponseMessage auditResponse = GraphQL.RetrieveContactAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic auditResult = JsonConvert.DeserializeObject<dynamic>(await auditResponse.Content.ReadAsStringAsync());

            Assert.True(!auditResult.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve contact audit graphql operation but found {auditResult.ToString()}");

            IEnumerable<ContactHistory> contactHistories = JsonConvert.DeserializeObject<IEnumerable<ContactHistory>>(auditResult?.data?.retrieveContactHistory?.contactHistoryAudit?.ToString());

            Assert.True(contactHistories.Any(),
                "Expected atleast one ContactHistory (Current Contact Details) in the response but found null.");

            contactHistories = contactHistories.Where(x => x.ContactType.Equals("Email")).OrderByDescending(x => x.LastUpdatedOn);

            #endregion

            #region Validation of updated email details in the response

            if (currentPhoneDetailsAfterUpdate is not null && currentPhoneDetailsAfterUpdate.Primary.Equals("Y"))
            {

                Assert.True(currentPhoneDetailsAfterUpdate.ContactSubType == contactHistories.First().ContactSubType,
                   $"Expected the Current Email Subtype to be {currentPhoneDetailsAfterUpdate.ContactSubType} but found {contactHistories.First().ContactSubType} in the response");

                Assert.True(currentPhoneDetailsAfterUpdate.ContactValue == contactHistories.First().ContactValue,
                  $"Expected the Current Email Value to be {currentPhoneDetailsAfterUpdate.ContactValue} but found {contactHistories.First().ContactValue} in the response");

                contactHistories = contactHistories.Skip(1); // Skip Current Email Details from the List after validation 

            }

            #endregion

            #region Validation of old email details in the response

            Assert.True(currentPhoneDetailsBeforeUpdate.ContactSubType == contactHistories.First().ContactSubType,
                 $"Expected the Email Subtype to be {currentPhoneDetailsBeforeUpdate.ContactSubType} but found {contactHistories.First().ContactSubType} in the response");

            Assert.True(currentPhoneDetailsBeforeUpdate.ContactValue == contactHistories.First().ContactValue,
                $"Expected the Email Value to be {currentPhoneDetailsBeforeUpdate.ContactValue} but found {contactHistories.First().ContactValue} in the response");


            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(12)]
        public async Task TC04_ValidateContactAuditHistory_AfterPhoneUpdate_SuccessScenario()
        {

            #region Test Data            

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LastUpdatedBy <> @channel order by c.LastUpdatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@channel", "WEB");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, "Failed to retrieve Customer Document");

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            //Current Phone Details from Customer Document before update
            ContactDetails currentPhoneDetailsBeforeUpdate = customerDoc.ContactDetails?.FirstOrDefault(x => x.ContactType.Equals("Phone") && x.Primary == "Y");

            Assert.True(currentPhoneDetailsBeforeUpdate is not null, $"No Primary Phone Details found for the customer {customerId}");

            string phoneContactId = currentPhoneDetailsBeforeUpdate.ContactId;

            string phoneNumber = DataGenerator.RandomNumber(100000000, 999999999).ToString();

            string startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string updatedBy = DataGenerator.RandomFirstName();

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberPhone(_tokenFixture.GetToken("customer"), customerId, phoneNumber, startDate, phoneContactId, updatedBy).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.Has(expectedResponse),
                $"Expected the response to be {expectedResponse} but was {result}");

            #endregion

            #region Email Validation After the Update

            queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @lastUpdatedBy ";

            paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId).WithParameter("@lastUpdatedBy", updatedBy);

            customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            int retry = 5;

            while (retry > 0 && customerDoc is null)
            {
                await Task.Delay(1500);

                customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

                retry--;

            }

            Assert.True(customerDoc is not null, $" Failed to retrieve Primary Phone for the Member {customerId} after the Phone update");

            ContactDetails currentPhoneDetailsAfterUpdate = customerDoc.ContactDetails?.FirstOrDefault(c => c.ContactType == "Phone" && c.Primary == "Y");

            Assert.True(currentPhoneDetailsAfterUpdate.ContactValue.Equals(phoneNumber),
                $"Expected the PhoneNumber  to be {phoneNumber} but was {currentPhoneDetailsAfterUpdate.ContactValue}");

            Assert.True(currentPhoneDetailsAfterUpdate.LastUpdatedOn.Value.Date.Equals(DateTime.UtcNow.Date),
                $"Expected the PhoneNumber Update Date to be {DateTime.UtcNow.Date} but was {currentPhoneDetailsAfterUpdate.LastUpdatedOn.Value.Date}");

            #endregion

            #region Audit API Call

            HttpResponseMessage auditResponse = GraphQL.RetrieveContactAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic auditResult = JsonConvert.DeserializeObject<dynamic>(await auditResponse.Content.ReadAsStringAsync());

            Assert.True(!auditResult.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve contact audit graphql operation but found {auditResult.ToString()}");

            IEnumerable<ContactHistory> contactHistories = JsonConvert.DeserializeObject<IEnumerable<ContactHistory>>(auditResult?.data?.retrieveContactHistory?.contactHistoryAudit?.ToString());

            Assert.True(contactHistories.Any(),
                "Expected atleast one ContactHistory (Current Contact Details) in the response but found null.");

            contactHistories = contactHistories.Where(x => x.ContactType.Equals("Phone")).OrderByDescending(x => x.LastUpdatedOn);

            #endregion

            #region Validation of updated Phone details in the response

            if (currentPhoneDetailsAfterUpdate is not null && currentPhoneDetailsAfterUpdate.Primary.Equals("Y"))
            {

                Assert.True(currentPhoneDetailsAfterUpdate.ContactValue == contactHistories.First().ContactValue,
                  $"Expected the Current Phone Value to be {currentPhoneDetailsAfterUpdate.ContactValue} but found {contactHistories.First().ContactValue} in the response");

                contactHistories = contactHistories.Skip(1); // Skip Current Phone Details from the List after validation 

            }

            #endregion

            #region Validation of old phone details in the response

            Assert.True(currentPhoneDetailsBeforeUpdate.ContactValue == contactHistories.First().ContactValue,
                $"Expected the Phone Number Value to be {currentPhoneDetailsBeforeUpdate.ContactValue} but found {contactHistories.First().ContactValue} in the response");


            #endregion

        }
    }
}

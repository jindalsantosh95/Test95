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
    public class AddressAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public AddressAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
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

            _cosmosFixture.RegisterType<Address>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<AddressHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(1)]
        [InlineData("customerId")]
        [InlineData("memberNumber")]
        public async Task TC01_ValidateAddressAuditHistory_SuccessScenario(string field)
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FlyingClubMember Document");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Document for Customer Id : {fcmDoc.CustomerId}");

            //Current Address from Customer Document
            Address currentAddress = customerDoc.AddressDetails.FirstOrDefault();

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string queryForAddressHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForAddressHistoryDoc).WithParameter("@docType", "AddressHistory").WithParameter("@partitionKey", customerId);

            //Address History Documents 
            AddressHistory[] addressHistoryDocs = _cosmosFixture.CosmosQueryItems<AddressHistory>(query).OrderByDescending(x => x.LastUpdatedOn).ToArray();

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

            HttpResponseMessage response = GraphQL.RetrieveAddressAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
                $"Expected a valid response for retrieve Address audit graphql operation but found {result.ToString()}");

            IEnumerable<AddressHistory> addressHistories = JsonConvert.DeserializeObject<IEnumerable<AddressHistory>>(result.data.RetrieveAddressHistory.ToString());

            addressHistories = addressHistories.OrderByDescending(x => x.LastUpdatedOn);

            Assert.True(addressHistories.Count() == addressHistoryDocs.Count() + 1,
                $"Expected the total docs in the response to be {addressHistoryDocs.Count() + 1} but found {addressHistories.Count()}");

            #endregion

            #region Validation


            if (currentAddress is not null && currentAddress.Primary.Equals("Y"))
            {

                Assert.True(currentAddress.AddressType.Equals(addressHistories.First().AddressType),
                   $"Expected the Current AddressType to be {currentAddress.AddressType} but found {addressHistories.First().AddressType} in the response");

                Assert.True(currentAddress.AddressLine1 == addressHistories.First().AddressLine1,
                    $"Expected the Current AddressLine1 to be {currentAddress.AddressLine1} but found {addressHistories.First().AddressLine1} in the response");

                Assert.True(currentAddress.AddressLine2 == addressHistories.First().AddressLine2,
                    $"Expected the Current AddressLine2 to be {currentAddress.AddressLine2} but found {addressHistories.First().AddressLine2} in the response");

                Assert.True(currentAddress.AddressLine3 == addressHistories.First().AddressLine3,
                    $"Expected the Current AddressLine3 to be {currentAddress.AddressLine3} but found {addressHistories.First().AddressLine3} in the response");

                Assert.True(currentAddress.City.Equals(addressHistories.First().City),
                   $"Expected the Current Address City to be {currentAddress.City} but found {addressHistories.First().City} in the response");

                Assert.True(currentAddress.Country.Equals(addressHistories.First().Country),
                   $"Expected the Current Address Country to be {currentAddress.Country} but found {addressHistories.First().Country} in the response");

                Assert.True(currentAddress.PostCode.Equals(addressHistories.First().PostCode),
                   $"Expected the Current Address PostCode to be {currentAddress.Country} but found {addressHistories.First().PostCode} in the response");

                addressHistories = addressHistories.Skip(1); // Skip Current Address from the List after validation 

            }

            int i = 0;

            foreach (AddressHistory addressHistory in addressHistories) // Validation of only history documents
            {

                Assert.True(addressHistoryDocs[i].AddressType == addressHistory.AddressType,
                     $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine1 to be {addressHistoryDocs[i].AddressType} but found {addressHistory.AddressType} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine1 == addressHistory.AddressLine1,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine1 to be {addressHistoryDocs[i].AddressLine1} but found {addressHistory.AddressLine1} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine2 == addressHistory.AddressLine2,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine2 to be {addressHistoryDocs[i].AddressLine2} but found {addressHistory.AddressLine2} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine3 == addressHistory.AddressLine3,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine3 to be {addressHistoryDocs[i].AddressLine3} but found {addressHistory.AddressLine3} in the response");

                Assert.True(addressHistoryDocs[i].City.Equals(addressHistory.City),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address City to be {addressHistoryDocs[i].City} but found {addressHistory.City} in the response");

                Assert.True(addressHistoryDocs[i].Country.Equals(addressHistory.Country),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address Country to be {addressHistoryDocs[i].Country} but found {addressHistory.Country} in the response");

                Assert.True(addressHistoryDocs[i].PostCode.Equals(addressHistory.PostCode),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address PostCode to be {addressHistoryDocs[i].Country} but found {addressHistory.PostCode} in the response");

                i++;
            }

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_ErrorValidationWhenBothCustomerIdAndMemberNumberIsEmpty()
        {
            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveAddressAudit(_tokenFixture.GetToken("graphqlv2"), "", "");

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the Error Message to be {expectedResponse} but found {result.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_ValidateAddressAuditHistoryAfterAddressUpdate_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Failed to retrieve FlyingClubMember Document");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId);

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Failed to retrieve Customer Document for Customer Id : {fcmDoc.CustomerId}");

            //Current Address from Customer Document Before Update
            Address currentAddressBeforeUpdate = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            string customerId = customerDoc.CustomerId;

            string memberNumber = customerDoc.LoyaltyMemberNumber;

            string addressId = currentAddressBeforeUpdate.AddressId;

            #region Calling Update Address API

            Random random = new();

            string addressLine1 = "Test AddressLine1 " + DataGenerator.RandomString(5);

            string addressLine2 = "Test AddressLine2 " + DataGenerator.RandomString(5);

            string addressLine3 = "Test AddressLine3 " + DataGenerator.RandomString(5);

            string company = "TestCompany" + DataGenerator.RandomString(5);

            string startDate = DateTime.Now.ToString("dd-MMM-yyyy");

            string[] postalCodes = { "PH2 3BF", "PH33 3WE", "CO10 4HA", "AB45 7BJ", "DN6 0TA", "TR11 9PY" };

            string[] cities = { "London", "Church Stretton", "Whitton", "New York", "Sydney", "Old Dailly" };

            string[] countries = { "United Kingdom", "United States of America", "Australia" };

            string city = cities[random.Next(cities.Length)];

            string postalCode = postalCodes[random.Next(postalCodes.Length)];

            string country = countries[random.Next(countries.Length)];

            string updatedBy = DataGenerator.RandomFirstName();


            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberAddress(customerId, addressId, _tokenFixture.GetToken("customer"),
                                    addressLine1, addressLine2, addressLine3, company,
                                    city, startDate, postalCode, "", country, updatedBy).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #region Address Validation After the Update

            queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.LastUpdatedBy = @lastUpdatedBy ";

            paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", fcmDoc.CustomerId).WithParameter("@lastUpdatedBy", updatedBy);

            Address currentAddressAfterUpdate = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery)?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");

            int retry = 5;

            while (retry > 0 && currentAddressAfterUpdate is null)
            {
                await Task.Delay(1500);

                currentAddressAfterUpdate = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery)?.AddressDetails?.FirstOrDefault(c => c.Primary == "Y");

                retry--;

            }

            Assert.True(currentAddressAfterUpdate is not null, $" Failed to retrieve Primary Adddress for the Member {customerId} after the address update");


            Assert.True(currentAddressAfterUpdate.AddressLine1.Equals(addressLine1),
                $"Expected the AddressLine1 of the Parent {customerId} to be {addressLine1} but found {currentAddressAfterUpdate.AddressLine1}.");

            Assert.True(currentAddressAfterUpdate.AddressLine2.Equals(addressLine2),
                $"Expected the AddressLine2 of the Parent {customerId} to be {addressLine2} but found {currentAddressAfterUpdate.AddressLine2}.");

            Assert.True(currentAddressAfterUpdate.AddressLine3.Equals(addressLine3),
                $"Expected the AddressLine3 of the Parent {customerId} to be {addressLine3} but found {currentAddressAfterUpdate.AddressLine3}.");

            Assert.True(currentAddressAfterUpdate.LandlordOrCompanyName.Equals(company),
                $"Expected the CompanyName of the Parent {customerId} to be {company} but found {currentAddressAfterUpdate.LandlordOrCompanyName}.");

            Assert.True(currentAddressAfterUpdate.City.Equals(city),
                $"Expected the City of the Parent {customerId} to be {city} but found {currentAddressAfterUpdate.City}.");

            Assert.True(currentAddressAfterUpdate.PostCode.Equals(postalCode),
                $"Expected the PostCode of the Parent {customerId} to be {postalCode} but found {currentAddressAfterUpdate.PostCode}.");

            Assert.True(currentAddressAfterUpdate.Country.Equals(country),
                $"Expected the Country of the Parent {customerId} to be {country} but found {currentAddressAfterUpdate.Country}.");


            #endregion

            #endregion

            #region API Call

            HttpResponseMessage auditResponse = GraphQL.RetrieveAddressAudit(_tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic auditResult = JsonConvert.DeserializeObject<dynamic>(await auditResponse.Content.ReadAsStringAsync());

            Assert.True(!auditResult.ToString().Contains("Request failed"),
                $"Expected a valid response for retrieve Address audit graphql operation but found {result.ToString()}");

            IEnumerable<AddressHistory> addressHistories = JsonConvert.DeserializeObject<IEnumerable<AddressHistory>>(auditResult?.data?.RetrieveAddressHistory.ToString());

            addressHistories = addressHistories.OrderByDescending(x => x.LastUpdatedOn);

            #endregion

            #region Validations

            #region Validation of current new address in audit response

            if (currentAddressAfterUpdate is not null && currentAddressAfterUpdate.Primary.Equals("Y"))
            {

                Assert.True(currentAddressAfterUpdate.AddressType.Equals(addressHistories.First().AddressType),
                   $"Expected the Current AddressType to be {currentAddressAfterUpdate.AddressType} but found {addressHistories.First().AddressType} in the response");

                Assert.True(currentAddressAfterUpdate.AddressLine1 == addressHistories.First().AddressLine1,
                    $"Expected the Current AddressLine1 to be {currentAddressAfterUpdate.AddressLine1} but found {addressHistories.First().AddressLine1} in the response");

                Assert.True(currentAddressAfterUpdate.AddressLine2 == addressHistories.First().AddressLine2,
                    $"Expected the Current AddressLine2 to be {currentAddressAfterUpdate.AddressLine2} but found {addressHistories.First().AddressLine2} in the response");

                Assert.True(currentAddressAfterUpdate.AddressLine3 == addressHistories.First().AddressLine3,
                    $"Expected the Current AddressLine3 to be {currentAddressAfterUpdate.AddressLine3} but found {addressHistories.First().AddressLine3} in the response");

                Assert.True(currentAddressAfterUpdate.City.Equals(addressHistories.First().City),
                   $"Expected the Current Address City to be {currentAddressAfterUpdate.City} but found {addressHistories.First().City} in the response");

                Assert.True(currentAddressAfterUpdate.Country.Equals(addressHistories.First().Country),
                   $"Expected the Current Address Country to be {currentAddressAfterUpdate.Country} but found {addressHistories.First().Country} in the response");

                Assert.True(currentAddressAfterUpdate.PostCode.Equals(addressHistories.First().PostCode),
                   $"Expected the Current Address PostCode to be {currentAddressAfterUpdate.Country} but found {addressHistories.First().PostCode} in the response");

                addressHistories = addressHistories.Skip(1); // Skip Current Address from the List after validation 

            }

            #endregion

            #region Validation of Old Address in audit response

            Assert.True(currentAddressBeforeUpdate.AddressType == addressHistories.First().AddressType,
                 $" After the address Update in the audit doc, Expected the AddressLine1 to be {currentAddressBeforeUpdate.AddressType} but found {addressHistories.First().AddressType} in the response");

            Assert.True(currentAddressBeforeUpdate.AddressLine1 == addressHistories.First().AddressLine1,
                $"After the address Update in the audit doc,Expected the AddressLine1 to be {currentAddressBeforeUpdate.AddressLine1} but found {addressHistories.First().AddressLine1} in the response");

            Assert.True(currentAddressBeforeUpdate.AddressLine2 == addressHistories.First().AddressLine2,
                $"After the address Update in the audit doc, Expected the AddressLine2 to be {currentAddressBeforeUpdate.AddressLine2} but found {addressHistories.First().AddressLine2} in the response");

            Assert.True(currentAddressBeforeUpdate.AddressLine3 == addressHistories.First().AddressLine3,
                $"After the address Update in the audit doc, Expected the AddressLine3 to be {currentAddressBeforeUpdate.AddressLine3} but found {addressHistories.First().AddressLine3} in the response");

            Assert.True(currentAddressBeforeUpdate.City.Equals(addressHistories.First().City),
               $"After the address Update in the audit doc, Expected the Address City to be {currentAddressBeforeUpdate.City} but found {addressHistories.First().City} in the response");

            Assert.True(currentAddressBeforeUpdate.Country.Equals(addressHistories.First().Country),
               $"After the address Update in the audit doc, Expected the Address Country to be {currentAddressBeforeUpdate.Country} but found {addressHistories.First().Country} in the response");

            Assert.True(currentAddressBeforeUpdate.PostCode.Equals(addressHistories.First().PostCode),
               $"After the address Update in the audit doc, Expected the Address PostCode to be {currentAddressBeforeUpdate.Country} but found {addressHistories.First().PostCode} in the response");

            #endregion


            #endregion

        }

    }
}

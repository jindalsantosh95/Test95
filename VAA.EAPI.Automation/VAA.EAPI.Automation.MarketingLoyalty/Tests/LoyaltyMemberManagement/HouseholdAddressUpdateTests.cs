using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class HouseholdAddressUpdateTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        public static string env = Config.Environment;

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public HouseholdAddressUpdateTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(15)]
        public async Task A_HouseHoldAddressUpdate_SuccessScenario()
        {
            string parentCustomerId = string.Empty;

            #region  PreRequisite - Fetching the Parent and Child Account Details for household Customers

            string queryForFCMDoc = "SELECT Top 5 * FROM  c where c.DocType='FlyingClubMember' and c.MemberType='Household' and c.HeadOfGroupMemberNumber='' and c.GroupId!='' order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> flyingClubDocument = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            Assert.True(flyingClubDocument is not null, $"No Document available for the Searched Query {queryForFCMDoc}.");

            Dictionary<string, string>? childCustomerIds = new();

            foreach (FlyingClubMember memberProfile in flyingClubDocument)
            {
                parentCustomerId = memberProfile.CustomerId;

                string queryForGroupDocument = $"SELECT * FROM  c where c.DocType='Group' and c.HeadOfGroupCustomerId= '{parentCustomerId}' order by c.LastUpdatedOn desc ";

                IEnumerable<GroupHistory> groupHistory = cosmosFixture.QueryItem<Group>(queryForGroupDocument).GroupHistory;

                IEnumerable<GroupHistory> activeChilds = groupHistory.Where(c => !c.CustomerId.Equals(memberProfile.CustomerId) && string.IsNullOrEmpty(c.ExitDate));

                foreach (GroupHistory history in activeChilds)
                {
                    childCustomerIds.Add(history.CustomerId, "");
                }

                if (childCustomerIds.Any())
                {
                    break;
                }
            }

            Assert.True(!string.IsNullOrEmpty(parentCustomerId),
                "The parent CustomerId value returned is Null or Empty");

            Assert.True(childCustomerIds.Any(),
                "Expected one or more child customerIds for validating address update feature but found 0");

            #endregion

            #region Fetching the Customer Document of Parent

            string queryForCustomerDoc = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{parentCustomerId}' order by c.LastUpdatedOn desc";

            IEnumerable<Address> addressDetails = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).AddressDetails;

            string? addressId = addressDetails.Where(c => c.Primary == "Y").Select(c => c.AddressId).FirstOrDefault();

            Assert.True(!string.IsNullOrEmpty(addressId), "The Parent Primary Address Id value is Null or Empty");

            #endregion

            #region Fetching the Customer Document of ChildAccounts

            foreach (KeyValuePair<string, string> keyValuePair in childCustomerIds)
            {
                queryForCustomerDoc = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{keyValuePair.Key}' order by c.LastUpdatedOn desc";

                IEnumerable<Core.DB_Model.Entities.Customer> custDoc = cosmosFixture.QueryItems<Core.DB_Model.Entities.Customer>(queryForCustomerDoc);

                childCustomerIds[keyValuePair.Key] = custDoc.Select(c => c._etag).FirstOrDefault().ToString();
            }

            #endregion

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

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberAddress(parentCustomerId, addressId, tokenFixture.GetToken("customer"), addressLine1, addressLine2, addressLine3, company, city, startDate, postalCode, "", country, updatedBy).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #region Validating new Address of Parent after successful Address Update

            queryForCustomerDoc = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{parentCustomerId}' and c.LastUpdatedBy = '{updatedBy}' order by c.LastUpdatedOn desc";

            Address primaryAddress = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).AddressDetails.FirstOrDefault(c => c.Primary == "Y");

            int retry = 5;

            while (retry > 0 && primaryAddress is null)
            {
                await Task.Delay(1000);

                primaryAddress = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).AddressDetails.FirstOrDefault(c => c.Primary == "Y");

                retry--;
            }


            Assert.True(primaryAddress.AddressLine1.Equals(addressLine1),
                $"Expected the AddressLine1 of the Parent {parentCustomerId} to be {addressLine1} but found {primaryAddress.AddressLine1}.");

            Assert.True(primaryAddress.AddressLine2.Equals(addressLine2),
                $"Expected the AddressLine2 of the Parent {parentCustomerId} to be {addressLine2} but found {primaryAddress.AddressLine2}.");

            Assert.True(primaryAddress.AddressLine3.Equals(addressLine3),
                $"Expected the AddressLine3 of the Parent {parentCustomerId} to be {addressLine3} but found {primaryAddress.AddressLine3}.");

            Assert.True(primaryAddress.LandlordOrCompanyName.Equals(company),
                $"Expected the CompanyName of the Parent {parentCustomerId} to be {company} but found {primaryAddress.LandlordOrCompanyName}.");

            Assert.True(primaryAddress.City.Equals(city),
                $"Expected the City of the Parent {parentCustomerId} to be {city} but found {primaryAddress.City}.");

            Assert.True(primaryAddress.PostCode.Equals(postalCode),
                $"Expected the PostCode of the Parent {parentCustomerId} to be {postalCode} but found {primaryAddress.PostCode}.");

            Assert.True(primaryAddress.Country.Equals(country),
                $"Expected the Country of the Parent {parentCustomerId} to be {country} but found {primaryAddress.Country}.");


            #endregion

            #region Validating the Address of Active Linked Child Accounts after the successful Parent Address Update

            foreach (KeyValuePair<string, string> kvPair in childCustomerIds)
            {
                retry = 10;

                string custId = kvPair.Key;

                queryForCustomerDoc = $"SELECT * FROM  c where c.DocType='Customer' and c.PartitionKey='{custId}' order by c.LastUpdatedOn desc";

                IEnumerable<Core.DB_Model.Entities.Customer> custDoc = cosmosFixture.QueryItems<Core.DB_Model.Entities.Customer>(queryForCustomerDoc);

                string? etag = custDoc.Select(c => c._etag).FirstOrDefault().ToString();

                IEnumerable<Address> primaryAdd = custDoc.Select(c => c.AddressDetails).FirstOrDefault().Where(c => c.Primary == "Y");

                Assert.True(primaryAdd is not null, $"No Primary Address found for the child {custId}");

                while (retry > 0 && kvPair.Value == etag)
                {
                    await Task.Delay(500);

                    custDoc = cosmosFixture.QueryItems<Core.DB_Model.Entities.Customer>(queryForCustomerDoc);

                    primaryAdd = custDoc.Select(c => c.AddressDetails).FirstOrDefault().Where(c => c.Primary == "Y");

                    retry--;
                }
                foreach (Address address in primaryAdd)
                {
                    Assert.True(address.AddressLine1.Equals(addressLine1),
                        $"Expected the AddressLine1 for the child {custId} to be {addressLine1} but found {address.AddressLine1}.");

                    Assert.True(address.AddressLine2.Equals(addressLine2),
                        $"Expected the AddressLine2 for the child {custId} to be {addressLine2} but found {address.AddressLine2}.");

                    Assert.True(address.AddressLine3.Equals(addressLine3),
                        $"Expected the AddressLine3 for the child {custId} to be {addressLine3} but found {address.AddressLine3}.");

                    Assert.True(address.LandlordOrCompanyName.Equals(company),
                        $"Expected the CompanyName for the child {custId} to be {company} but found {address.LandlordOrCompanyName}.");

                    Assert.True(address.City.Equals(city),
                        $"Expected the City for the child {custId} to be {city} but found {address.City}.");

                    Assert.True(address.PostCode.Equals(postalCode),
                        $"Expected the PostCode for the child {custId} to be {postalCode} but found {address.PostCode}.");

                    Assert.True(address.Country.Equals(country),
                        $"Expected the Country for the child {custId} to be {country} but found {address.Country}.");
                }

            }

            #endregion


        }
    }
}

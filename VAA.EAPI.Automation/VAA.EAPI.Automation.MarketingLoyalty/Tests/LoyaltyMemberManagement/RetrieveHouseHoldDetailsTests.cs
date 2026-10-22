using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{

    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetrieveHouseHoldDetailsTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public RetrieveHouseHoldDetailsTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
            this.cosmosFixture = cosmosFixture;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(16)]
        public async Task A_HouseHoldDetailsRetrieval_SuccessScenario()
        {

            string headAccountCustomerId = string.Empty;
            string headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string subOrdinateAccountMemberNumber = string.Empty;

            #region  PreRequisite - Fetching the Test Data from Cosmos DB

            string queryForFCMDoc = "SELECT TOP 1 * FROM  c where c.DocType ='FlyingClubMember'  and c.Status='Active' and  c.MemberType='Individual' and c.HeadOfGroupMemberNumber !='' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

            int retry = 5;

            while (retry > 0 && flyingClubDocument == null)
            {
                await Task.Delay(500);

                flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

                retry--;
            }

            Assert.True(flyingClubDocument is not null, $"Failed to Retrieve the Test Data .No Document available for the Searched Query {queryForFCMDoc}.");

            headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            subOrdinateAccountCustomerId = flyingClubDocument.CustomerId;

            subOrdinateAccountMemberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = $"SELECT TOP 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber='{headAccountMemberNumber}'";

            CustomerSearch customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

            retry = 5;

            while (retry > 0 && customer == null)
            {
                await Task.Delay(500);

                customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

                retry--;
            }

            Assert.True(customer is not null, $"No Document available for the Searched Query {queryForCustomerSearchDoc}.");

            headAccountCustomerId = customer.CustomerId;


            #endregion


            #region API Call and Validations for Head Account HouseHold Details Retrieval

            HttpResponseMessage response = GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), headAccountCustomerId, "").EnsureIsSuccessStatusCode();

            string retrieveHouseHoldDetailsResult = await response.Content.ReadAsStringAsync();

            HouseHoldDetailsRetrieval houseHoldDetailsRetrieval = JsonConvert.DeserializeObject<HouseHoldDetailsRetrieval>(retrieveHouseHoldDetailsResult);

            Assert.True(houseHoldDetailsRetrieval is not null,
                $"Failed to retrieve the member Household Details for customer {headAccountCustomerId}, the response was {retrieveHouseHoldDetailsResult} ");

            IEnumerable<HouseholdHistory> actualInactiveHistory = houseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.householdHistory;

            IEnumerable<HouseholdMember> actualActiveHistory = houseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.householdMembers;

            bool isChildAccountPresent = actualActiveHistory.Any(x => x.customerId == subOrdinateAccountCustomerId);

            Assert.True(isChildAccountPresent,
                $"In the HeadAccount Household details Response , Expected the Active Child Account {subOrdinateAccountCustomerId} in the HouseHold Member section but not found.");


            foreach (HouseholdMember activeHouseholdMember in actualActiveHistory)
            {

                Assert.True(!string.IsNullOrEmpty(activeHouseholdMember.lastUpdatedBy),
                    "In the HeadAccount Household details Response , " +
                    $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the LastUpdatedBy to have a valid value but found null");

                Assert.True(activeHouseholdMember.lastUpdatedOn != DateTime.MinValue,
                    "In the HeadAccount Household details Response , " +
                    $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the LastUpdatedOn to have a valid value but found MinValue/Null");

                Assert.True(!string.IsNullOrEmpty(activeHouseholdMember.createdBy),
                   "In the HeadAccount Household details Response , " +
                   $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the CreatedBy to have a valid value but found null");

                Assert.True(activeHouseholdMember.createdOn != DateTime.MinValue,
                 "In the HeadAccount Household details Response , " +
                 $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the CreatedOn to have a valid value but found MinValue/Null");


            }



            foreach (HouseholdHistory inactiveHouseholdHistory in actualInactiveHistory)
            {

                Assert.True(!string.IsNullOrEmpty(inactiveHouseholdHistory.lastUpdatedBy),
                    "In the HeadAccount Household details Response , " +
                    $"Under Household history Section , For the Child Customer {inactiveHouseholdHistory.customerId} Expected the LastUpdatedBy to have a valid value but found null");

                Assert.True(inactiveHouseholdHistory.endDate != DateTime.MinValue,
                    "In the HeadAccount Household details Response , " +
                    $"Under Household history Section ,For the Child Customer {inactiveHouseholdHistory.customerId} Expected the EndDate to have a valid value but found MinValue/Null");

                Assert.True(inactiveHouseholdHistory.lastUpdatedOn != DateTime.MinValue,
                   "In the HeadAccount Household details Response , " +
                   $"Under Household history Section ,For the Child Customer {inactiveHouseholdHistory.customerId} Expected the LastUpdatedOn to have a valid value but found MinValue/Null");


            }

            #endregion


            #region API Call and Validations for Child Account HouseHold Details Retrieval

            response = GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, "").EnsureIsSuccessStatusCode();

            string retrievechildHouseHoldDetailsResult = await response.Content.ReadAsStringAsync();

            HouseHoldDetailsRetrieval childHouseHoldDetailsRetrieval = JsonConvert.DeserializeObject<HouseHoldDetailsRetrieval>(retrievechildHouseHoldDetailsResult);

            Assert.True(childHouseHoldDetailsRetrieval is not null,
                $"Failed to retrieve the member Household Details for child customer {subOrdinateAccountCustomerId}, the response was {retrieveHouseHoldDetailsResult} ");

            IEnumerable<HouseholdHistory> childActualInactiveHistory = childHouseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.householdHistory;

            IEnumerable<HouseholdMember> childActualActiveHistory = childHouseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.householdMembers;

            HeadOfHouseHold headOfHouseHoldDetails = childHouseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.headOfHousehold;

            Assert.True(headOfHouseHoldDetails is not null, $"HeadOfHousehold details not found in the child account {subOrdinateAccountCustomerId} HH Response ");

            bool isParentAccountPresent = childHouseHoldDetailsRetrieval.data?.loyaltyMemberHousehold?.headOfHousehold?.customerId == headAccountCustomerId;

            Assert.True(isParentAccountPresent,
                $"In the ChildAccount Household details Response , Expected the HeadofHousehold CustomerId  {headAccountCustomerId} in the HeadOfHouseHold  section but not found.");


            Assert.True(!string.IsNullOrEmpty(headOfHouseHoldDetails.createdBy),
                   "In the ChildAccount Household details Response , " +
                   $"Under HeadOfHousehold Section , For the Head Customer {headOfHouseHoldDetails.customerId} Expected the CreatedBy to have a valid value but found null");

            Assert.True(headOfHouseHoldDetails.createdOn != DateTime.MinValue,
                "In the ChildAccount Household details Response , " +
                $"Under HeadOfHousehold Section ,For the Head Customer {headOfHouseHoldDetails.customerId} Expected the CreatedOn to have a valid value but found MinValue/Null");

            Assert.True(headOfHouseHoldDetails.customerId == headAccountCustomerId,
               "In the ChildAccount Household details Response , " +
               $"Under HeadOfHousehold Section , Expected the HeadAccountCustomerId to be {headAccountCustomerId} but found {headOfHouseHoldDetails.customerId}");


            foreach (HouseholdMember activeHouseholdMember in childActualActiveHistory)
            {

                Assert.True(!string.IsNullOrEmpty(activeHouseholdMember.lastUpdatedBy),
                    "In the ChildAccount Household details Response , " +
                    $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the LastUpdatedBy to have a valid value but found null");

                Assert.True(activeHouseholdMember.lastUpdatedOn != DateTime.MinValue,
                    "In the ChildAccount Household details Response , " +
                    $"Under Household Member Section ,For the Child Customer {activeHouseholdMember.customerId} Expected the LastUpdatedOn to have a valid value but found MinValue/Null");

            }


            foreach (HouseholdHistory inactiveHouseholdHistory in childActualInactiveHistory)
            {

                Assert.True(!string.IsNullOrEmpty(inactiveHouseholdHistory.lastUpdatedBy),
                    "In the ChildAccount Household details Response , " +
                    $"Under Household history Section , For the Child Customer {inactiveHouseholdHistory.customerId} Expected the LastUpdatedBy to have a valid value but found null");

                Assert.True(inactiveHouseholdHistory.endDate != DateTime.MinValue,
                    "In the ChildAccount Household details Response , " +
                    $"Under Household history Section ,For the Child Customer {inactiveHouseholdHistory.customerId} Expected the EndDate to have a valid value but found MinValue/Null");

                Assert.True(inactiveHouseholdHistory.lastUpdatedOn != DateTime.MinValue,
                   "In the ChildAccount Household details Response , " +
                   $"Under Household history Section ,For the Child Customer {inactiveHouseholdHistory.customerId} Expected the LastUpdatedOn to have a valid value but found MinValue/Null");


            }

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(17)]
        public async Task B_RetrieveHouseHoldDetails_MandatoryFieldMissingError_WhenBothCustomerIdAndMemberNumberIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), "", "");

            dynamic retrieveHouseHoldDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'customerId' or memberId must be spefied.)";

            Assert.True(retrieveHouseHoldDetailsResult.ToString().Contains(expectedResponse),
              $"The Retrieve HouseHold Acc Details response is not as expected. Expected {expectedResponse} " +
              $" but was {retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.ToString()}");
        }

    }
}

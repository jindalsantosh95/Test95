using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class HouseHoldDelinkingTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        string parentCustomerId = "", parentMemberNumber = "", childCustomerId = "", childMemberNumber = "";

        public HouseHoldDelinkingTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            (string parentCustomerId, string parentMemberNumber, string childCustomerId, string childMemberNumber) houseHoldDetails = GetValidHouseholdMemberDetails();

            parentMemberNumber = houseHoldDetails.parentMemberNumber;
            parentCustomerId = houseHoldDetails.parentCustomerId;
            childMemberNumber = houseHoldDetails.childMemberNumber;
            childCustomerId = houseHoldDetails.childCustomerId;
        }



        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(9)]
        public async Task A_HouseHoldDelinking_MandatoryFieldMissingError_WhenHeadAccountDetailsAreEmptyOrNull()
        {

            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, "", "", "TestScript");

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(householdDelinkingResult?.errors[0]?.message?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdDelinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(10)]
        public async Task B_HouseHoldDelinking_MandatoryFieldMissingError_WhenChildAccountDetailsAreEmptyOrNull()
        {

            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), "", "", parentCustomerId, parentMemberNumber, "TestScript");

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(householdDelinkingResult?.errors[0]?.message?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdDelinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(11)]
        public async Task C_HouseHoldDelinking_MandatoryFieldMissingError_WhenInititedByFieldValueIsNullOrEmpty()
        {
            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, "");

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "InitiatedBy'must be specified.";

            Assert.True(householdDelinkingResult?.errors[0]?.message?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdDelinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(12)]
        public async Task D_HouseHoldDelinking_InvalidFieldValueError_WhenHeadAccountCustomerIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, "a76ce121-a95a-456b-9e37-c326dc82d", parentMemberNumber, "TestScript");

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "The customer ID provided is incorrect. Kindly re-try with correct customer ID.";

            Assert.True(householdDelinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdDelinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(13)]
        public async Task E_HouseHoldDelinking_InvalidFieldValueError_WhenChildAccountCustomerIdIsInvalid()
        {
            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), "a76ce121-a95a-456b-9e37-c326dc82d", childMemberNumber, parentCustomerId, parentMemberNumber, "TestScript");

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "The customer ID provided is incorrect. Kindly re-try with correct customer ID.";

            Assert.True(householdDelinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdDelinkingResult.errors[0].extensions.response.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(14)]
        public async Task F_HouseHoldDelinking_SuccessScenario()
        {

            string headAccountCustomerId = string.Empty;
            string headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string subOrdinateAccountMemberNumber = string.Empty;

            #region  PreRequisite - Fetching the Test Data from Cosmos DB

            string queryForFCMDoc = "SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember'  and c.Status='Active' and  c.MemberType='Individual' and c.HeadOfGroupMemberNumber!='' and c.CreatedBy not in ('WEB-MYA' , 'APP-MYA') order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

            int retry = 5;

            while (retry > 0 && flyingClubDocument == null)
            {
                await Task.Delay(500);

                flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

                retry--;
            }

            if (flyingClubDocument is null)
            {
                throw new Xunit.Sdk.XunitException($"Failed to Retrieve the Test Data .No Document available for the Searched Query {queryForFCMDoc}.");
            }

            headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            subOrdinateAccountCustomerId = flyingClubDocument.CustomerId;

            subOrdinateAccountMemberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = $"SELECT Top 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber='{headAccountMemberNumber}'";

            CustomerSearch customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

            retry = 5;

            while (retry > 0 && customer == null)
            {
                await Task.Delay(500);

                customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

                retry--;
            }

            if (customer is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document available for the Searched Query {queryForCustomerSearchDoc}.");
            }

            headAccountCustomerId = customer.CustomerId;

            #endregion

            # region capturing the timestamp before the Group Document is updated             

            string queryForGroupDocumentBeforeDelink = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'Group' and c.HeadOfGroupCustomerId='{headAccountCustomerId}' ";

            Group groupBeforeDelink = cosmosFixture.QueryItem<Group>(queryForGroupDocumentBeforeDelink);

            DateTime? lastUpdatedTimeBeforeDelink = Convert.ToDateTime(groupBeforeDelink.LastUpdatedOn);

            string initiatedBy = DataGenerator.RandomFirstName();

            #endregion


            #region Delinking

            HttpResponseMessage response = GraphQL.HouseHoldDelinking(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, subOrdinateAccountMemberNumber, headAccountCustomerId, headAccountMemberNumber, initiatedBy);

            dynamic householdDelinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Request successfully initiated";

            Assert.True(householdDelinkingResult.data.loyaltyHouseholdDelinking.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response text but found {householdDelinkingResult.data.loyaltyHouseholdDelinking.ToString()} ");

            //Group Document Validations After HouseHoldDelinking           

            string queryForGroupDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'Group' and c.HeadOfGroupCustomerId='{headAccountCustomerId}' and c.LastUpdatedBy = '{initiatedBy}' order by c._ts desc";

            Group group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

            DateTime lastUpdatedOnAfterDelink = !string.IsNullOrEmpty(group?.LastUpdatedOn) ? Convert.ToDateTime(group?.LastUpdatedOn) : DateTime.MinValue;

            retry = 5;

            while (retry > 0 && group == null && lastUpdatedOnAfterDelink < lastUpdatedTimeBeforeDelink)
            {
                await Task.Delay(1000);

                group = cosmosFixture.QueryItem<Group>(queryForGroupDocument);

                lastUpdatedOnAfterDelink = !string.IsNullOrEmpty(group?.LastUpdatedOn) ? Convert.ToDateTime(group?.LastUpdatedOn) : DateTime.MinValue;

                retry--;
            }

            Assert.True(group.HeadOfGroupCustomerId.Equals(headAccountCustomerId));

            Assert.True(group.Status.Equals("Active"),
                $"Expected Group Documents Status as 'Active' but found {group.Status}");

            Assert.True(group.LastUpdatedBy.Equals(initiatedBy),
                $"Expected LastUpdatedBy as {initiatedBy} but found {group.LastUpdatedBy}");

            string groupDocId = group.GroupId;

            IOrderedEnumerable<GroupHistory> groupHistory = group.GroupHistory.OrderByDescending(x => x.LastUpdatedOn);

            int c = groupHistory.Count(x => x.CustomerId.Equals(subOrdinateAccountCustomerId) && string.IsNullOrWhiteSpace(x.ExitDate));

            Assert.True(c == 0, $"Expected no active household member details but found {c.ToString()}");

            string expectedDeLinkedDate = DateTime.UtcNow.Date.ToString("dd-MMM-yyyy");

            GroupHistory deLinkedChildDetails = groupHistory.FirstOrDefault(x => x.CustomerId.Equals(subOrdinateAccountCustomerId)
            && Convert.ToDateTime(x.ExitDate).ToString("dd-MMM-yyyy").Equals(expectedDeLinkedDate));

            Assert.True(deLinkedChildDetails.LastUpdatedBy.Equals(initiatedBy),
                $"Expected the LastUpdatedBy in the Child Group History to be {initiatedBy} but found {deLinkedChildDetails.LastUpdatedBy}");

            //FlyingClubMember Document Validations After HouseHoldDelinking

            //Parent FCM Document Validations

            string queryForFCMDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey='{headAccountCustomerId}' and c.LastUpdatedBy = '{initiatedBy}' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(1500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null, $"Failed to Retreive the Parent FCM Doc after Delinking for the Query {queryForFCMDocument} ");

            Assert.True(flyingClubMember.MemberType.Equals("Individual"), $"Expected Parent/Hod MemberType as Individual but found {flyingClubMember.MemberType}");

            Assert.True(flyingClubMember.GroupId.Equals(groupDocId), $"Expected the GroupId in Head FCM Doc as {groupDocId}  but found {flyingClubMember.GroupId}");

            Assert.True(string.IsNullOrEmpty(flyingClubMember.HeadOfGroupMemberNumber), $"Expected HeadOfHouseholdMember as null or empty in Head FCM Doc but found {flyingClubMember.HeadOfGroupMemberNumber}");

            //Child FCM Document Validations
            queryForFCMDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey='{subOrdinateAccountCustomerId}' and c.LastUpdatedBy = '{initiatedBy}' order by c._ts desc";

            flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(1500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null, $"Failed to Retrieve the Child FCM Doc after Delinking for the Query {queryForFCMDocument} ");

            Assert.True(flyingClubMember.MemberType.Equals("Individual"), $"Expected Child/SubOrdinate MemberType as Individual but found {flyingClubMember.MemberType}");

            Assert.True(string.IsNullOrEmpty(flyingClubMember.GroupId), $"Expected the GroupId in Child FCM Doc as null or empty  but found {flyingClubMember.GroupId}");

            Assert.True(string.IsNullOrEmpty(flyingClubMember.HeadOfGroupMemberNumber),
                $"Expected HeadOfHouseholdMember as null or empty in child FCM Doc but found {flyingClubMember.HeadOfGroupMemberNumber}");

            #endregion

            #region HeadAccountRetrievalAfterDelinking


            response = GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), headAccountCustomerId, "");

            dynamic retrieveHouseHoldDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdMembers.ToString().Contains(subOrdinateAccountCustomerId),
                $"Expected no child Account details in householdMembers List but found {retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdMembers.ToString()}");

            Assert.True(retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdHistory.ToString().Contains(subOrdinateAccountCustomerId),
                $"Expected {subOrdinateAccountCustomerId} and child Acc Details in the household history but found {retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdHistory.ToString()}");
            #endregion

            #region ChildAccountRetrievalAfterDelinking

            response = GraphQL.RetrieveHouseHoldDetails(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, "");

            retrieveHouseHoldDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.headOfHousehold.ToString()),
                $"Expected null or no Head Account Details But Found {retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.headOfHousehold.ToString()}");

            Assert.True(retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdHistory.ToString().Contains(headAccountCustomerId),
                $"Expected {headAccountCustomerId} and Head Acc Details in the household history but found {retrieveHouseHoldDetailsResult.data.loyaltyMemberHousehold.householdHistory.ToString()}");

            #endregion
        }

        public (string parentCustomerId, string parentMemberNumber, string childCustomerId, string childMemberNumber) GetValidHouseholdMemberDetails()
        {

            string headAccountCustomerId = string.Empty;
            string headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string subOrdinateAccountMemberNumber = string.Empty;

            #region  PreRequisite - Fetching the Test Data from Cosmos DB

            string queryForFCMDoc = "SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember'  and c.Status='Active' and  c.MemberType='Individual' and c.HeadOfGroupMemberNumber!='' and c.CreatedBy not in ('APP-MYA','WEB-MYA') order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

            int retry = 5;

            while (retry > 0 && flyingClubDocument == null)
            {
                Thread.Sleep(500);

                flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

                retry--;
            }

            Assert.True(flyingClubDocument is not null, $"Failed to Retrieve the Test Data .No Document available for the Searched Query {queryForFCMDoc}.");

            headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            subOrdinateAccountCustomerId = flyingClubDocument.CustomerId;

            subOrdinateAccountMemberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = $"SELECT Top 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber='{headAccountMemberNumber}'";

            CustomerSearch customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

            retry = 5;

            while (retry > 0 && customer == null)
            {
                Thread.Sleep(500);

                customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

                retry--;
            }

            Assert.True(customer is not null, $"No Document available for the Searched Query {queryForCustomerSearchDoc}.");

            headAccountCustomerId = customer.CustomerId;

            #endregion

            return (headAccountCustomerId, headAccountMemberNumber, subOrdinateAccountCustomerId, subOrdinateAccountMemberNumber);
        }
    }
}

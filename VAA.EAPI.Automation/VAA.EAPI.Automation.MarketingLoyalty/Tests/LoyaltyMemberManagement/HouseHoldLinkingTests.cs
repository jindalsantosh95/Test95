using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class HouseHoldLinkingTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        public static string env = Config.Environment;


        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        string parentCustomerId = "", parentMemberNumber = "", childCustomerId = "", childMemberNumber = "";

        public HouseHoldLinkingTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            preRequisiteTestData = new();

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Group>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            (string parentCustomerId, string parentMemberNumber, string childCustomerId, string childMemberNumber) validParentAndChildDetails = GetValidHeadAndChildMemberDetails();

            parentMemberNumber = validParentAndChildDetails.parentMemberNumber;
            parentCustomerId = validParentAndChildDetails.parentCustomerId;
            childMemberNumber = validParentAndChildDetails.childMemberNumber;
            childCustomerId = validParentAndChildDetails.childCustomerId;
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task A_HouseHoldLinking_SuccessScenario()
        {

            string parentCustomerId = await preRequisiteTestData.LinkingAChildToAParentAccount();

            Assert.True(!string.IsNullOrEmpty(parentCustomerId), "After linking Expected a valid Head/Parent FC customerId but found null or empty");

        }

        [Trait("Category", "Local-Run")]
        [Fact(Skip = "Not Applicable when running in both the Environments due to Test Data")]
        public async Task B_HouseHoldLinking_InvalidEmailFormatError_WhenEmailFormatIsInvalidForHeadOfHouseHoldAccount()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                                , "f7ff4d2b-6e69-427c-b021-f79b98a7f560", "2101085443", "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "A valid Head Of Account EmailId is required";

            Assert.True(householdLinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        [Trait("Category", "Local-Run")]
        [Fact(Skip = "Not Applicable when running in both the Environments due to Test Data")]
        public async Task C_HouseHoldLinking_InvalidEmailFormatError_WhenEmailFormatIsInvalidForChildAccount()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), "b33df5f1-9056-46d3-8b81-dc9f289793e7", "1078762349"
                                                , parentCustomerId, parentMemberNumber, "TestScript");


            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "A valid Child Account EmailId is required";

            Assert.True(householdLinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task D_HouseHoldLinking_InvalidMemberStatusError_WhenStatusOfHeadAccountIsNotActive()
        {
            #region Retrieving Head Account with status Not Active

            string queryForFCMDoc = "SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember'  and c.Status !='Active' and c.TierCode in ('Gold','Silver') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.CreatedBy not in ('VRED','AXIS','SNAPP','WEB-MYA') order by c.CreatedOn desc";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            string headAccountCustomerId = "";

            string headAccountMemberNumber = flyingClubMemberDoc?.FirstOrDefault(x =>
             {
                 headAccountCustomerId = x.CustomerId;

                 string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{headAccountCustomerId}'";

                 IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).ContactDetails;

                 return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

             })?.MemberNumber ?? "";

            Assert.True(!string.IsNullOrEmpty(headAccountMemberNumber),
                "Didn't find FC Member whose Status is not Active from the retrieved FCM Documents");

            #endregion

            #region Retrieving Valid Child Account 

            queryForFCMDoc = $"SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember' and c.Status='Active' and c.TierCode not in ('Glass','VAC') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.PartitionKey != '{headAccountCustomerId}' and c.CreatedBy != 'WEB-MYA' order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> fcmDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            string childStatus = string.Empty;

            string childTierCode = string.Empty;

            string subOrdinateAccountCustomerId = "";

            string subOrdinateMemberNumber = fcmDoc?.FirstOrDefault(x =>
            {
                childTierCode = x.TierCode;

                subOrdinateAccountCustomerId = x.CustomerId;

                childStatus = x.Status;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{subOrdinateAccountCustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber ?? "";

            Assert.True(!string.IsNullOrEmpty(subOrdinateMemberNumber),
                "Didn't find Child FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            #endregion

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), subOrdinateAccountCustomerId, subOrdinateMemberNumber, headAccountCustomerId, headAccountMemberNumber, "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Head of the account status is not active";

            string actualResponse = householdLinkingResult?.errors[0]?.extensions?.response?.ToString() ?? "";

            Assert.True(actualResponse.Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task E_HouseHoldLinking_MandatoryFieldMissingError_WhenHeadAccountDetailsAreEmptyOrNull()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, "", "", "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(householdLinkingResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.errors[0].ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task F_HouseHoldLinking_MandatoryFieldMissingError_WhenChildAccountDetailsAreEmptyOrNull()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), "", "", parentCustomerId, parentMemberNumber, "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(householdLinkingResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task G_HouseHoldLinking_MandatoryFieldMissingError_WhenInititedByFieldValueIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, "");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "InitiatedBy'must be specified.";

            Assert.True(householdLinkingResult?.errors[0]?.message?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task H_HouseHoldLinking_InvalidFieldValueError_WhenHeadAccountCustomerIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber, "a76ce121-a95a-456b-9e37-c326dc82d", parentMemberNumber, "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "The customer ID provided is incorrect. Kindly re-try with correct customer ID.";

            Assert.True(householdLinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task I_HouseHoldLinking_InvalidFieldValueError_WhenChildAccountCustomerIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.HouseHoldLinking(tokenFixture.GetToken("graphqlv2"), "a76ce121-a95a-456b-9e37-c326dc82d", childMemberNumber, parentCustomerId, parentMemberNumber, "TestScript");

            dynamic householdLinkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "The customer ID provided is incorrect. Kindly re-try with correct customer ID.";

            Assert.True(householdLinkingResult?.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {householdLinkingResult.ToString()}");
        }

        public (string parentCustomerId, string parentMemberNumber, string childCustomerId, string childMemberNumber) GetValidHeadAndChildMemberDetails()
        {

            string headAccountCustomerId = string.Empty;
            string? headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string? subOrdinateAccountMemberNumber = string.Empty;

            #region Fetching a Valid Gold FC Member

            string queryForFCMDoc = "SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember'  and c.Status='Active' and c.TierCode in ('Gold','Silver') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.CreatedBy not in ('VRED','AXIS','SNAPP','WEB-MYA') order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> flyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            headAccountMemberNumber = flyingClubMemberDoc?.FirstOrDefault(x =>
            {
                headAccountCustomerId = x.CustomerId;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{headAccountCustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(headAccountMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");


            #endregion

            #region Fetching a Valid Child FC Member

            queryForFCMDoc = $"SELECT Top 10 * FROM  c where c.DocType='FlyingClubMember' and c.Status='Active' and c.TierCode not in ('Glass','VAC') and c.MemberType='Individual' and c.HeadOfGroupMemberNumber='' and c.PartitionKey != '{headAccountCustomerId}' and c.CreatedBy != 'WEB-MYA' order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> fcmDocs = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            string childStatus = string.Empty;

            string childTierCode = string.Empty;

            subOrdinateAccountMemberNumber = fcmDocs?.FirstOrDefault(x =>
            {
                childTierCode = x.TierCode;

                subOrdinateAccountCustomerId = x.CustomerId;

                childStatus = x.Status;

                string queryForCustomerDoc = $"SELECT * FROM CustomerCollection c where c.DocType='Customer' and c.PartitionKey='{subOrdinateAccountCustomerId}'";

                IEnumerable<ContactDetails> contactDetails = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(queryForCustomerDoc).ContactDetails;

                return contactDetails.Any(c => c.ContactType.Equals("Email") && c.Valid == "Y" && c.Primary == "Y");

            })?.MemberNumber;

            Assert.True(!string.IsNullOrEmpty(subOrdinateAccountMemberNumber),
                "Didn't find FC Member whose Email Valid flag is Y from the retrieved FCM Documents");

            Assert.True(!string.IsNullOrEmpty(childStatus),
               "Expected childStatus to have a value but found null or empty value");

            Assert.True(!string.IsNullOrEmpty(childTierCode),
               "Expected child TierCode to have a value but found null or empty value");

            #endregion

            return (headAccountCustomerId, headAccountMemberNumber, subOrdinateAccountCustomerId, subOrdinateAccountMemberNumber);
        }
    }
}

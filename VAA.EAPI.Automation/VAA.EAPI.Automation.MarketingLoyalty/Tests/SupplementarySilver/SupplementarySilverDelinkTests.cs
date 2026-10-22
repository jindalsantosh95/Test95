using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.SupplementarySilver
{
    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class SupplementarySilverDelinkTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public SupplementarySilverDelinkTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(31)]
        public async Task SupplementarySilverDelinkAccount_Success()
        {
            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }


            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");


            string childCustomerId = flyingClubMember.CustomerId;
            string childMemberNumber = flyingClubMember.MemberNumber;
            string sSAMemberNumber = flyingClubMember.SSAMemberNumber;
            string childDocEtag = flyingClubMember._etag;

            string queryForSSAFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber = @ssaMemberNumber";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForSSAFCMDocument).WithParameter("@ssaMemberNumber", sSAMemberNumber);

            FlyingClubMember ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retryAgain = 5;

            while (retryAgain > 0 && ssaFlyingClubMember == null)
            {
                await Task.Delay(500);

                ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retryAgain--;
            }


            Assert.True(ssaFlyingClubMember is not null,
                $"No Document available for the Searched Query {queryForSSAFCMDocument}.");


            string sSACustomerId = ssaFlyingClubMember.CustomerId;
            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , sSACustomerId, sSAMemberNumber, "TestScript");

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Request successfully initiated";

            Assert.True(delinkResult.data?.loyaltySupplementarySilverDelinking?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response text but found {delinkResult.ToString()} ");

            #endregion


            #region FlyingClubMember Document Validations After SSA delinking 


            string queryForFCMDocumentAfterDelinking = "SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'FlyingClubMember' and c.PartitionKey = @childCustomerId order by c._ts desc";

            parameterizedQuery = new QueryDefinition(query: queryForFCMDocumentAfterDelinking).WithParameter("@childCustomerId", childCustomerId);

            FlyingClubMember childFlyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retryAgain2 = 5;

            while ((retryAgain2 > 0 && childFlyingClubMemberDocument == null) || childDocEtag == childFlyingClubMemberDocument._etag)
            {
                await Task.Delay(500);

                childFlyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retryAgain2--;
            }

            Assert.True(childFlyingClubMemberDocument is not null,
                $"No Document available for the Searched Query {queryForFCMDocumentAfterDelinking}.");

            Assert.True(string.IsNullOrEmpty(childFlyingClubMemberDocument.SSAMemberNumber),
                $"Expected ssaMemberNumber should be BLANK but found {sSAMemberNumber}");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(32)]
        public async Task SupplementarySilverDelinkAccount_MandatoryFieldMissingError_WhenHeadAccountDetailsAreEmptyOrNull()
        {

            #region Pre-Requisite for Fetching the Child member details 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");

            string childCustomerId = flyingClubMember.CustomerId;
            string childMemberNumber = flyingClubMember.MemberNumber;

            string headCustomerId = string.Empty;
            string headMemberNumber = string.Empty;


            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , headCustomerId, headMemberNumber, "TestScript");

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(delinkResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {delinkResult.ToString()}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(33)]
        public async Task SupplementarySilverDelinkAccount_MandatoryFieldMissingError_WhenChildAccountDetailsAreEmptyOrNull()
        {

            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");

            string childCustomerId = string.Empty;
            string childMemberNumber = string.Empty;
            string sSAMemberNumber = flyingClubMember.SSAMemberNumber;

            string queryForSSAFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber = @ssaMemberNumber";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForSSAFCMDocument).WithParameter("@ssaMemberNumber", sSAMemberNumber);

            FlyingClubMember ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retryAgain = 5;

            while (retryAgain > 0 && ssaFlyingClubMember == null)
            {
                await Task.Delay(500);

                ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retryAgain--;
            }

            Assert.True(ssaFlyingClubMember is not null,
                $"No Document available for the Searched Query {queryForSSAFCMDocument}.");

            string sSACustomerId = ssaFlyingClubMember.CustomerId;

            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , sSACustomerId, sSAMemberNumber, "TestScript");

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)";

            Assert.True(delinkResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {delinkResult.ToString()}");

            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(34)]
        public async Task SupplementarySilverDelinkAccount_MandatoryFieldMissingError_WhenInititedByFieldValueIsNullOrEmpty()
        {

            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");

            string childCustomerId = flyingClubMember.CustomerId;
            string childMemberNumber = flyingClubMember.MemberNumber;
            string sSAMemberNumber = flyingClubMember.SSAMemberNumber;

            string queryForSSAFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber = @ssaMemberNumber";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForSSAFCMDocument).WithParameter("@ssaMemberNumber", sSAMemberNumber);

            FlyingClubMember ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retryAgain = 5;

            while (retryAgain > 0 && ssaFlyingClubMember == null)
            {
                await Task.Delay(500);

                ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retryAgain--;
            }

            Assert.True(ssaFlyingClubMember is not null,
                $"No Document available for the Searched Query {queryForSSAFCMDocument}.");

            string sSACustomerId = ssaFlyingClubMember.CustomerId;
            string initiatedBy = string.Empty;

            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , sSACustomerId, sSAMemberNumber, initiatedBy);

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "'InitiatedBy'must be specified.";

            Assert.True(delinkResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {delinkResult.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(35)]
        public async Task SupplementarySilverDelinkAccount_InvalidFieldValueError_WhenChildAccountCustomerIdIsInvalid()
        {

            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");

            string childCustomerId = flyingClubMember.CustomerId + "invalid";
            string childMemberNumber = flyingClubMember.MemberNumber;
            string sSAMemberNumber = flyingClubMember.SSAMemberNumber;


            string queryForSSAFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber = @ssaMemberNumber";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForSSAFCMDocument).WithParameter("@ssaMemberNumber", sSAMemberNumber);

            FlyingClubMember ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retryAgain = 5;

            while (retryAgain > 0 && ssaFlyingClubMember == null)
            {
                await Task.Delay(500);

                ssaFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retryAgain--;
            }

            Assert.True(ssaFlyingClubMember is not null,
                $"No Document available for the Searched Query {queryForSSAFCMDocument}.");

            string sSACustomerId = ssaFlyingClubMember.CustomerId;

            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , sSACustomerId, sSAMemberNumber, "TestScript");

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid customer ID.";

            Assert.True(delinkResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {delinkResult.ToString()}");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(36)]
        public async Task SupplementarySilverDelinkAccount_InvalidFieldValueError_WhenHeadAccountCustomerIdIsInvalid()
        {

            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            FlyingClubMember flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(500);

                flyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDocument);

                retry--;
            }

            Assert.True(flyingClubMember is not null,
                $"No Document available for the Searched Query {queryForFCMDocument}.");

            string childCustomerId = flyingClubMember.CustomerId;
            string childMemberNumber = flyingClubMember.MemberNumber;
            string sSAMemberNumber = flyingClubMember.SSAMemberNumber;
            string sSACustomerId = "cf032daf-199b-48bd-9cf4-9410183fd-7ce12"; //invalid Cust Id

            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , sSACustomerId, sSAMemberNumber, "TestScript");

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid customer ID.";

            Assert.True(delinkResult.errors[0].extensions.response.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response but found {delinkResult.ToString()}");

            #endregion
        }
    }
}




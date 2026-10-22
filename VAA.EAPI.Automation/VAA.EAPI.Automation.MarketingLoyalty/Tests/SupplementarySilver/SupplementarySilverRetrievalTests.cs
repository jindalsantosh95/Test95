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
    public class SupplementarySilverRetrievalTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public SupplementarySilverRetrievalTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
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
        [Fact, RunOrder(37)]
        public async Task SupplementarySilverAccountSearchByPassingChildAccountDetails_Success()
        {
            #region Pre-Requisite for Fetching the SSA CustomerId from the FC document 

            string queryForChildFCMDocument = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            List<FlyingClubMember> childFlyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFCMDocument).ToList();

            string childCustomerId = string.Empty;
            string parentCustomerId = string.Empty;
            string parentMemberNumber = string.Empty;
            string childMemberNumber = string.Empty;
            FlyingClubMember parentFlyingClubMemberDoc = null;


            for (int i = 0; i < childFlyingClubMemberDoc.Count; i++)
            {
                childCustomerId = childFlyingClubMemberDoc[i].CustomerId;

                childMemberNumber = childFlyingClubMemberDoc[i].MemberNumber;

                string queryForParentFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForParentFCMDocument).WithParameter("@ssaMemberNumber", childFlyingClubMemberDoc[i].SSAMemberNumber);

                parentFlyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (parentFlyingClubMemberDoc is not null)
                {
                    if (parentFlyingClubMemberDoc.TierCode.Equals("Gold"))
                    {
                        parentMemberNumber = parentFlyingClubMemberDoc.MemberNumber;

                        parentCustomerId = parentFlyingClubMemberDoc.CustomerId;

                        break;
                    }
                }

            }

            Assert.True(!string.IsNullOrEmpty(parentCustomerId) || !string.IsNullOrEmpty(parentMemberNumber),
                $"The Test Data did not meet the test case requirement .The Parent Member Number (SSAMemberNumber) associated to a child account is not a Gold Tier Member.");


            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverSearch(tokenFixture.GetToken("graphqlv2"), childCustomerId,
                childMemberNumber);

            dynamic sSASearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaHeadMember.customerId.ToString().Equals(parentCustomerId),
                $"The Retreive SSA member Search with child customerId is not as expected. Expected the parent customerId to be" +
                $" {parentCustomerId}  , but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaHeadMember.memberNumber.ToString().Equals(parentMemberNumber),
                $"The Retreive SSA member Search with child customerId is not as expected. Expected the parent membernumber to be" +
                $" {parentMemberNumber}  , but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaHeadMember.relationShipType.ToString().Equals("Linked Gold"),
               $"The Retreive SSA member Search with child customerId is not as expected. " +
               $"Expected the relationship type with parent to be Linked Gold" +
               $" but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaHeadMember.memberStatus.ToString().Equals(parentFlyingClubMemberDoc.Status),
               $"The Retreive SSA member Search with child customerId is not as expected.  " +
               $"Expected the status of the parent to be {parentFlyingClubMemberDoc.Status}" +
               $" but was {sSASearchResult.ToString()}");

            Assert.True(string.IsNullOrEmpty(sSASearchResult.data.loyaltySupplementarySilver.ssaChildMember.ToString()),
               $"The Retreive SSA member Search with child customerId is not as expected. " +
               $"Expected null or empty values for child member details as the search is using child member details" +
               $" but was {sSASearchResult.ToString()}");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(38)]
        public async Task SupplementarySilverAccountSearchByPassingParentAccountDetails_Success()
        {
            #region Pre-Requisite for Fetching the SSA CustomerId from the FC document 

            string queryForChildFCMDocument = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.SSAMemberNumber <>'' order by c._ts desc";

            List<FlyingClubMember> childFlyingClubMemberDoc = cosmosFixture.QueryItems<FlyingClubMember>(queryForChildFCMDocument).ToList();

            string childCustomerId = string.Empty;
            string childMemberNumber = string.Empty;
            string parentCustomerId = string.Empty;
            string parentMemberNumber = string.Empty;
            int j = 0;


            for (int i = 0; i < childFlyingClubMemberDoc.Count; i++)
            {
                childCustomerId = childFlyingClubMemberDoc[i].CustomerId;

                childMemberNumber = childFlyingClubMemberDoc[i].MemberNumber;

                string queryForParentFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and  c.MemberNumber=@ssaMemberNumber";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForParentFCMDocument).WithParameter("@ssaMemberNumber", childFlyingClubMemberDoc[i].SSAMemberNumber);

                FlyingClubMember parentFlyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (parentFlyingClubMemberDoc is not null)
                {
                    if (parentFlyingClubMemberDoc.TierCode.Equals("Gold"))
                    {
                        parentMemberNumber = parentFlyingClubMemberDoc.MemberNumber;

                        parentCustomerId = parentFlyingClubMemberDoc.CustomerId;

                        j = i;

                        break;
                    }
                }

            }

            Assert.True(!string.IsNullOrEmpty(parentCustomerId) || !string.IsNullOrEmpty(parentMemberNumber),
                   $"The Parent Member Number (SSAMemberNumber) associated to a child account is not a Gold Tier Member.");


            #endregion

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverSearch(tokenFixture.GetToken("graphqlv2"), "",
                parentMemberNumber);

            dynamic sSASearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaChildMember.memberNumber.ToString().Equals(childMemberNumber),
                $"The Retreive SSA member Search with parent member number is not as expected. Expected child member number to be  {childMemberNumber}" +
                $" but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaChildMember.customerId.ToString().Equals(childCustomerId),
                $"The Retreive SSA member Search with parent member number is not as expected. Expected child customer Id {childCustomerId}" +
                $" but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaChildMember.relationShipType.ToString().Equals("Supp Silver"),
               $"The Retreive SSA member Search with parent member number is not as expected. " +
               $"Expected the relationship type with child to be Supp Silver" +
               $" but was {sSASearchResult.ToString()}");

            Assert.True(sSASearchResult.data.loyaltySupplementarySilver.ssaChildMember.memberStatus.ToString().Equals(childFlyingClubMemberDoc[j].Status),
               $"The Retreive SSA member Search with parent member number is not as expected. " +
               $"Expected the status of the child to be {childFlyingClubMemberDoc[j].Status}" +
               $" but was {sSASearchResult.ToString()}");

            Assert.True(string.IsNullOrEmpty(sSASearchResult.data.loyaltySupplementarySilver.ssaHeadMember.ToString()),
               $"The Retreive SSA member Search with parent member number is not as expected. " +
               $"Expected null or empty values for head member details as the search is using head member details" +
               $" but was {sSASearchResult.ToString()}");


            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(39)]
        public async Task SupplementarySilverAccountSearch_MandatoryFieldMissingError_WhenBothCustomerIdAndMemberNumberIsNullOrEmpty()
        {
            string customerId = string.Empty;

            string memberNumber = string.Empty;

            #region Validations for SSA member number against the GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverSearch(tokenFixture.GetToken("graphqlv2"), customerId,
                memberNumber);

            dynamic sSASearchResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'customerId' or memberId must be spefied.)";

            Assert.True(sSASearchResult.errors[0]?.message?.ToString().ToLower().Contains(expectedResponse.ToLower()),
                $"Expected {expectedResponse} in the response but found {sSASearchResult.ToString()}");

            #endregion
        }
    }
}




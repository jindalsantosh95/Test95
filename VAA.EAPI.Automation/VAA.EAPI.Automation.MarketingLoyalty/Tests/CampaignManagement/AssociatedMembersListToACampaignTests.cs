using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{
    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class AssociatedMembersListToACampaignTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public AssociatedMembersListToACampaignTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task AssociatedMembersListDownLoadToACampaign_Success()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            object queryResult = AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = (string)queryResult;

            int count = AzureReportingDBHelper.ExecuteCampaignAssociatedMembersStoredProcedure("SP_CAMPAIGN_MEMBERS", campaignCode).Count();

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.AssociatedMembersToACampaign(_tokenFixture.GetToken("graphqlv2"), campaignCode).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string currentDate = DateTime.UtcNow.ToString("MM-dd-yyyy").Replace("-", "");

            string expectedresponse = count > 0 ? $"associatedmembersforcampaign/{campaignCode}_{currentDate}.xlsx" : "No Data Found.";

            Assert.True(result.ToString().Contains(expectedresponse),
                $"Expected the response to be  {expectedresponse} for the searched campaign code {campaignCode} but found {result.ToString()}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task NoDataFoundValidation_WhenInputCampaignCodeIsInvalid()
        {
            string campaignCode = DataGenerator.RandomString(5).ToUpperInvariant();

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.AssociatedMembersToACampaign(_tokenFixture.GetToken("graphqlv2"), campaignCode).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedMessage = "No Data Found.";

            Assert.True(result.ToString().Contains(expectedMessage),
                $"Expected the associated members to campaign list response for invalid Campaign Code to be  {expectedMessage} for the searched campaign code {campaignCode} but found {result.ToString()}.");

            #endregion
        }

    }
}

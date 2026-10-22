using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{

    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CampaignSearchTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        PreRequisiteTestData preRequisiteTestData;

        public CampaignSearchTests(TokenFixture tokenFixture)
        {

            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            preRequisiteTestData = new();

            #region Pre-Requsite to delete rows where same campaign is linked with multiple promotions if any (for matching the campaign counts)

            bool dupCheck = preRequisiteTestData.DeletingMultipleCampaignPromotionLinking();

            Assert.True(dupCheck, "Duplicate Rows exist in the Campaign promotion detail table");

            #endregion Pre-Requsite checks for deleting rows where same campaign is linked with multiple promotions (for matching the campaign counts)
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(30)]
        public async Task CampaignSearch_Using_CampaignCode()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            string campCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", campCode,
                "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            CampaignDetails campaignDetails = JsonConvert.DeserializeObject<CampaignDetails>(result.data.loyaltyCampaignSearch.items[0].ToString());

            Assert.True(campaignDetails is not null,
                $"Expected Campaign details for the searched campaign code {campCode} but found null response");

            Assert.True(campCode.Equals(campaignDetails.CampaignCode),
                $"Expected the CampaignCode in the response to be {campCode} but found {campaignDetails.CampaignCode}.");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(31)]
        public async Task CampaignSearch_Using_CampaignName()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 Name from [dbo].[Campaigns] order by NEWID()";

            string campName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", campName, "",
                "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<CampaignDetails> campaignDetails = JsonConvert.DeserializeObject<List<CampaignDetails>>(result.data.loyaltyCampaignSearch.items.ToString());

            Assert.True(campaignDetails is not null,
               $"Expected Campaign details for the searched campaign Name {campName} but found null response");

            foreach (CampaignDetails campaign in campaignDetails)
            {
                Assert.True(campName.Equals(campaign.Name, StringComparison.OrdinalIgnoreCase),
                     $"Expected the CampaignName in the response to be {campName} but found {campaign.Name}.");
            }

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(32)]
        public async Task CampaignSearch_Using_CampaignType()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 Type from [dbo].[Campaigns] order by NEWID() ";

            string campType = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                "", "", "", "", "", campType).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<CampaignDetails> campaignDetails = JsonConvert.DeserializeObject<List<CampaignDetails>>(result.data.loyaltyCampaignSearch.items.ToString());

            Assert.True(campaignDetails is not null,
               $"Expected Campaign details for the searched campaign type {campType} but found null response");

            foreach (CampaignDetails campaign in campaignDetails)
            {
                Assert.True(campType.Equals(campaign.Type, StringComparison.OrdinalIgnoreCase),
                     $"Expected the CampaignName in the response to be {campType} but found {campaign.Type}.");
            }

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(33)]
        public async Task CampaignSearch_Using_StartDateFromAndStartDateTo()
        {
            #region  Test Data from Reference DB

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string startDateTo = DateTime.UtcNow.ToString("dd-MMM-yyyy");

            string queryForCampaignSearch = $"SELECT count(*) from [dbo].[Campaigns] where StartDate>='{startDateFrom}' and StartDate<='{startDateTo}' ";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                startDateFrom, "", startDateTo, "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearch} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(34)]
        public async Task CampaignSearch_Using_EndDateFromAndEndDateTo()
        {
            #region  Test Data from Reference DB

            string endDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DataGenerator.RandomFutureDate().ToString("dd-MMM-yyyy");

            string queryForCampaignSearch = $"SELECT count(*) from [dbo].[Campaigns] where EndDate>='{endDateFrom}' and EndDate<='{endDateTo}' ";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                "", endDateFrom, "", endDateTo, "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearch} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(35)]
        public async Task CampaignSearch_Using_StartDateFromAndEndDateTo()
        {
            #region  Test Data from Reference DB

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DataGenerator.RandomFutureDate().ToString("dd-MMM-yyyy");

            string queryForCampaignSearch = $"SELECT count(*) from [dbo].[Campaigns] where StartDate>='{startDateFrom}' and EndDate<='{endDateTo}' ";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                startDateFrom, "", "", endDateTo, "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearch} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(36)]
        public async Task CampaignSearch_Using_StartDateFrom()
        {
            #region  Test Data from Reference DB

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string queryForCampaignSearch = $"SELECT count(*) from [dbo].[Campaigns] where StartDate>='{startDateFrom}' ";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                startDateFrom, "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearch} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(37)]
        public async Task CampaignSearch_Using_EndDateTo()
        {
            #region  Test Data from Reference DB           

            string endDateTo = DataGenerator.RandomFutureDate().ToString("dd-MMM-yyyy");

            string queryForCampaignSearch = $"SELECT count(*) from [dbo].[Campaigns] where EndDate<='{endDateTo}' ";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", "",
                "", "", "", endDateTo, "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearch} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(38)]
        public async Task CampaignSearch_Using_CampaignCodeWildCard()
        {
            #region  Test Data from Reference DB      

            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] Where len(CampaignCode)>'3' ORDER BY NEWID()";

            string campCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            string[] wildCards = { "FirstWordWildCard", "LastWordWildCard", "MiddleWordWildCard" };

            Random random = new();

            string wildCard = wildCards[random.Next(wildCards.Length)];

            int rvalue = random.Next(campCode.Length);

            switch (wildCard)
            {
                case "FirstWordWildCard":
                    campCode = string.Concat("*", campCode.Substring(rvalue, campCode.Length - rvalue));
                    break;

                case "LastWordWildCard":
                    campCode = string.Concat(campCode.Substring(0, campCode.Length - rvalue), "*");
                    break;

                case "MiddleWordWildCard":
                    campCode = string.Concat("*", campCode.Substring(rvalue, campCode.Length - rvalue), "*");
                    break;

                default:
                    break;
            }

            string queryForCampaignSearchWildCard = $"SELECT count(*) from [dbo].[Campaigns] Where CampaignCode like '{campCode.Replace("*", "%")}'";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearchWildCard);

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", "", campCode,
                "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<CampaignDetails> campaignDetails = JsonConvert.DeserializeObject<List<CampaignDetails>>(result.data.loyaltyCampaignSearch.items.ToString());

            Assert.True(campaignDetails is not null,
               $"Expected Campaign details for the searched campaign Code {campCode} but found null response");

            foreach (CampaignDetails campaign in campaignDetails)
            {
                Assert.True(campaign.CampaignCode.Has(campCode.Replace("*", "")),
                     $"Expected the CampaignCode in the response to be matching with wildCard {campCode} , but found {campaign.CampaignCode}.");
            }

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearchWildCard} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(39)]
        public async Task CampaignSearch_Using_CampaignNameWildCard()
        {
            #region  Test Data from Reference DB      

            string queryForCampaignSearch = "SELECT TOP 1 Name from [dbo].[Campaigns] Where len(Name)>'6' ORDER BY NEWID()";

            string campName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            string[] wildCards = { "FirstWordWildCard", "LastWordWildCard", "MiddleWordWildCard" };

            Random random = new();

            string wildCard = wildCards[random.Next(wildCards.Length)];

            int rvalue = random.Next(campName.Length);

            switch (wildCard)
            {
                case "FirstWordWildCard":
                    campName = string.Concat("*", campName.Substring(rvalue, campName.Length - rvalue));
                    break;

                case "LastWordWildCard":
                    campName = string.Concat(campName.Substring(0, campName.Length - rvalue), "*");
                    break;

                case "MiddleWordWildCard":
                    campName = string.Concat("*", campName.Substring(rvalue, campName.Length - rvalue), "*");
                    break;

                default:
                    break;
            }

            string queryForCampaignSearchWildCard = $"SELECT count(*) from [dbo].[Campaigns] Where Name like '{campName.Replace("*", "%")}'";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearchWildCard);

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignSearch(tokenFixture.GetToken("graphqlv2"), "", campName, "",
                "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<CampaignDetails> campaignDetails = JsonConvert.DeserializeObject<List<CampaignDetails>>(result.data.loyaltyCampaignSearch.items.ToString());

            Assert.True(campaignDetails is not null,
               $"Expected Campaign details for the searched campaign Name {campName} but found null response");

            foreach (CampaignDetails campaign in campaignDetails)
            {
                Assert.True(campaign.Name.Has(campName.Replace("*", "")),
                     $"Expected the CampaignCode in the response to be matching with wildCard {campName} but found {campaign.Name}.");
            }

            int actualCount = Convert.ToInt32(result.data.loyaltyCampaignSearch.totalCount.ToString());

            Assert.True(count.Equals(actualCount),
                            $"Expected the total count of campaigns for the query {queryForCampaignSearchWildCard} " +
                            $"to be {count} but found {actualCount}.");

            #endregion
        }

    }
}

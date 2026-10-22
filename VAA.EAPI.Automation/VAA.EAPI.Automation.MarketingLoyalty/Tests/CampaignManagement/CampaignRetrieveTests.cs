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
    public class CampaignRetrieveTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public CampaignRetrieveTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(25)]
        public async Task CampaignRetrieve_Using_CampaignCode()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 RowId,CampaignCode,Name,Type,StartDate,EndDate,Status,Description,RegistrationRequired from [dbo].[Campaigns] order by NEWID()";

            object[][] queryResult = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campId = queryResult[0][0]?.ToString();
            string campCode = queryResult[0][1]?.ToString();
            string campName = queryResult[0][2]?.ToString();
            string campType = queryResult[0][3]?.ToString();
            string startDate = queryResult[0][4]?.ToString();
            string endDate = queryResult[0][5]?.ToString();
            string status = queryResult[0][6]?.ToString();
            string description = queryResult[0][7]?.ToString();
            string registrationRequired = queryResult[0][8]?.ToString();

            string queryForPromotionLinking = $"Select PromotionId from dbo.CampaignsPromotionsMapping where CampaignId='{campId}'";

            string promId = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForPromotionLinking);

            string promName = string.Empty;

            string promStartDate = string.Empty;

            string promEndDate = string.Empty;

            string promLastUpdatedBy = string.Empty;

            if (!string.IsNullOrEmpty(promId))
            {
                string queryForPromotionTable = $"Select Name,StartDate,EndDate,LastUpdatedBy from dbo.Promotions where RowId={promId}";

                queryResult = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionTable);

                promName = queryResult[0][0]?.ToString();

                promStartDate = queryResult[0][1]?.ToString();

                promEndDate = queryResult[0][2]?.ToString();

                promLastUpdatedBy = queryResult[0][3]?.ToString();
            }


            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignRetrieve(_tokenFixture.GetToken("graphqlv2"), "", campCode).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("errors"),
                $"Expected valid response for retrieve campaign but found {result.ToString()} for the searched campaign code {campCode}. ");

            CampaignDetails campaignDetails = JsonConvert.DeserializeObject<CampaignDetails>(result.data.loyaltyCampaignRetrieve.campaign.ToString());

            Assert.True(campaignDetails is not null,
                $"Expected Campaign details for the searched campaign code {campCode} but found null response");

            Assert.True(campCode.Equals(campaignDetails.CampaignCode),
                $"Expected the CampaignCode in the response to be {campCode} but found {campaignDetails.CampaignCode}.");

            Assert.True(campId.Equals(campaignDetails.CampaignId),
               $"Expected the CampaignId in the response to be {campId} but found {campaignDetails.CampaignId}.");

            Assert.True(campName.Equals(campaignDetails.Name),
              $"Expected the Campaign Name in the response to be {campName} but found {campaignDetails.Name}.");

            Assert.True(DateTime.Parse(startDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.StartDate).ToString("dd-MMM-yyyy")),
             $"Expected the Campaign StartDate in the response to be {startDate} but found {campaignDetails.StartDate}.");

            Assert.True(DateTime.Parse(endDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.EndDate).ToString("dd-MMM-yyyy")),
             $"Expected the Campaign EndDate in the response to be {endDate} but found {campaignDetails.EndDate}.");

            Assert.True(status.Equals(campaignDetails.Status),
             $"Expected the Campaign Status in the response to be {status} but found {campaignDetails.Status}.");

            Assert.True(campType.Equals(campaignDetails.Type),
             $"Expected the Campaign Type in the response to be {campType} but found {campaignDetails.Type}.");

            Assert.True(description.Equals(campaignDetails.Description),
             $"Expected the Campaign Description in the response to be {description} but found {campaignDetails.Description}.");

            Assert.True(registrationRequired.Equals(campaignDetails.RegistrationRequired),
            $"Expected the Campaign RegReqFlag in the response to be {registrationRequired} but found {campaignDetails.RegistrationRequired}.");


            if (!string.IsNullOrEmpty(promId))
            {

                Assert.True(promId.Equals(campaignDetails.Promotion.PromotionId),
                 $"Expected the PromotionId in the response to be {promId} but found {campaignDetails.Promotion.PromotionId}.");

                Assert.True(promName.Equals(campaignDetails.Promotion.PromotionName),
                  $"Expected the PromotionName in the response to be {promName} but found {campaignDetails.Promotion.PromotionName}.");

                Assert.True(DateTime.Parse(promStartDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.Promotion.StartDate).ToString("dd-MMM-yyyy")),
                 $"Expected the PromotionStartDate in the response to be {promStartDate} but found {campaignDetails.Promotion.StartDate}.");

                Assert.True(DateTime.Parse(promEndDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.Promotion.EndDate).ToString("dd-MMM-yyyy")),
                 $"Expected the PromotionEndDate in the response to be {promEndDate} but found {campaignDetails.Promotion.EndDate}.");

                Assert.True(promLastUpdatedBy.Equals(campaignDetails.Promotion.LastModifiedBy),
                $"Expected the Promotion LastUpdatedBy in the response to be {promLastUpdatedBy} but found {campaignDetails.Promotion.LastModifiedBy}.");

            }

            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(26)]
        public async Task CampaignRetrieve_Using_CampaignId()
        {
            #region  Test Data from Reference DB

            string queryForCampaignSearch = "SELECT TOP 1 RowId,CampaignCode,Name,Type,StartDate,EndDate,Status,Description,RegistrationRequired from [dbo].[Campaigns] order by NEWID()";

            object[][] queryResult = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campId = queryResult[0][0]?.ToString();
            string campCode = queryResult[0][1]?.ToString();
            string campName = queryResult[0][2]?.ToString();
            string campType = queryResult[0][3]?.ToString();
            string startDate = queryResult[0][4]?.ToString();
            string endDate = queryResult[0][5]?.ToString();
            string status = queryResult[0][6]?.ToString();
            string description = queryResult[0][7]?.ToString();
            string registrationRequired = queryResult[0][8]?.ToString();

            string queryForPromotionLinking = $"Select PromotionId from dbo.CampaignsPromotionsMapping where CampaignId='{campId}'";

            string promId = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForPromotionLinking);

            string promName = string.Empty;

            string promStartDate = string.Empty;

            string promEndDate = string.Empty;

            string promLastUpdatedBy = string.Empty;

            if (!string.IsNullOrEmpty(promId))
            {
                string queryForPromotionTable = $"Select Name,StartDate,EndDate,LastUpdatedBy from dbo.Promotions where RowId={promId}";

                queryResult = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionTable);

                promName = queryResult[0][0]?.ToString();

                promStartDate = queryResult[0][1]?.ToString();

                promEndDate = queryResult[0][2]?.ToString();

                promLastUpdatedBy = queryResult[0][3]?.ToString();
            }


            #endregion

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignRetrieve(_tokenFixture.GetToken("graphqlv2"), campId, "").EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("errors"),
                $"Expected valid response for retrieve campaign but found {result.ToString()} for the searched campaign Id {campId}. ");

            CampaignDetails campaignDetails = JsonConvert.DeserializeObject<CampaignDetails>(result.data.loyaltyCampaignRetrieve.campaign.ToString());

            Assert.True(campaignDetails is not null,
                $"Expected Campaign details for the searched campaign code {campCode} but found null response");

            Assert.True(campCode.Equals(campaignDetails.CampaignCode),
                $"Expected the CampaignCode in the response to be {campCode} but found {campaignDetails.CampaignCode}.");

            Assert.True(campId.Equals(campaignDetails.CampaignId),
               $"Expected the CampaignId in the response to be {campId} but found {campaignDetails.CampaignId}.");

            Assert.True(campName.Equals(campaignDetails.Name),
              $"Expected the Campaign Name in the response to be {campName} but found {campaignDetails.Name}.");

            Assert.True(DateTime.Parse(startDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.StartDate).ToString("dd-MMM-yyyy")),
             $"Expected the Campaign StartDate in the response to be {startDate} but found {campaignDetails.StartDate}.");

            Assert.True(DateTime.Parse(endDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.EndDate).ToString("dd-MMM-yyyy")),
             $"Expected the Campaign EndDate in the response to be {endDate} but found {campaignDetails.EndDate}.");

            Assert.True(status.Equals(campaignDetails.Status),
             $"Expected the Campaign Status in the response to be {status} but found {campaignDetails.Status}.");

            Assert.True(campType.Equals(campaignDetails.Type),
             $"Expected the Campaign Type in the response to be {campType} but found {campaignDetails.Type}.");

            Assert.True(description.Equals(campaignDetails.Description),
             $"Expected the Campaign Description in the response to be {description} but found {campaignDetails.Description}.");

            Assert.True(registrationRequired.Equals(campaignDetails.RegistrationRequired),
            $"Expected the Campaign RegReqFlag in the response to be {registrationRequired} but found {campaignDetails.RegistrationRequired}.");


            if (!string.IsNullOrEmpty(promId))
            {

                Assert.True(promId.Equals(campaignDetails.Promotion.PromotionId),
                 $"Expected the PromotionId in the response to be {promId} but found {campaignDetails.Promotion.PromotionId}.");

                Assert.True(promName.Equals(campaignDetails.Promotion.PromotionName),
                  $"Expected the PromotionName in the response to be {promName} but found {campaignDetails.Promotion.PromotionName}.");

                Assert.True(DateTime.Parse(promStartDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.Promotion.StartDate).ToString("dd-MMM-yyyy")),
                 $"Expected the PromotionStartDate in the response to be {promStartDate} but found {campaignDetails.Promotion.StartDate}.");

                Assert.True(DateTime.Parse(promEndDate).ToString("dd-MMM-yyyy").Equals(DateTime.Parse(campaignDetails.Promotion.EndDate).ToString("dd-MMM-yyyy")),
                 $"Expected the PromotionEndDate in the response to be {promEndDate} but found {campaignDetails.Promotion.EndDate}.");

                Assert.True(promLastUpdatedBy.Equals(campaignDetails.Promotion.LastModifiedBy),
                $"Expected the Promotion LastUpdatedBy in the response to be {promLastUpdatedBy} but found {campaignDetails.Promotion.LastModifiedBy}.");

            }

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(27)]
        public async Task CampaignRetrieve_MandatoryFieldMissingError_WhenCampaignCodeandCampaignIdarePassedEmptyValues()
        {
            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignRetrieve(_tokenFixture.GetToken("graphqlv2"), "", "");

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedMessage = "Unhandled exception (Either 'campaignId' or campaignCode must be spefied.)";

            Assert.True(result.ToString().Contains(expectedMessage),
                $"Expected error was {expectedMessage} but found {result.ToString()}. ");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(28)]
        public async Task CampaignRetrieve_NoDataFoundError_WhenCampaignCodeIsInvalid()
        {
            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignRetrieve(_tokenFixture.GetToken("graphqlv2"), "", campaignCode);

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedMessage = "No Data Found";

            Assert.True(result.ToString().Contains(expectedMessage),
                $"Expected error was {expectedMessage} but found {result.ToString()}. ");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(29)]
        public async Task CampaignRetrieve_NoDataFoundError_WhenCampaignIdIsInvalid()
        {
            string campaignId = DataGenerator.RandomNumber(5);

            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.CampaignRetrieve(_tokenFixture.GetToken("graphqlv2"), campaignId, "");

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedMessage = "No Data Found";

            Assert.True(result.ToString().Contains(expectedMessage),
                $"Expected error was {expectedMessage} but found {result.ToString()}. ");

            #endregion

        }
    }
}

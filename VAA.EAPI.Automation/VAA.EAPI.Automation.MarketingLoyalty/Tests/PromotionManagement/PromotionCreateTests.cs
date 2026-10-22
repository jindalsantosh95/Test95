using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.PromotionManagement
{
    [Collection("PromotionManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class PromotionCreateTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public PromotionCreateTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact]
        public async Task A_PromotionCreate_Success()
        {

            string promotionName = DataGenerator.RandomString(10);

            string startDate = DataGenerator.RandomPastDate(+3).ToString("yyyy-MM-dd");

            string endDate = DataGenerator.RandomFutureDate(+500).ToString("yyyy-MM-dd");

            string partnerName = "Virgin Atlantic Airways";

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), promotionName,
                partnerName, startDate, endDate, "Y");

            string expectedResponse = "Promotion created successfully";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.data.loyaltyCreatePromotion.ToString() == expectedResponse,
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.ToString()}");

            string strQueryForPromotionTable = $"Select RowId from dbo.Promotions where Name='{promotionName}'";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionTable);

            Assert.True(!string.IsNullOrEmpty(promotionId), "The Promotion is not Created in Reference Database.");

            strQueryForPromotionTable = $"delete from dbo.Promotions where RowId='{promotionId}'";

            string dbResponse = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionTable);

            Assert.True(string.IsNullOrEmpty(dbResponse),
                "Promotion Deletion From Reference Database Table is Failed ");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task B_PromotionCreate_ShouldThrowMandatoryFieldMissingErrorWhenPromotionNameIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "", "Virgin Atlantic Airways",
                "2022-12-12", "2050-12-12", "Y");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task C_PromotionCreate_ShouldThrowMandatoryFieldMissingErrorWhenPartnerNameIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123", "", "2022-12-12",
                "2050-12-12", "Y");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task D_PromotionCreate_ShouldThrowMandatoryFieldMissingErrorWhenPromotionStartDateIsNull()
        {
            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "", "2050-12-12", "Y");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task E_PromotionCreate_ShouldThrowMandatoryFieldMissingErrorWhenPromotionEndDateIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "2022-12-12", "", "Y");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");


        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task F_PromotionCreate_ShouldThrowMandatoryFieldMissingErrorWhenActiveFlageIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "2022-12-12", "2050-12-12", "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");


        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task G_PromotionCreate_ShouldThrowInvalidDateFormatErrorWhenStartDateIsInvalid()
        {

            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "2022-13-12", "2050-12-12", "Y");

            string expectedResponse = "Invalid Date Formate.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task H_PromotionCreate_ShouldThrowInvalidDateFormatErrorWhenEndDateDateIsInvalid()
        {
            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "2022-12-12", "2050-13-12", "Y");

            string expectedResponse = "Invalid Date Formate.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(8)]
        public async Task I_PromotionCreate_ShouldThrowDuplicatePromotionNameErrorWhenPromotionNameAlreadyExistsInDatabase()
        {
            HttpResponseMessage response = GraphQL.PromotionCreate(tokenFixture.GetToken("graphqlv2"), "PROMO123",
                "Virgin Atlantic Airways", "2022-12-12", "2050-12-12", "Y");

            string expectedResponse = "Already Used Promotion Name.";

            dynamic createPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createPromotionResult.ToString().Contains(expectedResponse),
                $"The Promotion Create response is not as expected. Expected {expectedResponse} " +
                $" but was {createPromotionResult.data.loyaltyCreatePromotion.ToString()}");

        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.InsuranceQuote
{
    [Trait("Category", Traits.BookFeatureInsurance)]
    public class InsuranceDecisionGraphQL
    {
        private readonly TestConfig _testConfig;

        public InsuranceDecisionGraphQL()
        {
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(0, 1, 1, 1);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        public async Task SummaryPricesAreCorrectAfterAcceptingInsurance(LocationType origin, LocationType destination)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;

            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Insurance GraphQL
            var insuranceQuoteRequest = GraphQLRequestBuilder.InsuranceQuote(_testConfig.Internal.BasketId);
            var insuranceQuoteGqlResponse = GraphQLEndpoint.Book(insuranceQuoteRequest).EnsureGQLResponseSuccess();
            var insuranceQuoteResponse = JsonConvert.DeserializeObject<JToken>(await insuranceQuoteGqlResponse.Content.ReadAsStringAsync());

            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            var beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            GraphQLEndpoint.InsuranceDecision(_testConfig.Internal.BasketId, true).EnsureGQLResponseSuccess();

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            var afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateInsuranceCosts(beforeInsuranceBasketResponse, afterInsuranceBasketResponse, true, out var result, insuranceQuoteResponse), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        public async Task SummaryPricesAreCorrectAfterRejectingInsurance(LocationType origin, LocationType destination)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;

            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            GraphQLEndpoint.Book(_testConfig.Internal.BasketId).EnsureGQLResponseSuccess();

            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            var beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            GraphQLEndpoint.InsuranceDecision(_testConfig.Internal.BasketId, false).EnsureGQLResponseSuccess();

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            var afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateInsuranceCosts(beforeInsuranceBasketResponse, afterInsuranceBasketResponse, false, out var result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}
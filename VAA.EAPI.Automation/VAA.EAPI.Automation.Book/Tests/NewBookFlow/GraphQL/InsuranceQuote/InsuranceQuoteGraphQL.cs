using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.InsuranceQuote
{
    [Trait("Category", Traits.BookFeatureInsurance)]
    public class InsuranceQuoteGraphQL
    {
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        public async Task PriceIsReturned(LocationType origin, LocationType destination)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, FareFamilyType.ECON, FlightType.VS, origin, destination, adults: 1, teens: 1, child: 1, infants: 1, rewardFlag: false, out string basketId, out _);

            var insuranceHttpResponse = GraphQLEndpoint.InsuranceQuote(basketId).EnsureGQLResponseSuccess();
            var insuranceResponse = JsonConvert.DeserializeObject<JToken>(await insuranceHttpResponse.Content.ReadAsStringAsync());
            var price = insuranceResponse.SelectToken("data.InsuranceQuote[0].offers[0].priceDetails.amount");

            Assert.True(price != null, $"Price is null, check if response contains a price.\n{insuranceResponse}");
            Assert.True(decimal.Parse(price.ToString()) > 0, $"Price is not greater than 0.\nPrice: {price}.");
        }
    }
}
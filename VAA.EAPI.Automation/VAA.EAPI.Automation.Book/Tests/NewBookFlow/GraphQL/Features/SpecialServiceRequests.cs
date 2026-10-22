using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Features
{
    [Collection("Special Service Requests")]
    public class SpecialServiceRequests
    {
        [Theory(Skip = "Child SSR is disabled behind a feature flag awaiting front end changes.")]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(JourneyType.OneWay, 0, 1, 0)]
        [InlineData(JourneyType.Return, 0, 1, 0)]
        [InlineData(JourneyType.Return, 1, 1, 1)]
        [InlineData(JourneyType.Return, 1, 3, 1)]
        public async Task ChildMealIsAllocated(JourneyType journeyType, int youngAdults, int children, int infants)
        {
            var config = new TestConfig();
            config.Search.JourneyType = journeyType;
            config.Search.Passengers.AddPassengers(adults: 0, youngAdults, children, infants);

            GraphQLHelper.OfferSearchGraphQL(config);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(config.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(config.Internal.BasketId);

            GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(config.Internal.BasketId);

            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();

            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, out string hppUrl, out string pri);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails(config);

            //PostPayment status
            var paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(config.Internal.BasketId, pri);

            var paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();

            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            var orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(config.Internal.BasketId);

            var orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest).EnsureGQLResponseSuccess();

            var orderCreateGQLResult = JsonConvert.DeserializeObject<dynamic>(await orderCreateGQLResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response), response);

            Assert.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errorMessage), $"{errorMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

            GraphQLHelper.ConfirmationInfo(orderCreateGQLResponse, out string leadPassengerFirstName, out string leadPassengerSurname, out string email, out string currency, out double totalPrice, out string outBoundcabinClass, out string inBoundcabinClass, out string pnr);

            // Assert child meals have been allocated
            var childMealCount = RetrievePnrDetails.ReturnAllocatedChildMealCount(pnr);

            // Expected child meals is the number of children multiplied by the number of flights
            var multiplier = config.Search.JourneyType == JourneyType.OneWay ? 1 : 2;
            var expectedChildMeals = config.Search.Passengers.Children.Count * multiplier;
            Assert.True(childMealCount == expectedChildMeals, $"The expected number of child meals is '{expectedChildMeals}' but found '{childMealCount}'.\rPNR: {pnr}");
        }

        [Theory(Skip = "Child SSR is disabled behind a feature flag awaiting front end changes.")]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(JourneyType.OneWay, 1, 0, 1)]
        [InlineData(JourneyType.Return, 1, 0, 1)]
        [InlineData(JourneyType.Return, 0, 1, 0)]
        public async Task ChildMealIsNotAllocated(JourneyType journeyType, int youngAdults, int children, int infants)
        {
            var config = new TestConfig();
            config.Search.JourneyType = journeyType;
            config.Search.Passengers.AddPassengers(adults: 0, youngAdults, children, infants);

            GraphQLHelper.OfferSearchGraphQL(config);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(config.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(config.Internal.BasketId);

            GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(config.Internal.BasketId);

            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();

            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, out string hppUrl, out string pri);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails(config);

            //PostPayment status
            var paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(config.Internal.BasketId, pri);

            var paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();

            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            var orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(config.Internal.BasketId);

            var orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest).EnsureGQLResponseSuccess();

            var orderCreateGQLResult = JsonConvert.DeserializeObject<dynamic>(await orderCreateGQLResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response), response);

            Assert.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errorMessage), $"{errorMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

            GraphQLHelper.ConfirmationInfo(orderCreateGQLResponse, out string leadPassengerFirstName, out string leadPassengerSurname, out string email, out string currency, out double totalPrice, out string outBoundcabinClass, out string inBoundcabinClass, out string pnr);

            // Assert no child meals have been allocated
            var childMealCount = RetrievePnrDetails.ReturnChildMealCount(pnr);

            Assert.True(childMealCount == 0, $"Child meals have been allocated when they shouldn't.\rPNR: {pnr}");
        }
    }
}

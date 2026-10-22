using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Constants;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.TestDataConfig;
using Config = VAA.EAPI.Automation.Core.Config;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom
{
    public class GraphQl
    {
        public static string env = Config.Environment;
        private readonly TestConfig _testConfig;
        public readonly string depatureDate = OfferSearchHelper.DepartureDate();

        public GraphQl()
        {
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(adults: 2, 0, 0, 0);
        }

        [Theory]
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.DotCom)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.BUSFIRST, CabinName.VSUP, true, "CreditCard")]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, CabinName.VSCL, true, "CreditCard")]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.BUSFIRST, CabinName.VSUP, false, "PayPal")]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, CabinName.VSCL, false, "PayPal")]
        public async Task EndToEndWithSeatSelection(LocationType origin, LocationType destination, FareFamilyType fareFamily, CabinName cabinName, bool seats, string paymentType)
        {
            bool validateHppOnly = HeimdallPaymentExecutionSafety.RequiresHppValidationOnly(env);
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            double totalSeatPrice = 0.00;

            if (seats)
            {
                bool responseBool = GraphQLHelper.RetrieveSeatMapGraphQL(_testConfig.Internal.BasketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message);

                if (responseBool)
                {
                    GraphQLHelper.SelectSeatsForAllSectors(_testConfig.Internal.BasketId, fareFamily, retrieveBasketGQLResponse, cabinName, out totalSeatPrice);
                }
            }

            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

            GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            if (validateHppOnly)
            {
                await HostedPaymentAutomation.ValidateAsync(_testConfig, totalSeatPrice);
                return;
            }

            if (paymentType == "PayPal")
            {
                _testConfig.Book.Order.ChildPRI = await HostedPaymentAutomation.SubmitPayPalAsync(
                    _testConfig,
                    usePlaywright: true,
                    expectedTotalPrice: totalSeatPrice);
            }
            else
            {
                _testConfig.Book.Payment = new Core.TestDataConfig.Payment(CardType.Mastercard);
                await HostedPaymentAutomation.SubmitCardAsync(
                    _testConfig,
                    usePlaywright: true,
                    expectedTotalPrice: totalSeatPrice);
            }

            //PostPayment status
            var paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            var paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();
            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            await GraphQLHelper.ExecuteOrderCreate(_testConfig);
        }

        [Theory]
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.DotCom)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, CabinName.VSCL)]
        public async Task EndToEndWithSeatSelectionAndInsurance(LocationType origin, LocationType destination, FareFamilyType fareFamily, CabinName cabinName)
        {
            bool validateHppOnly = HeimdallPaymentExecutionSafety.RequiresHppValidationOnly(env);
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            //Insurance GraphQL 
            var insuranceQuoteResponse = GraphQLEndpoint.InsuranceDecision(_testConfig.Internal.BasketId, true).EnsureGQLResponseSuccess();
            var insuranceQuoteResult = JsonConvert.DeserializeObject<JToken>(await insuranceQuoteResponse.Content.ReadAsStringAsync());

            var insuranceDecisionResponse = GraphQLEndpoint.InsuranceDecision(_testConfig.Internal.BasketId, true).EnsureGQLResponseSuccess();
            var insuranceDecisionResult = JsonConvert.DeserializeObject<JToken>(await insuranceDecisionResponse.Content.ReadAsStringAsync());

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketWithPassengerGQLResponse);

            var afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketWithPassengerGQLResponse.Content.ReadAsStringAsync());

            _testConfig.Book.Cost.InsurancePrice = (double)(afterInsuranceBasketResponse?.SelectToken("data.retrieveBasket.summary.insurancePrice")?.Value<decimal?>() ?? 0m);
            double insuranceDecisionPrice = (double)(insuranceDecisionResult?.SelectToken("data.insuranceDecision.summary.insurancePrice")?.Value<decimal?>() ?? 0m);
            double insuranceQuotePrice = (double)(insuranceQuoteResult?.SelectToken("data.InsuranceQuote[0].products[0].totalPrice")?.Value<decimal?>() ?? 0m);

            Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceQuotePrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance Quote :{insuranceQuotePrice}  ");
            Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceDecisionPrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance decision :{insuranceDecisionPrice}  ");

            double totalSeatPrice = 0.00;

            bool responseBool = GraphQLHelper.RetrieveSeatMapGraphQL(_testConfig.Internal.BasketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message);

            if (responseBool)
            {
                GraphQLHelper.SelectSeatsForAllSectors(_testConfig.Internal.BasketId, fareFamily, retrieveBasketGQLResponse, cabinName, out totalSeatPrice);
            }

            //Payment retrieve - PRI
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            if (validateHppOnly)
            {
                await HostedPaymentAutomation.ValidateAsync(_testConfig, totalSeatPrice);
                return;
            }

            _testConfig.Book.Payment = new Core.TestDataConfig.Payment(CardType.Mastercard);
            await HostedPaymentAutomation.SubmitCardAsync(
                _testConfig,
                usePlaywright: true,
                expectedTotalPrice: totalSeatPrice);

            //Post Payment Status
            var paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            var paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();
            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            await GraphQLHelper.ExecuteOrderCreate(_testConfig);
        }

        [Fact]
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.DotCom)]
        public void NonLoggedInAwardSearch()
        {
            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.JFK;
            _testConfig.Search.IsRewardSearch = true;
            _testConfig.Search.FareFamilyType = FareFamilyType.AWARDECONOMY;

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "", _testConfig.Internal.Bearer).EnsureIsSuccessStatusCode();

            dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResults.ToString().Contains("Unhandled exception (Must be logged in to a valid account.)"), "Did not return expected error message");
        }
    }
}

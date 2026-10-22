using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using static VAA.EAPI.Automation.Core.Config;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom
{
    public class Bff
    {
        public static string env = Config.Environment;
        private readonly TestConfig _testConfig;

        public Bff()
        {
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(adults: 2, 0, 0, 0);
        }

        [Theory]
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.DotCom)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, CabinName.VSCL, false, "PayPal")]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, CabinName.VSCL, true, "CreditCard")]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.JFK, FareFamilyType.BUSFIRST, CabinName.VSUP, true, "CreditCard")]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.JFK, FareFamilyType.BUSFIRST, CabinName.VSUP, false, "PayPal")]
        public async Task EndToEnd_WithSeatsAsync(JourneyType journeyType, LocationType origin, LocationType destination, FareFamilyType fareFamily, CabinName cabinName, bool seats, string paymentType)
        {
            bool validateHppOnly = HeimdallPaymentExecutionSafety.RequiresHppValidationOnly(env);
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = journeyType;

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            string basketId, slice, offerId;

            if (env != "prod")
            {
                WebBFFHelper.SummaryURL(summaryUrl, out basketId, out slice, out offerId);
                summaryUrl = $"{TestResourcesVirginAtlantic.BaseUrl}/{env}/search/summary/{basketId}/{slice}|{offerId}";
            }
            else
            {
                WebBFFHelper.ProductionSummaryUrl(summaryUrl, out basketId, out slice, out offerId);
                summaryUrl = $"{TestResourcesVirginAtlantic.BaseUrl}/search/summary/{basketId}/{slice}|{offerId}";
            }

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            ////Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateBasketCurrency(summaryBFFPayload, getBasket, out string currency), currency);

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            //Hit GQL for Seats
            GraphQLHelper.RetrieveBasket(_testConfig.Internal.BasketId, out HttpResponseMessage retrieveBasketGQLResponse);

            double totalSeatPrice = 0.00;

            if (seats)
            {
                bool seatCheckOutbound = GraphQLHelper.SelectSeatsForSlice(_testConfig.Internal.BasketId, 0, fareFamily, retrieveBasketGQLResponse, cabinName);

                if (seatCheckOutbound)
                {
                    GraphQLHelper.RetrieveBasket(_testConfig.Internal.BasketId, out HttpResponseMessage basketWithSeats);

                    dynamic dynamicBasketResponse = JsonConvert.DeserializeObject<dynamic>(await basketWithSeats.Content.ReadAsStringAsync());

                    Assert.True(GraphQLAssertion.ValidateSeatsForAllPassengers(dynamicBasketResponse, out string errorMessage), errorMessage);

                    if (journeyType == JourneyType.Return)
                    {
                        GraphQLHelper.RetrieveBasket(_testConfig.Internal.BasketId, out HttpResponseMessage retrieveBasketGQLResponseReturn);

                        bool seatCheckInbound = GraphQLHelper.SelectSeatsForSlice(_testConfig.Internal.BasketId, 1, fareFamily, retrieveBasketGQLResponse, cabinName);

                        if (seatCheckInbound)
                        {
                            GraphQLHelper.RetrieveBasket(_testConfig.Internal.BasketId, out HttpResponseMessage basketWithSeatsReturn);

                            dynamicBasketResponse = JsonConvert.DeserializeObject<dynamic>(await basketWithSeatsReturn.Content.ReadAsStringAsync());

                            Assert.True(GraphQLAssertion.ValidateSeatsForAllPassengers(dynamicBasketResponse, out errorMessage), errorMessage);
                        }

                        totalSeatPrice = dynamicBasketResponse.data.retrieveBasket.summary.totalSeatPrice.Value;
                    }
                }
            }

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic paymentResponseBff = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            _testConfig.Book.Order.HppUrl = paymentResponseBff.paymentSessions[0].paymentLink.Value;

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
                await HostedPaymentAutomation.SubmitCardAsync(
                    _testConfig,
                    usePlaywright: true,
                    expectedTotalPrice: totalSeatPrice);
            }

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();
        }
    }
}

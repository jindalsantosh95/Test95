using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class DeepLink
    {
        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public async Task DeepLink_OneWay()
        {
            var departureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, departureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());

            Assert.True(offerSearchResult.offers.Count > 0, $"No offers returned for the search details provided: {offerSearchResult}");

            string offerId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.VS, CabinName.VSCL);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.VS} and cabin name->{CabinName.VSCL}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, departureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, offerId: offerId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(offerPriceResult.offers.Count > 0, $"No offers returned for summary: {offerPriceResult}");

            //Creating deep link summary request
            var deepLinkSummaryRequest = MicroserviceRequestBuilder.DeepLinkOfferSummaryRequest(JourneyType.OneWay, offerPriceResponse, CabinName.VSCL, numberOfadults: 1);

            // post price micro service
            HttpResponseMessage deepLinkSummaryResponse = MicroserviceEndpoint.DeepLinkSummary(deepLinkSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic deepLinkSummaryResult = JsonConvert.DeserializeObject<dynamic>(await deepLinkSummaryResponse.Content.ReadAsStringAsync());

            Assert.True(deepLinkSummaryResult.offers.Count > 0, $"No offers returned for deeplink summary: {deepLinkSummaryResult}");
        }
    }
}
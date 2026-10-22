using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class OfferBasket
    {
        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", Traits.BasketMicroserviceCiCd)]
        public void OfferBasket_TC01_ConnectivityTest()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flexiDateSearch: false);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(origin: LocationType.LHR, destination: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer basket micro service
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();

            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", Traits.BasketMicroserviceCiCd)]
        public void UpdateBasket_TC02_OfferId_OneWay()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(origin: LocationType.LHR, destination: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer basket micro service
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();

            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();

            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(getBasketCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string basketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(basketId, offerId).EnsureIsSuccessStatusCode();

            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(updateBasketResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void DeleteBasketFromCache_TC03_OneWay()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(origin: LocationType.LHR, destination: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer basket micro service
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();

            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            //get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();

            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(getBasketCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string basketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(basketId, offerId).EnsureIsSuccessStatusCode();

            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(updateBasketResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            HttpResponseMessage deleteBasketResponse = MicroserviceEndpoint.DeleteBasket(basketId).EnsureIsSuccessStatusCode();

            Assert.True(deleteBasketResponse.ReadResponseAsString() == "true", "true was not returned in the delete basket response body");

            HttpResponseMessage getDeletedBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL);

            Assert.True(getDeletedBasketCreateResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError, $"Internal server error was not returned from get basket endpoint after delete was called, response was {updateBasketResponse.ReadResponseAsString()}");
        }
    }
}
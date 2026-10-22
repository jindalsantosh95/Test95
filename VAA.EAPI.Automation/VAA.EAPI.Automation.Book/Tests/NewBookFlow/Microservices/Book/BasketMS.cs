using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Delta.Models.OfferSearch;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices.Book
{
    public class BasketMS : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly TestConfig _testConfig;

        public BasketMS(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(adults: 0, 1, 1, 1);
        }

        [Theory]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1, CabinName.VSLT)] //Economy Light
        [InlineData(LocationType.LHR, LocationType.ATL, 1, 2, 0, 0, CabinName.VSCL)] //Economy Classic
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 1, 0, CabinName.VSDT)] //Economy Delight
        [InlineData(LocationType.JFK, LocationType.LHR, 2, 1, 1, 1, CabinName.VSPE)] //Premium
        [InlineData(LocationType.LHR, LocationType.DEL, 2, 0, 1, 0, CabinName.VSUP)] //Upper
        public async Task BasketMS_Validation_OW_VirginAllPax(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, CabinName cabinName)
        {
            _testConfig.Internal.TestCaseName = "Basket_Microservice_Virgin_Validation";
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfyouths, numberOfchildren, numberOfinfants);
            _testConfig.Search.CabinName = cabinName;

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());
            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            //Get offer for Virgin
            _testConfig.Internal.OfferId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.VS, _testConfig.Search.CabinName);
            Assert.True(!_testConfig.Internal.OfferId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.VS} and cabin name->{_testConfig.Search.CabinName}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");
            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            //UpsellOfferID
            _testConfig.Internal.OfferId = OfferSearchHelper.GetUpsellOfferId(offerPriceResponse);
            var upsellPriceRequest = MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage upsellOfferPriceResponse = MicroserviceEndpoint.Price(upsellPriceRequest).EnsureIsSuccessStatusCode();
            dynamic upsellOfferPriceResult = JsonConvert.DeserializeObject<dynamic>(upsellOfferPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(upsellOfferPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);
            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();
            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");

            //Basket Request and response
            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(_testConfig);
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();
            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(_testConfig.Internal.BasketId, _testConfig.Internal.OfferId).EnsureIsSuccessStatusCode();
            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(await updateBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.OfferId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            //People update
            NBFMircoserviceHelper.UpdateAllPassengerDetails(updateBasketResult, _testConfig.Internal.BasketId);

            //Updating valid Basket contact details
            var contactBasketRequest = NBFMircoserviceHelper.ContactBasketRequstMS(_testConfig);
            HttpResponseMessage updateContactResponse = MicroserviceEndpoint.UpdateContactBasket(_testConfig.Internal.BasketId, contactBasketRequest).EnsureIsSuccessStatusCode();
            dynamic updateContactResult = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == updateContactResult.basketId.Value, $"Basket id not matched with updated basket");

            //Updating valid Basket Terms and Condition
            var termsAndConditionsBasketRequest = NBFMircoserviceHelper.TermsAndConditionsBasketRequstMS();
            HttpResponseMessage termsAndConditionsBasketResponse = MicroserviceEndpoint.TermsAndConditionsBasketMS(_testConfig.Internal.BasketId, termsAndConditionsBasketRequest).EnsureIsSuccessStatusCode();
            dynamic termsAndConditionsBasketResult = JsonConvert.DeserializeObject<dynamic>(await termsAndConditionsBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == termsAndConditionsBasketResult.basketId.Value, $"Basket id not matched with updated basket");

            // get basket create response
            getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            Assert.True(getBasketResult.validation == "VALID", $"Validation value in MS BasketResponse is INVALID after update \n----------\n Response : {getBasketResult} \n----------\n  ");
        }

        [Fact]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        public async Task BasketMS_Validation_OW_NonVirginAllPax()
        {
            _testConfig.Search.CabinName = CabinName.MAIN;
            _testConfig.Internal.TestCaseName = "BasketMS_Validation_OW_NonVirginAllPax";

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());
            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            //Get offer for Virgin
            _testConfig.Internal.OfferId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.DL, _testConfig.Search.CabinName);
            Assert.True(!_testConfig.Internal.OfferId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{_testConfig.Search.CabinName}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");
            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);
            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();
            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");

            //Basket Request and response
            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(_testConfig);
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();
            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(_testConfig.Internal.BasketId, _testConfig.Internal.OfferId).EnsureIsSuccessStatusCode();
            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(await updateBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.OfferId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            //People update
            NBFMircoserviceHelper.UpdateAllPassengerDetails(updateBasketResult, _testConfig.Internal.BasketId);

            //Updating valid Basket contact details
            var contactBasketRequest = NBFMircoserviceHelper.ContactBasketRequstMS(_testConfig);
            HttpResponseMessage updateContactResponse = MicroserviceEndpoint.UpdateContactBasket(_testConfig.Internal.BasketId, contactBasketRequest).EnsureIsSuccessStatusCode();
            dynamic updateContactResult = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == updateContactResult.basketId.Value, $"Basket id not matched with updated basket");

            //Updating valid Basket Terms and Condition
            var termsAndConditionsBasketRequest = NBFMircoserviceHelper.TermsAndConditionsBasketRequstMS();
            HttpResponseMessage termsAndConditionsBasketResponse = MicroserviceEndpoint.TermsAndConditionsBasketMS(_testConfig.Internal.BasketId, termsAndConditionsBasketRequest).EnsureIsSuccessStatusCode();
            dynamic termsAndConditionsBasketResult = JsonConvert.DeserializeObject<dynamic>(await termsAndConditionsBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == termsAndConditionsBasketResult.basketId.Value, $"Basket id not matched with updated basket");

            // get basket create response
            getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            Assert.True(getBasketResult.validation == "VALID", $"Validation value in MS BasketResponse is INVALID after update \n----------\n Response : {getBasketResult} \n----------\n  ");
        }

        [Fact]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        public async Task BasketMS_Validation_OW_VirginAndNonVirginmAllPax()
        {
            _testConfig.Search.CabinName = CabinName.MAIN;
            _testConfig.Search.FlightType = FlightType.DL;
            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.ATL;
            _testConfig.Internal.TestCaseName = "BasketMS_Validation_OW_VirginAndNonVirginAllPax";

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());
            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            //Get offer for Virgin
            _testConfig.Internal.OfferId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, _testConfig.Search.FlightType, _testConfig.Search.CabinName);
            Assert.True(!_testConfig.Internal.OfferId.IsNullOrEmpty(), $"No offer matched with flight type->{_testConfig.Search.FlightType} and cabin name->{_testConfig.Search.CabinName}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");
            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);
            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();
            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");

            //Basket Request and response
            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(_testConfig);
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();
            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(_testConfig.Internal.BasketId, _testConfig.Internal.OfferId).EnsureIsSuccessStatusCode();
            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(await updateBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.OfferId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            //People update
            NBFMircoserviceHelper.UpdateAllPassengerDetails(updateBasketResult, _testConfig.Internal.BasketId);

            //Updating valid Basket contact details
            var contactBasketRequest = NBFMircoserviceHelper.ContactBasketRequstMS(_testConfig);
            HttpResponseMessage updateContactResponse = MicroserviceEndpoint.UpdateContactBasket(_testConfig.Internal.BasketId, contactBasketRequest).EnsureIsSuccessStatusCode();
            dynamic updateContactResult = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == updateContactResult.basketId.Value, $"Basket id not matched with updated basket");

            //Updating valid Basket Terms and Condition
            var termsAndConditionsBasketRequest = NBFMircoserviceHelper.TermsAndConditionsBasketRequstMS();
            HttpResponseMessage termsAndConditionsBasketResponse = MicroserviceEndpoint.TermsAndConditionsBasketMS(_testConfig.Internal.BasketId, termsAndConditionsBasketRequest).EnsureIsSuccessStatusCode();
            dynamic termsAndConditionsBasketResult = JsonConvert.DeserializeObject<dynamic>(await termsAndConditionsBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == termsAndConditionsBasketResult.basketId.Value, $"Basket id not matched with updated basket");

            // get basket create response
            getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            Assert.True(getBasketResult.validation == "VALID", $"Validation value in MS BasketResponse is INVALID after update \n----------\n Response : {getBasketResult} \n----------\n  ");
        }

        [Theory]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1, CabinName.VSLT)] //Economy Light
        [InlineData(LocationType.LHR, LocationType.ATL, 1, 2, 0, 0, CabinName.VSCL)] //Economy Classic
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 1, 0, CabinName.VSDT)] //Economy Delight
        [InlineData(LocationType.JFK, LocationType.LHR, 2, 1, 1, 1, CabinName.VSPE)] //Premium
        [InlineData(LocationType.LHR, LocationType.DEL, 2, 0, 1, 0, CabinName.VSUP)] //Upper
        public async Task BasketMS_Validation_RT_VirginAllPax(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, CabinName cabinName)
        {
            _testConfig.Internal.TestCaseName = "Basket_Microservice_Virgin_Validation";
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfyouths, numberOfchildren, numberOfinfants);
            _testConfig.Search.CabinName = cabinName;

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());
            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            //Get offer for Virgin
            _testConfig.Internal.OfferId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchResponse, _testConfig.Search.FlightType, _testConfig.Search.CabinName);
            Assert.True(!_testConfig.Internal.OfferId.IsNullOrEmpty(), $"No offer matched with flight type->{_testConfig.Search.FlightType} and cabin name->{_testConfig.Search.CabinName}");

            //Generating offer search request for return
            _testConfig.Search.CurrentTripIndexId = "1";
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode(); ;
            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //find offer id
            _testConfig.Internal.ReturnOfferId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchReturnResponse, _testConfig.Search.FlightType, _testConfig.Search.CabinName);
            _testConfig.Internal.OfferId = _testConfig.Internal.ReturnOfferId;

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");
            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);
            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();
            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");

            //Basket Request and response
            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(_testConfig);
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();
            string basket_getURL = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(_testConfig.Internal.BasketId, _testConfig.Internal.OfferId).EnsureIsSuccessStatusCode();
            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(await updateBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.OfferId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            //People update
            NBFMircoserviceHelper.UpdateAllPassengerDetails(updateBasketResult, _testConfig.Internal.BasketId);

            //Updating valid Basket contact details
            var contactBasketRequest = NBFMircoserviceHelper.ContactBasketRequstMS(_testConfig);
            HttpResponseMessage updateContactResponse = MicroserviceEndpoint.UpdateContactBasket(_testConfig.Internal.BasketId, contactBasketRequest).EnsureIsSuccessStatusCode();
            dynamic updateContactResult = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == updateContactResult.basketId.Value, $"Basket id not matched with updated basket");

            //Updating valid Basket Terms and Condition
            var termsAndConditionsBasketRequest = NBFMircoserviceHelper.TermsAndConditionsBasketRequstMS();
            HttpResponseMessage termsAndConditionsBasketResponse = MicroserviceEndpoint.TermsAndConditionsBasketMS(_testConfig.Internal.BasketId, termsAndConditionsBasketRequest).EnsureIsSuccessStatusCode();
            dynamic termsAndConditionsBasketResult = JsonConvert.DeserializeObject<dynamic>(await termsAndConditionsBasketResponse.Content.ReadAsStringAsync());
            Assert.True(_testConfig.Internal.BasketId == termsAndConditionsBasketResult.basketId.Value, $"Basket id not matched with updated basket");

            // get basket create response
            getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();
            getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());
            _testConfig.Internal.BasketId = getBasketResult.basketId.Value;

            Assert.True(getBasketResult.validation == "VALID", $"Validation value in MS BasketResponse is INVALID after update \n----------\n Response : {getBasketResult} \n----------\n  ");
        }
    }
}
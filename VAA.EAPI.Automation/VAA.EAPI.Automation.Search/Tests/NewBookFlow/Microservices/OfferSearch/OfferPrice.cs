using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class OfferPrice
    {
        private bool requireUsableOfferForSignOff;

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "DirectMS")]
        public void SharedServicesSignOff_OfferPrice_DirectContract()
        {
            requireUsableOfferForSignOff = true;
            OfferPrice_TC01_OneWay_Virgin(LocationType.LHR, LocationType.JFK, 1, 0, 1, 0, CabinName.VSDT);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1, CabinName.VSLT)] //Economy Light
        [InlineData(LocationType.LHR, LocationType.ATL, 1, 2, 0, 0, CabinName.VSCL)] //Economy Classic
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 1, 0, CabinName.VSDT)] //Economy Delight
        [InlineData(LocationType.LHR, LocationType.SEA, 2, 1, 1, 1, CabinName.VSPE)] //Premium
        [InlineData(LocationType.LHR, LocationType.DEL, 2, 0, 1, 0, CabinName.VSUP)] //Upper
        public void OfferPrice_TC01_OneWay_Virgin(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, CabinName cabinName)
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"OfferMS direct pricing sign-off requires OfferSearch inventory before invoking OfferPrice. Response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            string offerId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.VS, cabinName);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.VS} and cabin name->{cabinName}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            string upsellOfferId = OfferSearchHelper.GetUpsellOfferId(offerPriceResponse);

            var upsellPriceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: upsellOfferId);

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
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC02_OneWay_NonVirgin()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            string offerId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.DL, CabinName.MAIN);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{CabinName.MAIN}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId);

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
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC03_OneWay_VirginAndNonVirgin()
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.ATL, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            string offerId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchResponse, FlightType.DL, CabinName.MAIN);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{CabinName.MAIN}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.ATL, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId);

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
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC04_RoundTrip_Virgin()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //find offer id
            string offerId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchResponse, FlightType.VS, CabinName.VSLT);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched for {FlightType.VS} and {CabinName.VSLT} in \n response: {offerSearchResult}");

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //find offer id
            string returnOfferId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchReturnResponse, FlightType.VS, CabinName.VSLT);

            Assert.True(!returnOfferId.IsNullOrEmpty(), $"No offer matched with for {FlightType.VS} and {CabinName.VSLT} in \n response: {offerSearchReturnResult}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

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
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC05_RoundTrip_NonVirgin()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.CUN, locationThree: LocationType.CUN, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchResponse, FlightType.DL, CabinName.BE);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{CabinName.BE}");

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.CUN, locationThree: LocationType.CUN, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var returnOfferId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchReturnResponse, FlightType.DL, CabinName.VSLT);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{CabinName.VSLT}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.CUN, locationThree: LocationType.CUN, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

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
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC06_RoundTrip_VirginAndNonVirgin()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchResponse, FlightType.VS, CabinName.VSCL);

            Assert.True(!offerId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.VS} and cabin name->{CabinName.VSCL}");

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var returnOfferId = OfferSearchHelper.FindOfferByOperatingCarrierAndFamilyType(offerSearchReturnResponse, FlightType.DL, CabinName.BE);

            Assert.True(!returnOfferId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.DL} and cabin name->{CabinName.BE}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

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
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, Currency.GBP)]
        [InlineData(LocationType.ATL, LocationType.LHR, Currency.USD)]
        [InlineData(LocationType.LHR, LocationType.YYZ, Currency.GBP)]
        [InlineData(LocationType.YYZ, LocationType.LHR, Currency.CAD)]
        [InlineData(LocationType.CDG, LocationType.JFK, Currency.EUR)]
        [InlineData(LocationType.BGI, LocationType.LHR, Currency.BBD)]
        [InlineData(LocationType.ANU, LocationType.LHR, Currency.XCD)]
        [InlineData(LocationType.GVA, LocationType.JFK, Currency.CHF)]
        public void OfferPrice_TC07_RoundTrip_TotalTaxCheckInLocalCurrencyForAward(LocationType origin, LocationType destination, Currency currency)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateTotalTaxInLocalCurrency(offerSearchResponse, currency.ToString(), out string message), message);

            Assert.True(OfferSearchAssertions.ValidateTotalPointsForAward(offerSearchResponse, out string totalPointsOutbound), totalPointsOutbound);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateTotalTaxInLocalCurrency(offerSearchReturnResponse, currency.ToString(), out string inboundTaxInfo), inboundTaxInfo);

            Assert.True(OfferSearchAssertions.ValidateTotalPointsForAward(offerSearchResponse, out string totalPointsInbound), totalPointsInbound);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryOfferId = offerPriceResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(summaryOfferId.Any(), "No offers have been returned for the offer id provided");

            Assert.True(OfferSearchAssertions.ValidateTotalTaxInLocalCurrency(offerPriceResponse, currency.ToString(), out string summaryTaxInfo), summaryTaxInfo);

            Assert.True(OfferSearchAssertions.ValidateTotalPointsForAward(offerSearchResponse, out string totalPointsSummary), totalPointsSummary);
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC08_DirectWithStop_OneWay()
        {
            var deptartureDate = OfferSearchHelper.DepartureDateSpecificDayOfWeek(DayOfWeek.Thursday)[0];

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            Assert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            Assert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            Assert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            Assert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            string offerId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectWithStop(offerSearchResponse, FlightType.VS, CabinName.VSCL);

            Assert.True(!offerId.IsNullOrEmpty(), $"Direct with stop offer not return for requested search: {offerSearchRequest} \n response: {offerSearchResult}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            Assert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerPriceResponse, out string summaryOriginDestination), summaryOriginDestination);

            Assert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerPriceResponse, out string summaryPaxList), summaryPaxList);

            Assert.True(OfferSearchAssertions.ValidateTotalAmount(offerSearchResponse, offerPriceResponse, offerId, out string summaryTotalAmount), summaryTotalAmount);

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerPriceResponse, offerSearchRequest, out string summaryFareId), summaryFareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerPriceResponse, offerSearchRequest, out string summaryFareSegments), summaryFareSegments);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.MAN, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.BGI)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.MCO)]
        [InlineData(LocationType.MAN, LocationType.MCO)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.LAX)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.MIA)]
        [InlineData(LocationType.LHR, LocationType.TPA)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.BLR)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.EDI, LocationType.MCO)]
        [InlineData(LocationType.LHR, LocationType.SFO)]
        [InlineData(LocationType.LHR, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.YYZ)]
        public void OfferPrice_TC09_Return_ExUK(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchRequest, offerSearchResponse, out string fareSegments), fareSegments);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchReturnRequest, offerSearchReturnResponse, out string inboundFareSegments), inboundFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        [InlineData(LocationType.JFK, LocationType.MAN)]
        [InlineData(LocationType.MCO, LocationType.LHR)]
        [InlineData(LocationType.MCO, LocationType.MAN)]
        [InlineData(LocationType.LAX, LocationType.LHR)]
        [InlineData(LocationType.LAX, LocationType.MAN)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.MIA, LocationType.LHR)]
        [InlineData(LocationType.TPA, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        [InlineData(LocationType.YYZ, LocationType.LHR)]
        public void OfferPrice_TC10_Return_ExUS(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchRequest, offerSearchResponse, out string fareSegments), fareSegments);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchReturnRequest, offerSearchReturnResponse, out string inboundFareSegments), inboundFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 4, 4, 1, 0)]
        [InlineData(LocationType.MAN, LocationType.JFK, 4, 2, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.LAX, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.LOS, 3, 2, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.BOM, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.DEL, 4, 4, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.MCO, 1, 1, 1, 1)]
        [InlineData(LocationType.MAN, LocationType.MCO, 4, 3, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.MIA, 4, 4, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.SFO, 2, 2, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.LAS, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.MBJ, 3, 2, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.TPA, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.PVG, 1, 1, 0, 0)]
        [InlineData(LocationType.BOS, LocationType.LHR, 1, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.CPT, 5, 1, 1, 1)]
        [InlineData(LocationType.DEL, LocationType.LHR, 5, 1, 1, 1)]
        [InlineData(LocationType.BLR, LocationType.LHR, 5, 1, 1, 1)]
        [InlineData(LocationType.BOM, LocationType.LHR, 5, 1, 1, 1)]
        public void OfferPrice_TC11_OneWay_Reward(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, rewardFlag: true);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            Assert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerPriceResponse, out string summaryOriginDestination), summaryOriginDestination);

            Assert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerPriceResponse, out string summaryPaxList), summaryPaxList);

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerPriceResponse, offerSearchRequest, out string summaryFareId), summaryFareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerPriceResponse, offerSearchRequest, out string summaryFareSegments), summaryFareSegments);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.MAN, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.BGI)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.MCO)]
        [InlineData(LocationType.MAN, LocationType.MCO)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.LAX)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.MIA)]
        [InlineData(LocationType.LHR, LocationType.TPA)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.BLR)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.EDI, LocationType.MCO)]
        [InlineData(LocationType.LHR, LocationType.SFO)]
        [InlineData(LocationType.LHR, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.YYZ)]
        public void OfferPrice_TC12_Return_Reward_ExUK(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchRequest, offerSearchResponse, out string fareSegments), fareSegments);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchReturnRequest, offerSearchReturnResponse, out string inboundFareSegments), inboundFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        [InlineData(LocationType.JFK, LocationType.MAN)]
        [InlineData(LocationType.MCO, LocationType.LHR)]
        [InlineData(LocationType.MCO, LocationType.MAN)]
        [InlineData(LocationType.LAX, LocationType.LHR)]
        [InlineData(LocationType.LAX, LocationType.MAN)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.MIA, LocationType.LHR)]
        [InlineData(LocationType.TPA, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        [InlineData(LocationType.YYZ, LocationType.LHR)]
        public void OfferPrice_TC13_Return_Reward_ExUS(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchRequest, offerSearchResponse, out string fareSegments), fareSegments);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //get random offer id
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchReturnRequest, offerSearchReturnResponse, out string inboundFareSegments), inboundFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferPrice_TC14_AwardSearchDirectWithStop_OneWay()
        {
            var deptartureDate = OfferSearchHelper.DepartureDateSpecificDayOfWeek(DayOfWeek.Thursday)[0];

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            Assert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            Assert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            Assert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            Assert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            string offerId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectWithStop(offerSearchResponse, FlightType.VS, CabinName.VSCL);

            Assert.True(!offerId.IsNullOrEmpty(), $"Direct with stop offer not return for requested search: {offerSearchRequest} \n response: {offerSearchResult}");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.ANU, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerId, rewardFlag: true);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            Assert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerPriceResponse, out string summaryOriginDestination), summaryOriginDestination);

            Assert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerPriceResponse, out string summaryPaxList), summaryPaxList);

            Assert.True(OfferSearchAssertions.ValidateTotalAmount(offerSearchResponse, offerPriceResponse, offerId, out string summaryTotalAmount), summaryTotalAmount);

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerPriceResponse, offerSearchRequest, out string summaryFareId), summaryFareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerPriceResponse, offerSearchRequest, out string summaryFareSegments), summaryFareSegments);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LON, LocationType.JFK)]
        [InlineData(LocationType.LON, LocationType.ATL)]
        [InlineData(LocationType.LON, LocationType.BGI)]
        [InlineData(LocationType.LON, LocationType.MCO)]
        [InlineData(LocationType.LON, LocationType.LOS)]
        [InlineData(LocationType.LON, LocationType.LAX)]
        [InlineData(LocationType.LON, LocationType.LAS)]
        [InlineData(LocationType.LON, LocationType.BOS)]
        [InlineData(LocationType.LON, LocationType.SEA)]
        [InlineData(LocationType.NYC, LocationType.LON)]
        [InlineData(LocationType.NYC, LocationType.LHR)]
        [InlineData(LocationType.NYC, LocationType.MAN)]
        [InlineData(LocationType.MCO, LocationType.LON)]
        [InlineData(LocationType.LAX, LocationType.LON)]
        [InlineData(LocationType.ATL, LocationType.LON)]
        [InlineData(LocationType.MIA, LocationType.LON)]
        public void CityCodeBasedSearchAndSummary_TC15_Return(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for outbound search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchRequest, offerSearchResponse, out string fareSegments), fareSegments);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            //get random offer id for inbound
            string returnOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchReturnResponse);

            Assert.True(!returnOfferId.IsNullOrEmpty(), "No offer returned for inbound search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchReturnRequest, offerSearchReturnResponse, out string inboundFareSegments), inboundFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            //get random offer id for summary
            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.SEA, LocationType.AMS, LocationType.AMS, LocationType.JFK, LocationType.JFK, LocationType.LHR)]
        [InlineData(LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.BOS)]
        public void MultiCitySearchAndSummary_TC16_Return(LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix)
        {
            var departureDateOne = OfferSearchHelper.DepartureDate();
            var departureDateTwo = OfferSearchHelper.ReturnDate(departureDateOne);
            var departureDateThree = OfferSearchHelper.ReturnDate(departureDateTwo);

            //Generating offer search request
            var offerSearchSliceZeroRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.MultiCity, departureDate: departureDateOne, flightTwoDepartureDate: departureDateTwo, flightThreeDepartureDate: departureDateThree, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, currentTripIndexId: "0");

            // post offer search micro service
            HttpResponseMessage offerSearchSliceZeroResponse = MicroserviceEndpoint.OfferSearch(offerSearchSliceZeroRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchSliceZeroResponse)) return;

            offerSearchSliceZeroResponse.EnsureIsSuccessStatusCode();

            //get random offer id
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchSliceZeroResponse);

            Assert.True(!offerId.IsNullOrEmpty(), "No offer returned for outbound search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchSliceZeroRequest, offerSearchSliceZeroResponse, out string fareSegments), fareSegments);

            //Generating offer search first slice request
            var offerSearchSliceOneRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.MultiCity, departureDate: departureDateOne, flightTwoDepartureDate: departureDateTwo, flightThreeDepartureDate: departureDateThree, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, currentTripIndexId: "1", offerId: offerId);

            // post offer search micro service
            HttpResponseMessage offerSearchSliceOneResponse = MicroserviceEndpoint.OfferSearch(offerSearchSliceOneRequest).EnsureIsSuccessStatusCode();

            //get random offer id for inbound
            string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchSliceOneResponse);

            Assert.True(!sliceOneOfferId.IsNullOrEmpty(), "No offer returned for inbound search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchSliceOneRequest, offerSearchSliceOneResponse, out string sliceOneFareSegments), sliceOneFareSegments);

            //Generating offer search 2nd slice request
            var offerSearchSliceTwoRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.MultiCity, departureDate: departureDateOne, flightTwoDepartureDate: departureDateTwo, flightThreeDepartureDate: departureDateThree, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, currentTripIndexId: "2", offerId: sliceOneOfferId);

            // post offer search micro service
            HttpResponseMessage offerSearchSliceTwoResponse = MicroserviceEndpoint.OfferSearch(offerSearchSliceTwoRequest).EnsureIsSuccessStatusCode();

            //get random offer id for inbound
            string sliceTwoOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerSearchSliceTwoResponse);

            Assert.True(!sliceTwoOfferId.IsNullOrEmpty(), "No offer returned for inbound search");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(offerSearchSliceTwoRequest, offerSearchSliceTwoResponse, out string sliceTwoFareSegments), sliceTwoFareSegments);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.MultiCity, departureDate: departureDateOne, flightTwoDepartureDate: departureDateTwo, flightThreeDepartureDate: departureDateThree, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: sliceTwoOfferId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            //get random offer id for summary
            string summaryOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromMSResponse(offerPriceResponse, randomOffer: false);

            Assert.True(!summaryOfferId.IsNullOrEmpty(), "No offer returned for summary");

            Assert.True(OfferSearchAssertions.ValidateFareSegments(priceRequest, offerPriceResponse, out string summaryFareSegments), summaryFareSegments);

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(summaryFareRulesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");
        }
    }
}

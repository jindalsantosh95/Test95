using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using MicroserviceRequestBuilder = VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.MicroserviceRequestBuilder;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Production.Microservices
{
    public class FlightSearchMS
    {
        public static string env = Config.Environment;

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task FlightSearch_OneWay()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flexiDateSearch: false);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(await offerSearchResponse.Content.ReadAsStringAsync());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            string offerId = offerIds[0];

            var createBasketRequest = MicroserviceRequestBuilder.CreateBasketRequest(origin: LocationType.LHR, destination: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            // post offer basket micro service
            HttpResponseMessage offerBasketResponse = MicroserviceEndpoint.CreateBasket(createBasketRequest).EnsureIsSuccessStatusCode();

            dynamic offerBasketResult = JsonConvert.DeserializeObject<dynamic>(await offerBasketResponse.Content.ReadAsStringAsync());

            Assert.True(!string.IsNullOrEmpty(offerBasketResult.basketId.Value), "No basket id exist");

            // get the basket url
            var basketGetUrl = offerBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = MicroserviceEndpoint.GetBasket(basketGetUrl).EnsureIsSuccessStatusCode();

            dynamic getBasketResult = JsonConvert.DeserializeObject<dynamic>(await getBasketCreateResponse.Content.ReadAsStringAsync());

            string basketId = getBasketResult.basketId.Value;

            //post basket update
            HttpResponseMessage updateBasketResponse = MicroserviceEndpoint.UpdateBasketWithOfferId(basketId, offerId).EnsureIsSuccessStatusCode();

            dynamic updateBasketResult = JsonConvert.DeserializeObject<dynamic>(await updateBasketResponse.Content.ReadAsStringAsync());

            Assert.True(offerId == updateBasketResult.offerIds[0].Value, $"Offer id not matched with updated basket");

            //Hit GetOffer microservice
            var offerResponse = MicroserviceEndpoint.GetOffer(offerId).EnsureIsSuccessStatusCode();

            // Generating brand ms request
            var brandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage brandMSResponse = MicroserviceEndpoint.BrandMS(brandMSRequest).EnsureIsSuccessStatusCode();

            dynamic brandMSResult = JsonConvert.DeserializeObject<dynamic>(await brandMSResponse.Content.ReadAsStringAsync());

            Assert.True(brandMSResult.itineraries.Count > 0, "No itineraries exists");

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            string summaryOfferId = offerPriceResult["offers"][0]["offerId"].ToString();

            //calling get fare rules end point for summary
            HttpResponseMessage summaryFareRulesResponse = MicroserviceEndpoint.GetFareRulesForSummary(summaryOfferId).EnsureIsSuccessStatusCode();

            dynamic summaryFareRulesResult = JsonConvert.DeserializeObject<dynamic>(await summaryFareRulesResponse.Content.ReadAsStringAsync());

            Assert.True(summaryFareRulesResult.fareRules.Count > 0, "No fare rules returned for the summary offer id provided");

            //Generating sustainability request
            var sustainabilityRequest = MicroserviceRequestBuilder.SustainabilityMSRequest(origin: LocationType.LHR, destination: LocationType.JFK, "", deptartureDate, "Economy", 1);

            //post sustainability API
            HttpResponseMessage sustainabilityResponse = MicroserviceEndpoint.Sustainability(sustainabilityRequest).EnsureIsSuccessStatusCode();

            dynamic sustainabilityResult = JsonConvert.DeserializeObject<dynamic>(await sustainabilityResponse.Content.ReadAsStringAsync());

            Assert.True(sustainabilityResult.Count > 0, "No sustainability details returned");
        }

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task FlightFlexDateSearch_OneWay()
        {
            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 1, numberOfinfants: 0, flexiDateSearch: true, rewardFlag: false);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferFlexiSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());

            Assert.True(offerSearchResult.flexibleDateCalendar.columnText.Count > 0, "No offers have been returned for the search details provided");
        }

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task FlightCalenderSearch_OneWay()
        {
            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, calenderSearch: true, rewardFlag: false);

            // calling post OfferCalendarSearch end point
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferCalendarSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());

            Assert.True(offerSearchResult.flexibleDateCalendar.departureDetails[0].flexibleDatePriceCalendarCell.Count > 0, "No offers have been returned for the calender search");
        }

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        public async Task ReferenceMS_Test()
        {
            // calling reference countries get end point
            HttpResponseMessage countriesDetails = MicroserviceEndpoint.ReferenceMSGet("countries").EnsureIsSuccessStatusCode();

            dynamic countriesResult = JsonConvert.DeserializeObject<dynamic>(await countriesDetails.Content.ReadAsStringAsync());

            Assert.True(countriesResult.Count > 0, "No countries details returned");

            // calling reference airlinedetails get end point
            HttpResponseMessage airlineDetails = MicroserviceEndpoint.ReferenceMSGet("airlinedetails").EnsureIsSuccessStatusCode();

            dynamic airlineResult = JsonConvert.DeserializeObject<dynamic>(await airlineDetails.Content.ReadAsStringAsync());

            Assert.True(airlineResult.Count > 0, "No airline details returned");

            // calling reference aircraftdetails get end point
            HttpResponseMessage aircraftDetails = MicroserviceEndpoint.ReferenceMSGet("aircraftdetails").EnsureIsSuccessStatusCode();

            dynamic aircraftResult = JsonConvert.DeserializeObject<dynamic>(await aircraftDetails.Content.ReadAsStringAsync());

            Assert.True(aircraftResult.Count > 0, "No aircraft details returned");

            // calling reference airportdetail get end point
            HttpResponseMessage airportDetails = MicroserviceEndpoint.ReferenceMSGet("airportdetail").EnsureIsSuccessStatusCode();

            dynamic airportResult = JsonConvert.DeserializeObject<dynamic>(await airportDetails.Content.ReadAsStringAsync());

            Assert.True(airportResult.Count > 0, "No airport details returned");

            // calling reference cabindetails get end point
            HttpResponseMessage cabinDetails = MicroserviceEndpoint.ReferenceMSGet("cabindetails").EnsureIsSuccessStatusCode();

            dynamic cabinResult = JsonConvert.DeserializeObject<dynamic>(await cabinDetails.Content.ReadAsStringAsync());

            Assert.True(cabinResult.Count > 0, "No cabin details returned");

            // calling reference productidentifiers get end point
            HttpResponseMessage productIdentifiers = MicroserviceEndpoint.ReferenceMSGet("productidentifiers").EnsureIsSuccessStatusCode();

            dynamic productIdentifiersResult = JsonConvert.DeserializeObject<dynamic>(await productIdentifiers.Content.ReadAsStringAsync());

            Assert.True(productIdentifiersResult.Count > 0, "No product identifiers details returned");

            // generating reference sales banner request
            var referenceSalesBannersRequest = OfferSearchHelper.ReferenceSalesBannersRequest("search-top");

            // calling reference sales banner post end point
            HttpResponseMessage salesbanners = MicroserviceEndpoint.ReferenceMSPost(referenceSalesBannersRequest, "salesbanners").EnsureIsSuccessStatusCode();

            dynamic salesbannersResult = JsonConvert.DeserializeObject<dynamic>(await salesbanners.Content.ReadAsStringAsync());

            Assert.True(((JContainer)salesbannersResult).Count > 0, "No sales banners details returned");

            // calling reference refresh sales banner post end point
            HttpResponseMessage refreshsalesbanners = MicroserviceEndpoint.ReferenceMSPost("", "refreshsalesbanners").EnsureIsSuccessStatusCode();

            dynamic refreshsalesbannersResult = JsonConvert.DeserializeObject<dynamic>(await refreshsalesbanners.Content.ReadAsStringAsync());

            Assert.True(!string.IsNullOrEmpty(refreshsalesbannersResult), "No refresh for sales banners");
        }
    }
}
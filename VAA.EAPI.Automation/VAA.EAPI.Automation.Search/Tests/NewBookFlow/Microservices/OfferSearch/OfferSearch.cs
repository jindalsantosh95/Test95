using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class OfferSearch : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private bool requireUsableOfferForSignOff;

        public OfferSearch(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForDeltaAPIFromKeyVault("delta");
        }

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "DirectMS")]
        public void SharedServicesSignOff_OfferSearch_DirectContract() => OfferSearch_TC01_ConnectivityTest();

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("SharedServiceLayer", "DirectMS")]
        public void SharedServicesSignOff_OfferSearch_FlexDate()
        {
            requireUsableOfferForSignOff = true;
            OfferSearch_TC02_OneWay_FlexDateSearch(LocationType.LHR, LocationType.ANU, Currency.GBP, true);
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC01_ConnectivityTest()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //post offer search delta API
            HttpResponseMessage deltaOfferSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var softAssert = new SoftAssertHelper();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferSearchAssertions.ValidateDataList(deltaOfferSearchResponse, offerSearchResponse, out string dataList), dataList);

            softAssert.True(OfferSearchAssertions.ValidateFareComponent(deltaOfferSearchResponse, offerSearchResponse, out string fareComponent), fareComponent);

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.ANU, Currency.GBP, true)] // For Miles Search
        [InlineData(LocationType.ATL, LocationType.LHR, Currency.USD, false)] // For Currency Search
        public void OfferSearch_TC02_OneWay_FlexDateSearch(LocationType origin, LocationType destination, Currency currency, bool rewardFlag)
        {
            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 1, numberOfinfants: 0, flexiDateSearch: true, rewardFlag: rewardFlag);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferFlexiSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, $"OfferMS flex-date sign-off requires a successful response with usable calendar results. Response: {offerSearchResponse.ReadResponseAsString()}");
                return;
            }

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.flexibleDateCalendar.columnText.Count > 0, "No offers have been returned for the search details provided");

            Assert.True(OfferSearchAssertions.ValidateTotalTaxInLocalCurrency(offerSearchResponse, currency.ToString(), out string message), message);

            if (rewardFlag)
                Assert.True(OfferSearchAssertions.ValidateTotalPointsForAward(offerSearchResponse, out string totalPoints), totalPoints);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.ANU, Currency.GBP, true)] // For Miles Search
        [InlineData(LocationType.ATL, LocationType.LHR, Currency.USD, false)] // For Currency Search
        public void OfferSearch_TC03_OneWay_CalenderSearch(LocationType origin, LocationType destination, Currency currency, bool rewardFlag)
        {
            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 1, numberOfinfants: 0, calenderSearch: true, rewardFlag: rewardFlag);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferCalendarSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(OfferSearchAssertions.ValidateCalendarPriceAmountInLocalCurrency(offerSearchResponse, currency.ToString(), out string message), message);
        }

        [Fact]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        public void OfferSearch_TC04_Return_PriceDifference()
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

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            Assert.True(OfferSearchAssertions.ValidatePriceDifferenceInboundReturn(offerSearchResult, offerSearchReturnResult, out string priceDifference), priceDifference);
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", GlobalTraits.OfferMicroserviceCiCd)]
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
        [InlineData(LocationType.LHR, LocationType.JNB, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, 7, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.IAD, 1, 1, 1, 1)]
        [InlineData(LocationType.AUS, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.SEA, 1, 1, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.LHR, 2, 2, 0, 1)]
        [InlineData(LocationType.JFK, LocationType.TLV, 5, 1, 0, 1)]
        [InlineData(LocationType.IAD, LocationType.LOS, 4, 1, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.BOM, 1, 1, 1, 1)]
        [InlineData(LocationType.LAX, LocationType.LOS, 1, 0, 0, 0)]
        public void OfferSearch_TC05_VAFlights_OneWay(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var softAssert = new SoftAssertHelper();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            softAssert.True(OfferSearchAssertions.ValidatePayWithPointsFlag(offerSearchResponse, offerSearchRequest, out string payWithPointsFlag), payWithPointsFlag);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.MAN, LocationType.BGI, 2, 2, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.ANU, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.MLE, 1, 1, 0, 1)]
        [InlineData(LocationType.LHR, LocationType.PLS, 2, 1, 0, 1)]
        [InlineData(LocationType.LAS, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.EDI, LocationType.MCO, 4, 4, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.DXB, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.UVF, 3, 0, 0, 2)]
        [InlineData(LocationType.LHR, LocationType.NAS, 3, 1, 2, 0)]
        public void OfferSearch_TC06_InterlineAndCodeshare_OneWay(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-dd");

            // Generating offer search request
            var offerSearchCalendarRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, calenderSearch: true);

            // post offer search micro service
            HttpResponseMessage offerSearchCalendarResponse = MicroserviceEndpoint.OfferCalendarSearch(offerSearchCalendarRequest).EnsureIsSuccessStatusCode();

            var availableDeptartureDate = OfferSearchHelper.ReturnAvailableDepartureDate(offerSearchCalendarResponse);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, availableDeptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage deltaOfferSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, availableDeptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var softAssert = new SoftAssertHelper();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferSearchAssertions.ValidateDataList(deltaOfferSearchResponse, offerSearchResponse, out string dataList), dataList);

            softAssert.True(OfferSearchAssertions.ValidateFareComponent(deltaOfferSearchResponse, offerSearchResponse, out string fareComponent), fareComponent);

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LAX, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, 4, 4, 1, 0)]
        [InlineData(LocationType.MIA, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.IAD, LocationType.LHR, 2, 0, 2, 0)]
        [InlineData(LocationType.LHR, LocationType.BOS, 2, 2, 0, 1)]
        [InlineData(LocationType.TPA, LocationType.LHR, 2, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.TLV, 3, 1, 2, 0)]
        [InlineData(LocationType.MCO, LocationType.LHR, 3, 0, 0, 2)]
        [InlineData(LocationType.SEA, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.LOS, 2, 1, 1, 1)]
        [InlineData(LocationType.MCO, LocationType.MAN, 3, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.MAN, 4, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.MAN, 5, 0, 0, 1)]
        [InlineData(LocationType.LAX, LocationType.TLV, 5, 2, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.AUS, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.BGI, 2, 0, 2, 0)]
        [InlineData(LocationType.LHR, LocationType.SVD, 2, 2, 0, 1)]
        [InlineData(LocationType.MAN, LocationType.ATL, 2, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.GND, 3, 1, 2, 0)]
        [InlineData(LocationType.MAN, LocationType.LAS, 1, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.DEL, 1, 1, 0, 0)]
        [InlineData(LocationType.LAX, LocationType.DEL, 2, 2, 2, 1)]
        [InlineData(LocationType.ATL, LocationType.DEL, 3, 2, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.JNB, 3, 2, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.JNB, 3, 2, 1, 1)]
        [InlineData(LocationType.LAX, LocationType.JNB, 1, 1, 1, 1)]
        [InlineData(LocationType.MCO, LocationType.EDI, 4, 4, 0, 0)]
        public void OfferSearch_OneWay_TC07(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage deltaOfferSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            // Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var softAssert = new SoftAssertHelper();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferSearchAssertions.ValidateDataList(deltaOfferSearchResponse, offerSearchResponse, out string dataList), dataList);

            softAssert.True(OfferSearchAssertions.ValidateFareComponent(deltaOfferSearchResponse, offerSearchResponse, out string fareComponent), fareComponent);

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [Trait("Category", GlobalTraits.OfferMicroserviceCiCd)]
        [InlineData(LocationType.LHR, LocationType.ATL, 4, 4, 1, 0)]
        [InlineData(LocationType.MAN, LocationType.JFK, 4, 2, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.LAX, 1, 1, 1, 1)]
        [InlineData(LocationType.MAN, LocationType.BGI, 2, 2, 1, 0)]
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
        [InlineData(LocationType.BLR, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1)]
        public void OfferSearch_TC08_VAFlights_Return(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            Assert.True(OfferSearchAssertions.ValidatePayWithPointsFlag(offerSearchResponse, offerSearchRequest, out string payWithPointsFlag), payWithPointsFlag);

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            var softAssert = new SoftAssertHelper();

            softAssert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchReturnRequest, offerSearchReturnResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareId), returnFareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareSegments), returnFareSegments);

            softAssert.True(OfferSearchAssertions.ValidatePayWithPointsFlag(offerSearchReturnResponse, offerSearchReturnRequest, out string inboundPayWithPointsFlag), inboundPayWithPointsFlag);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.AUS, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.SVD, 2, 2, 0, 1)]
        [InlineData(LocationType.MAN, LocationType.ATL, 2, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.GND, 3, 1, 2, 0)]
        [InlineData(LocationType.MAN, LocationType.LAS, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.NAS, 3, 1, 2, 0)]
        [InlineData(LocationType.JFK, LocationType.DEL, 1, 1, 0, 0)]
        [InlineData(LocationType.LAX, LocationType.DEL, 2, 2, 2, 1)]
        [InlineData(LocationType.ATL, LocationType.DEL, 3, 2, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.JNB, 3, 2, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.JNB, 3, 2, 1, 1)]
        [InlineData(LocationType.LAX, LocationType.JNB, 1, 1, 1, 1)]
        [InlineData(LocationType.MCO, LocationType.EDI, 4, 4, 0, 0)]
        [InlineData(LocationType.LAX, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.JFK, LocationType.LHR, 4, 4, 1, 0)]
        [InlineData(LocationType.MIA, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.IAD, LocationType.LHR, 2, 0, 2, 0)]
        [InlineData(LocationType.LHR, LocationType.BOS, 2, 2, 0, 1)]
        [InlineData(LocationType.TPA, LocationType.LHR, 2, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.TLV, 3, 1, 2, 0)]
        [InlineData(LocationType.MCO, LocationType.LHR, 3, 0, 0, 2)]
        [InlineData(LocationType.SEA, LocationType.LHR, 2, 0, 0, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.LOS, 2, 1, 1, 1)]
        [InlineData(LocationType.MCO, LocationType.MAN, 3, 1, 1, 1)]
        [InlineData(LocationType.JFK, LocationType.MAN, 4, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.MAN, 5, 0, 0, 1)]
        [InlineData(LocationType.LAX, LocationType.TLV, 5, 2, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.SEA, 1, 1, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.LHR, 2, 2, 0, 1)]
        [InlineData(LocationType.JFK, LocationType.TLV, 5, 1, 0, 1)]
        [InlineData(LocationType.IAD, LocationType.LOS, 4, 1, 1, 1)]
        [InlineData(LocationType.SFO, LocationType.BOM, 1, 1, 1, 1)]
        [InlineData(LocationType.LAX, LocationType.LOS, 1, 0, 0, 0)]
        [InlineData(LocationType.LAS, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.EDI, LocationType.MCO, 4, 4, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 7, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.IAD, 1, 1, 1, 1)]
        [InlineData(LocationType.AUS, LocationType.LHR, 2, 0, 0, 0)]
        public void OfferSearch_Return_TC09(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage deltaOfferSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            var softAssert = new SoftAssertHelper();

            softAssert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            softAssert.True(OfferSearchAssertions.ValidateDataList(deltaOfferSearchResponse, offerSearchResponse, out string dataList), dataList);

            softAssert.True(OfferSearchAssertions.ValidateFareComponent(deltaOfferSearchResponse, offerSearchResponse, out string fareComponent), fareComponent);

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchReturnRequest, offerSearchReturnResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareId), returnFareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareSegments), returnFareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.BGI, 2, 0, 2, 0)]
        [InlineData(LocationType.LHR, LocationType.UVF, 3, 0, 0, 2)]
        [InlineData(LocationType.LHR, LocationType.DXB, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.ANU, 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.MLE, 1, 1, 0, 1)]
        [InlineData(LocationType.LHR, LocationType.PLS, 2, 1, 0, 1)]
        public void OfferSearch_TC10_Return(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var currentDate = OfferSearchHelper.DepartureDate();

            // Generating offer search request
            var offerSearchCalendarRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, currentDate, locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, calenderSearch: true);

            // post offer search micro service
            HttpResponseMessage offerSearchCalendarResponse = MicroserviceEndpoint.OfferCalendarSearch(offerSearchCalendarRequest).EnsureIsSuccessStatusCode();

            var availableDeptartureDate = OfferSearchHelper.ReturnAvailableDepartureDate(offerSearchCalendarResponse);

            var returnDate = OfferSearchHelper.ReturnDate(availableDeptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, availableDeptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage deltaOfferSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, availableDeptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, availableDeptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            var softAssert = new SoftAssertHelper();

            softAssert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            softAssert.True(OfferSearchAssertions.ValidateDataList(deltaOfferSearchResponse, offerSearchResponse, out string dataList), dataList);

            softAssert.True(OfferSearchAssertions.ValidateFareComponent(deltaOfferSearchResponse, offerSearchResponse, out string fareComponent), fareComponent);

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchReturnRequest, offerSearchReturnResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareId), returnFareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareSegments), returnFareSegments);

            softAssert.AssertAll();
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
        public void OfferSearchRewards_OneWay_TC11(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, OfferSearchHelper.DepartureDate(), locationOne: origin, locationTwo: destination, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var softAssert = new SoftAssertHelper();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.OfferMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 4, 4, 1, 0)]
        [InlineData(LocationType.MAN, LocationType.JFK, 4, 2, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.LAX, 1, 1, 1, 1)]
        [InlineData(LocationType.MAN, LocationType.BGI, 2, 2, 1, 0)]
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
        [InlineData(LocationType.BLR, LocationType.LHR, 1, 1, 1, 1)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1)]
        public void OfferSearchRewards_Return_TC12(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            Assert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchResponse, offerSearchRequest, out string fareId), fareId);

            Assert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchResponse, offerSearchRequest, out string fareSegments), fareSegments);

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, rewardFlag: true, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            var softAssert = new SoftAssertHelper();

            softAssert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            softAssert.True(OfferSearchAssertions.ValidateOriginAndDestination(offerSearchReturnRequest, offerSearchReturnResponse, out string originDestination), originDestination);

            softAssert.True(OfferSearchAssertions.ValidatePaxList(offerSearchRequest, offerSearchResponse, out string paxList), paxList);

            softAssert.True(OfferSearchAssertions.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out string journeyPaxList), journeyPaxList);

            softAssert.True(OfferSearchAssertions.ValidateOfferExpirationTTL(offerSearchResponse, out string offerExpiration), offerExpiration);

            softAssert.True(OfferSearchAssertions.ValidateFareIdExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareId), returnFareId);

            softAssert.True(OfferSearchAssertions.ValidateFareSegmentsExists(offerSearchReturnResponse, offerSearchReturnRequest, out string returnFareSegments), returnFareSegments);

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 2)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 3)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 4)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 5)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 6)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 7)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 8)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 9)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, 10)]
        public void Performance(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, int runNo)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            var offerId = offerIds[0];

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");
        }

        [Theory]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData(LocationType.LHR, LocationType.ATL, 4, 4, 1, 0, 1)]
        [InlineData(LocationType.MAN, LocationType.JFK, 4, 2, 1, 0, 2)]
        [InlineData(LocationType.LHR, LocationType.LAX, 1, 1, 1, 1, 3)]
        [InlineData(LocationType.MAN, LocationType.BGI, 2, 2, 1, 0, 4)]
        [InlineData(LocationType.LHR, LocationType.LOS, 3, 2, 1, 0, 5)]
        [InlineData(LocationType.LHR, LocationType.BOM, 1, 1, 1, 0, 6)]
        [InlineData(LocationType.LHR, LocationType.DEL, 4, 4, 1, 0, 7)]
        [InlineData(LocationType.LHR, LocationType.MCO, 1, 1, 1, 1, 8)]
        [InlineData(LocationType.MAN, LocationType.MCO, 4, 3, 1, 1, 9)]
        [InlineData(LocationType.LHR, LocationType.MIA, 4, 4, 1, 0, 10)]
        [InlineData(LocationType.LHR, LocationType.SFO, 2, 2, 1, 1, 11)]
        [InlineData(LocationType.LHR, LocationType.LAS, 1, 1, 1, 1, 12)]
        [InlineData(LocationType.LHR, LocationType.MBJ, 3, 2, 1, 1, 13)]
        [InlineData(LocationType.LHR, LocationType.TPA, 1, 1, 1, 1, 14)]
        [InlineData(LocationType.LHR, LocationType.PVG, 1, 1, 0, 0, 15)]
        [InlineData(LocationType.BOS, LocationType.LHR, 1, 0, 0, 0, 16)]
        [InlineData(LocationType.LHR, LocationType.CPT, 5, 1, 1, 1, 17)]
        [InlineData(LocationType.DEL, LocationType.LHR, 5, 1, 1, 1, 18)]
        [InlineData(LocationType.BLR, LocationType.LHR, 1, 1, 1, 1, 19)]
        [InlineData(LocationType.BOM, LocationType.LHR, 1, 1, 1, 1, 20)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1, 21)]
        public void Performance_RoutesAndPTC(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, int runNo)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate(); //"2024-07-15T00:00:00";
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);  //"2024-07-20T00:00:00";

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            Debug.WriteLine($"Return Outbound-{runNo} = {origin}-{destination} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants}");

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            var offerId = offerIds[0];

            Debug.WriteLine($"{((JContainer)offerSearchResult.SelectToken("$.offers[0]")).Last}");

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerId, currentTripIndexId: "1");

            Debug.WriteLine($"Return Inbound-{runNo} = {destination}-{origin} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants}");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");
        }
    }
}

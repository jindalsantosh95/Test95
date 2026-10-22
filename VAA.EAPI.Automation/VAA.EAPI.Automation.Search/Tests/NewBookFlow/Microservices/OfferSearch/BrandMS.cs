using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Constants;
using MicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class BrandMS
    {
        private bool requireUsableOfferForSignOff;

        [Fact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "BrandMS")]
        [Trait("SharedServiceConsumer", "Search")]
        [Trait("TestType", "Contract")]
        public void SharedServicesSignOff_Brand_DirectContract()
        {
            requireUsableOfferForSignOff = true;
            BrandMSTest_OneWay(LocationType.LHR, LocationType.JFK);
        }

        [Theory]
        [Trait("Category", Traits.BrandMicroserviceRegression)]
        [Trait("Category", Traits.BrandMicroserviceCiCd)]
        [InlineData(LocationType.DUB, LocationType.MIA)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        public void BrandMSTest_OneWay(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            // Exit early if the response is unprocessable (status code 422)
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse))
            {
                Assert.False(requireUsableOfferForSignOff, "BrandMS contract sign-off requires OfferSearch to return a usable offer before invoking BrandMS.");
                return;
            }

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            // Generating brand ms request
            var brandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage brandMSResponse = MicroserviceEndpoint.BrandMS(brandMSRequest).EnsureIsSuccessStatusCode();

            dynamic brandMSResult = JsonConvert.DeserializeObject<dynamic>(brandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandMSResult.itineraries.Count > 0, "No itineraries exists");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandMSResponse, brandMSRequest, "OneWay-Outbound", out string baggageInfo), baggageInfo);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerIds[0]);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            // Generating brand ms request
            var brandSummaryRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerPriceResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage brandSummaryResponse = MicroserviceEndpoint.BrandMS(brandSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic brandSummaryResult = JsonConvert.DeserializeObject<dynamic>(brandSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandSummaryResult.itineraries.Count > 0, "No itineraries exists");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandSummaryResponse, brandSummaryRequest, "OneWay-Summary", out string summaryBaggageInfo), summaryBaggageInfo);
        }

        [Theory]
        [Trait("Category", Traits.BrandMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        public void BrandMSTest_Return(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            // Exit early if the response is unprocessable (status code 422)
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            // Generating brand ms request
            var brandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage brandMSResponse = MicroserviceEndpoint.BrandMS(brandMSRequest).EnsureIsSuccessStatusCode();

            dynamic brandMSResult = JsonConvert.DeserializeObject<dynamic>(brandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandMSResult.itineraries.Count > 0, "No itineraries exists for outbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandMSResponse, brandMSRequest, "Return-Outbound", out string baggageInfo), baggageInfo);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerIds[0], currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            var inboundBrandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchReturnResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage inboundBrandMSResponse = MicroserviceEndpoint.BrandMS(inboundBrandMSRequest).EnsureIsSuccessStatusCode();

            dynamic inboundBrandMSResult = JsonConvert.DeserializeObject<dynamic>(inboundBrandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(inboundBrandMSResult.itineraries.Count > 0, "No itineraries exists for Inbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(inboundBrandMSResponse, inboundBrandMSRequest, "Return-Inbound", out string inboundBaggageInfo), inboundBaggageInfo);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: returnOfferIds[0]);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryOfferId = offerPriceResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(summaryOfferId.Any(), "No offers have been returned for the offer id provided");

            var summaryBrandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerPriceResponse);

            // Send BrandMS request to microservice
            HttpResponseMessage summaryBrandMSResponse = MicroserviceEndpoint.BrandMS(summaryBrandMSRequest).EnsureIsSuccessStatusCode();

            dynamic summaryBrandMSResult = JsonConvert.DeserializeObject<dynamic>(summaryBrandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryBrandMSResult.itineraries.Count > 0, "No itineraries exists for Inbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(summaryBrandMSResponse, summaryBrandMSRequest, "Return-Summary", out string summaryBaggageInfo), summaryBaggageInfo);
        }

        [Theory]
        [Trait("Category", Traits.BrandMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        public void BrandMSRewardTest_OneWay(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            // Exit early if the response is unprocessable (status code 422)
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            // Generating brand ms request
            var brandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchResponse, rewardFlag: true);

            // Send BrandMS request to microservice
            HttpResponseMessage brandMSResponse = MicroserviceEndpoint.BrandMS(brandMSRequest).EnsureIsSuccessStatusCode();

            dynamic brandMSResult = JsonConvert.DeserializeObject<dynamic>(brandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandMSResult.itineraries.Count > 0, "No itineraries exists");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandMSResponse, brandMSRequest, "OneWay_reward-Outbound", out string baggageInfo), baggageInfo);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerIds[0], rewardFlag: true);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            // Generating brand ms request
            var brandSummaryRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerPriceResponse, rewardFlag: true);

            // Send BrandMS request to microservice
            HttpResponseMessage brandSummaryResponse = MicroserviceEndpoint.BrandMS(brandSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic brandSummaryResult = JsonConvert.DeserializeObject<dynamic>(brandSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandSummaryResult.itineraries.Count > 0, "No itineraries exists");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandSummaryResponse, brandSummaryRequest, "OneWay_reward-Summary", out string summaryBaggageInfo), summaryBaggageInfo);
        }

        [Theory]
        [Trait("Category", Traits.BrandMicroserviceRegression)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.TLV)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LHR, LocationType.SEA)]
        [InlineData(LocationType.LHR, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.IAD)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.ANU)]
        [InlineData(LocationType.LHR, LocationType.AUS)]
        [InlineData(LocationType.LHR, LocationType.SVD)]
        [InlineData(LocationType.MAN, LocationType.ATL)]
        [InlineData(LocationType.LHR, LocationType.GND)]
        [InlineData(LocationType.LHR, LocationType.UVF)]
        [InlineData(LocationType.LHR, LocationType.MLE)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.NAS)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.PLS)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.AUS, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.ATL, LocationType.MAN)]
        [InlineData(LocationType.IAD, LocationType.LOS)]
        [InlineData(LocationType.ATL, LocationType.DEL)]
        public void BrandMSRewardTest_Return(LocationType origin, LocationType destination)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating offer search request
            var offerSearchRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, rewardFlag: true);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);

            // Exit early if the response is unprocessable (status code 422)
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var offerIds = offerSearchResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(offerIds.Any(), "Count of offers returned was not greater than 0");

            // Generating brand ms request
            var brandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchResponse, rewardFlag: true);

            // Send BrandMS request to microservice
            HttpResponseMessage brandMSResponse = MicroserviceEndpoint.BrandMS(brandMSRequest).EnsureIsSuccessStatusCode();

            dynamic brandMSResult = JsonConvert.DeserializeObject<dynamic>(brandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(brandMSResult.itineraries.Count > 0, "No itineraries exists for outbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(brandMSResponse, brandMSRequest, "Return_reward-Outbound", out string baggageInfo), baggageInfo);

            //Generating offer search request for return
            var offerSearchReturnRequest = MicroserviceRequestBuilder.OfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, offerId: offerIds[0], rewardFlag: true, currentTripIndexId: "1");

            // post offer search micro service
            HttpResponseMessage offerSearchReturnResponse = MicroserviceEndpoint.OfferSearch(offerSearchReturnRequest).EnsureIsSuccessStatusCode();

            JToken offerSearchReturnResult = JsonConvert.DeserializeObject<JToken>(offerSearchReturnResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Return all oferIds in the response
            var returnOfferIds = offerSearchReturnResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(returnOfferIds.Any(), "Count of offers returned was not greater than 0");

            var inboundBrandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerSearchReturnResponse, rewardFlag: true);

            // Send BrandMS request to microservice
            HttpResponseMessage inboundBrandMSResponse = MicroserviceEndpoint.BrandMS(inboundBrandMSRequest).EnsureIsSuccessStatusCode();

            dynamic inboundBrandMSResult = JsonConvert.DeserializeObject<dynamic>(inboundBrandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(inboundBrandMSResult.itineraries.Count > 0, "No itineraries exists for Inbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(inboundBrandMSResponse, inboundBrandMSRequest, "Return_reward-Inbound", out string inboundBaggageInfo), inboundBaggageInfo);

            //Generating price request
            var priceRequest = MicroserviceRequestBuilder.PriceRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, rewardFlag: true, offerId: returnOfferIds[0]);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryOfferId = offerPriceResult.SelectTokens("$..offerId").Select(x => x.ToString()).ToArray();

            Assert.True(summaryOfferId.Any(), "No offers have been returned for the offer id provided");

            var summaryBrandMSRequest = MicroserviceRequestBuilder.CreateBrandMSRequest(offerPriceResponse, rewardFlag: true);

            // Send BrandMS request to microservice
            HttpResponseMessage summaryBrandMSResponse = MicroserviceEndpoint.BrandMS(summaryBrandMSRequest).EnsureIsSuccessStatusCode();

            dynamic summaryBrandMSResult = JsonConvert.DeserializeObject<dynamic>(summaryBrandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(summaryBrandMSResult.itineraries.Count > 0, "No itineraries exists for Inbound");

            Assert.True(OfferSearchAssertions.ValidateBaggageExists(summaryBrandMSResponse, summaryBrandMSRequest, "Return_reward-Summary", out string summaryBaggageInfo), summaryBaggageInfo);
        }
    }
}

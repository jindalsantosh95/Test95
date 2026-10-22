using Newtonsoft.Json;
using System.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using DeltaOfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.DeltaOfferSearchHelper;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Delta
{
    public class DeltaOfferGraphql : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public DeltaOfferGraphql(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForDeltaAwsAPIFromKeyVault("awsDelta");
        }

        [Theory]
        [Trait("Category", "Local-Run")]
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
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1)]
        public void DeltaOfferGraphql_OneWay(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"One Way = {origin}-{destination} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {sizeInBytes}");

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.data.searchOffers.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        [Theory]
        [Trait("Category", "Local-Run")]
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
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1)]
        public void DeltaOfferGraphql_Return(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"Return Outbound = {origin}-{destination} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {sizeInBytes}");

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");

            string inboundOffer = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerInboundResponse)[0];

            var inboundResponsePayload = offerInboundResponse.ReadResponseAsString();

            var inboundSizeInBytes = inboundResponsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"Return Inbound = {destination}-{origin} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {inboundSizeInBytes}");

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: inboundOffer, currentTripIndexId: "2");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.data.searchOffers.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        [Theory]
        [Trait("Category", "Local-Run")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void Performance(int runNo)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferGraphqlRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.DeltaOfferGraphql(tokenFixture.GetToken("awsDelta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");
        }
    }
}

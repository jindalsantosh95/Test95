using Newtonsoft.Json;
using System.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using DeltaOfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.DeltaOfferSearchHelper;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;


namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Delta
{
    public class OfferSearch : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public OfferSearch(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForDeltaAPIFromKeyVault("delta");
            this.tokenFixture.AddTokenForDeltaAwsAPIFromKeyVault("awsDelta");
        }

        [Fact]
        [Trait("Category", "Local-Run")]
        public void DeltaAWSOfferAPI_OneWay_ConnectivityTest()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        [Fact]
        [Trait("Category", "Local-Run")]
        public void DeltaAWSOfferAPI_Return_ConnectivityTest()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");

            string inboundOffer = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerInboundResponse)[0];

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: inboundOffer, currentTripIndexId: "2");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        [Fact]
        [Trait("Category", "Local-Run")]
        public void DeltaOnPremiseOfferAPI_OneWay_ConnectivityTest()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        [Fact]
        [Trait("Category", "Local-Run")]
        public void DeltaOnPremiseOfferAPI_RoundTrip_ConnectivityTest()
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");

            string inboundOffer = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerInboundResponse)[0];

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: inboundOffer, currentTripIndexId: "2");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
        }

        //Test not to be configure to be ran in azure pipelines
        [Theory]
        [Trait("Category", "Local-Run")]
        [InlineData(LocationType.LHR, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.LOS)]
        [InlineData(LocationType.LHR, LocationType.MCO)]
        [InlineData(LocationType.LAX, LocationType.LHR)]
        [InlineData(LocationType.MAN, LocationType.MCO)]
        [InlineData(LocationType.JFK, LocationType.LHR)]
        [InlineData(LocationType.LHR, LocationType.LAX)]
        [InlineData(LocationType.LHR, LocationType.MIA)]
        [InlineData(LocationType.SFO, LocationType.LHR)]
        [InlineData(LocationType.MAN, LocationType.JFK)]
        [InlineData(LocationType.LHR, LocationType.BGI)]
        [InlineData(LocationType.LHR, LocationType.MBJ)]
        [InlineData(LocationType.LHR, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.DEL)]
        [InlineData(LocationType.LHR, LocationType.SFO)]
        [InlineData(LocationType.LON, LocationType.JFK)]
        [InlineData(LocationType.LAX, LocationType.LON)]
        [InlineData(LocationType.LON, LocationType.NYC)]
        [InlineData(LocationType.LHR, LocationType.TPA)]
        [InlineData(LocationType.MIA, LocationType.LHR)]
        [InlineData(LocationType.NYC, LocationType.LON)]
        [InlineData(LocationType.LHR, LocationType.BOM)]
        [InlineData(LocationType.LON, LocationType.MCO)]
        [InlineData(LocationType.BOS, LocationType.LHR)]
        [InlineData(LocationType.LHR, LocationType.DXB)]
        [InlineData(LocationType.LHR, LocationType.NYC)]
        [InlineData(LocationType.MCO, LocationType.LHR)]
        [InlineData(LocationType.IAD, LocationType.LHR)]
        [InlineData(LocationType.LON, LocationType.LOS)]
        [InlineData(LocationType.SEA, LocationType.LHR)]
        [InlineData(LocationType.EDI, LocationType.MCO)]
        [InlineData(LocationType.MAN, LocationType.LAS)]
        [InlineData(LocationType.LHR, LocationType.BOS)]
        [InlineData(LocationType.LHR, LocationType.CPT)]
        [InlineData(LocationType.JFK, LocationType.LON)]
        [InlineData(LocationType.LON, LocationType.LAX)]
        [InlineData(LocationType.LHR, LocationType.JNB)]
        [InlineData(LocationType.TPA, LocationType.LHR)]
        [InlineData(LocationType.MAN, LocationType.BGI)]
        [InlineData(LocationType.SFO, LocationType.LON)]
        [InlineData(LocationType.LON, LocationType.MIA)]
        [InlineData(LocationType.LOS, LocationType.LHR)]
        [InlineData(LocationType.LON, LocationType.BGI)]
        [InlineData(LocationType.ATL, LocationType.LHR)]
        [InlineData(LocationType.LHR, LocationType.PVG)]
        [InlineData(LocationType.LON, LocationType.MBJ)]
        [InlineData(LocationType.BOS, LocationType.LON)]
        [InlineData(LocationType.LON, LocationType.LAS)]
        [InlineData(LocationType.MIA, LocationType.LON)]

        public void DeltaSearchesToReturnResponseBytes(LocationType origin, LocationType destination)
        {
            var departureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, departureDate, OfferSearchHelper.ReturnDate(departureDate), locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta on-premise API
            //HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.OfferSearch(tokenFixture.GetToken("delta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            //post offer search delta AWS API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            int deltaOffersCounts = DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse);

            Assert.True(deltaOffersCounts > 0, "Offers returned were not greater than 0");

            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Output in debug window
            Debug.WriteLine($"Size in bytes = {sizeInBytes}");

            //Debug.WriteLine($"Size in bytes = {sizeInBytes} \nResponse payload = {responsePayload}");

            //Convert to megabytes
            //long bytes = sizeInBytes;

            //double megabytes = (double)bytes / (1024 * 1024);
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
        public void DeltaAWSOfferAPI_OneWay(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"One Way = {origin}-{destination} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {sizeInBytes}");

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.OneWay, deptartureDate, locationOne: origin, locationTwo: destination, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
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
        public void DeltaAWSOfferAPI_Return(LocationType origin, LocationType destination, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            var deptartureDate = OfferSearchHelper.DepartureDate();
            var returnDate = OfferSearchHelper.ReturnDate(deptartureDate);

            //Generating delta offer search request
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            var responsePayload = offerSearchResponse.ReadResponseAsString();

            var sizeInBytes = responsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"Return Outbound = {origin}-{destination} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {sizeInBytes}");

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");

            string inboundOffer = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerInboundResponse)[0];

            var inboundResponsePayload = offerInboundResponse.ReadResponseAsString();

            var inboundSizeInBytes = inboundResponsePayload.Length;

            //Output in debug window          
            Debug.WriteLine($"Return Inbound = {destination}-{origin} PTC Counts = numberOfadults: {numberOfadults} numberOfchildren: {numberOfchildren} numberOfyouths: {numberOfyouths} numberOfinfants: {numberOfinfants} Size in bytes = {inboundSizeInBytes}");

            //Generating delta offer summary request
            var deltaOfferSummaryRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, offerId: inboundOffer, currentTripIndexId: "2");

            //post offer search delta API
            HttpResponseMessage offerSummaryResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSummaryRequest).EnsureIsSuccessStatusCode();

            dynamic offerSummaryResult = JsonConvert.DeserializeObject<dynamic>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSummaryResult.selectedOfferSets.Count > 0, "Offer returned were not greater than 0");
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
            var deltaOfferSearchRequest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.LOS, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0);

            //post offer search delta API
            HttpResponseMessage offerSearchResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferSearchRequest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerSearchResponse) > 0, "Offer returned were not greater than 0");

            string offerid = OfferSearchHelper.ReturnDeltaNotNullOffersIds(offerSearchResponse)[0];

            //Generating delta inbound request
            var deltaOfferInboundReuest = Core.Delta.Factory.OfferOrder.DeltaOfferSearchRequest(JourneyType.Return, deptartureDate, returnDate, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.LOS, locationFour: LocationType.LHR, cabinTypeCode: "4", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, offerId: offerid, currentTripIndexId: "1");

            //post offer search delta API
            HttpResponseMessage offerInboundResponse = Core.Delta.Endpoint.OfferOrder.DeltaAwsOfferSearch(tokenFixture.GetToken("awsDelta"), deltaOfferInboundReuest).EnsureIsSuccessStatusCode();

            Assert.True(DeltaOfferSearchHelper.DeltaOffersReturnedCount(offerInboundResponse) > 0, "Offer returned were not greater than 0");
        }
    }
}
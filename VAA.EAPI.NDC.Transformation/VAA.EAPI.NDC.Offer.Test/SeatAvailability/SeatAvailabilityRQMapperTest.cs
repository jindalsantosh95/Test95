using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using Google.Protobuf.WellKnownTypes;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class SeatAvailabilityRQMapperTest
    {
        [Fact]
        public void Should_Map_SeatAvailability_MS_Request_using_OfferDoc()
        {
            var request = TestDataProvider.GetTestData<SeatAvailabilityQuery>("NDCOffer\\SeatAvailability\\SeatAvailabilityRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            IEnumerable<FlightSegment> flightSegments = Enumerable.Empty<FlightSegment>();

            var seatMSRQ = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestOfferDoc(request, offerDoc, true, flightSegments);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("GBP", seatMSRQ.Context.CurrencyCode);
            Assert.Equal("91281035", seatMSRQ.Parties.Participants.First().AgencyID);

        }

        [Fact]
        public void Should_Map_SeatAvailability_MS_Request_when_OfferDoc_is_null()
        {
            var request = TestDataProvider.GetTestData<SeatAvailabilityQuery>("NDCOffer\\SeatAvailability\\SeatAvailabilityRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");

            var seatMSRQ = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestOfferDoc(request, offerDoc, false, null);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("GBP", seatMSRQ.Context.CurrencyCode);
            Assert.Equal("91281035", seatMSRQ.Parties.Participants.First().AgencyID);

        }

        [Fact]
        public void Should_Map_SeatAvailability_MS_Request_using_OrderDoc()
        {
            var request = TestDataProvider.GetTestData<SeatAvailabilityQuery>("NDCOffer\\SeatAvailability\\SeatAvailabilityOrderDoc");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            
            IEnumerable<Segment> flightSegments = Enumerable.Empty<Segment>();

            var seatMSRQ = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestOrderDoc(request, orderDoc, true, flightSegments);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("GBP", seatMSRQ.Context.CurrencyCode);          
            Assert.Equal("91281035", seatMSRQ.Parties.Participants.First().AgencyID);
         

        }


        [Fact]
        public void Should_Map_Flight_Segment_in_SeatAvailability_MS_Request()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");           
            var seatMSRQ = SeatAvailabilityRqMapper.GetFlightSegments(orderDoc.DataList.SegmentDetails);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("TLV", seatMSRQ.First().DestinationAirportCode);
            Assert.Equal("01", seatMSRQ.First().SegmentID);
            Assert.Equal("VS", seatMSRQ.First().MarketingCarrier);
            Assert.Equal("453", seatMSRQ.First().MarketingFlightNumber);

        }

        [Fact]
        public void Should_Map_Parties_in_SeatAvailability_MS_Request()
        {
            var request = TestDataProvider.GetTestData<SeatAvailabilityQuery>("NDCOffer\\SeatAvailability\\SeatAvailabilityOrderDoc");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");

            var seatMSRQ = SeatAvailabilityRqMapper.GetParties(request.SeatAvailabilityRQ.DistributionChain, offerDoc.Parties.CorporateAccount, true);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("91281035", seatMSRQ.Participants.First().AgencyID);

        }

        [Fact]
        public void Should_Map_GDS_SeatAvailability_MS_Request_using_OfferDoc()
        {
            var request = TestDataProvider.GetTestData<SeatAvailabilityQuery>("NDCOffer\\SeatAvailability\\SeatAvailabilityRQ");
            Air4OrderSearchMSResponse orderDoc = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");

            IEnumerable<Segment> flightSegments = new Segment[] { new() { OperatingCarrier = "DL" } };
            
            var seatMSRQ = SeatAvailabilityRqMapper.MapSeatAvailabilityMSRequestGDS(request, orderDoc, flightSegments, true);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("USD", seatMSRQ.Context.CurrencyCode);
            Assert.Equal("91281035", seatMSRQ.Parties.Participants.First().AgencyID);

        }

        [Fact]
        public void Should_Map_Segment_In_GDS_SeatAvailability_MS_Request_using_OfferDoc()
        {
            Air4OrderSearchMSResponse orderDoc = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            
            var seatMSRQ = SeatAvailabilityRqMapper.GetGDSFlightSegments(orderDoc, orderDoc.OrderSearchResponse.SegmentDetails);

            Assert.NotNull(seatMSRQ);
            Assert.Equal("LHR", seatMSRQ.First().DestinationAirportCode);
            Assert.Equal("01", seatMSRQ.First().SegmentID);
            Assert.Equal("VS", seatMSRQ.First().MarketingCarrier);
            Assert.Equal("0454",seatMSRQ.First().MarketingFlightNumber);
            Assert.Equal("Economy Classic", seatMSRQ.First().FareFamily);
            Assert.Equal("02", seatMSRQ.Last().SegmentID);
            Assert.Equal("LHR", seatMSRQ.Last().OriginAirportCode);
            Assert.Equal("Economy Classic", seatMSRQ.Last().FareFamily);

        }
    }
}

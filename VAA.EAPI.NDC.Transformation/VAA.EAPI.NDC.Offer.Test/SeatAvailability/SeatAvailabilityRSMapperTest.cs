using Gremlin.Net.Process.Traversal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Mapper.SeatAvailability;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class SeatAvailabilityRSMapperTest
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;
       
        public SeatAvailabilityRSMapperTest()
        {
            _hostEnvironment = new Mock<IHostEnvironment>();
        }

        [Fact]
        public void Should_map_NDC_Seat_response_with_Seat_MS_Response_with_OfferDoc()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            OrderDocument orderDocument = new();
            var paxList = offerDoc.DataList?.CustomerDetails;
            var seatMSRS = SeatAvailabilityRSMapper.MapSeatAvailabilityResponse(seatMsResponse, paxList, orderDocument, offerDoc, _hostEnvironment.Object);
            var result = (SeatAvailResponseType)seatMSRS.Items[0];
            
            Assert.NotNull(seatMSRS);
            Assert.Equal("DOS1", result.DataLists.DatedMarketingSegmentList[0].DatedOperatingSegmentRefId);
            Assert.Equal("ADT", result.DataLists.PaxList[0].PTC);

        }


        [Fact]
        public void Should_map_NDC_Seat_response_with_Seat_MS_Response_with_OrderDoc()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            OfferResponse offerDoc = new OfferResponse();
            var paxList = orderDoc.DataList.CustomerDetails;
            var seatMSRS = SeatAvailabilityRSMapper.MapSeatAvailabilityResponse(seatMsResponse, paxList, orderDoc, offerDoc, _hostEnvironment.Object);
            var result = (SeatAvailResponseType)seatMSRS.Items[0];

            Assert.NotNull(seatMSRS);
            Assert.True(seatMSRS.Items.Count() == 1);
            Assert.Equal("ADT", result.DataLists.PaxList[0].PTC);
        }


        [Fact]
        public void Should_map_Flight_segment_with_Seat_MS_Response_using_OfferDoc()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            var seatMSRS = SeatAvailabilityRSMapper.GetFlightSegments(offerDoc);

            Assert.NotNull(seatMSRS);
            Assert.Equal("K", seatMSRS.First().ClassOfService);
            Assert.Equal("VS", seatMSRS.First().MarketingCarrier);
            Assert.Equal("JFK", seatMSRS.First().DestinationAirportCode);

        }

        [Fact]
        public void Should_map_SeatRow_Toilet_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetColumnIDForToilet(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "AC|DEFG|HK");
            
            Assert.NotNull(seatMSRS);
            Assert.Equal("A", seatMSRS[0]);
        }

        [Fact]
        public void Should_map_SeatRow_Galley_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetColumnIDForGalley(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "AC|DEFG|HK");

            Assert.NotNull(seatMSRS);
            Assert.Equal("E", seatMSRS[1].ToUpper());
            
        }

        [Fact]
        public void Should_map_SeatRow_Closet_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetColumnIDForCloset(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "A|G|K");

            Assert.NotNull(seatMSRS);
            Assert.Equal("|", seatMSRS[0].ToUpper().ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_RowNumber_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetRowNumber(flightSeatMap.Seatmap.First().SeatRows, 1);

            Assert.NotNull(seatMSRS);
            Assert.Equal("1", seatMSRS[0].ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_PositionCode_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetPositionCode(flightSeatMap.Seatmap.First().SeatRows, 1,1);

            Assert.NotNull(seatMSRS);
            Assert.Equal("Before", seatMSRS.ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_RowCharacteristics_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = SeatAvailabilityRSMapper.GetRowCharacteristics(flightSeatMap.Seatmap.First().SeatRows.First());

            Assert.NotNull(seatMSRS);
            Assert.Equal("X", seatMSRS[0]);
        }
    }

}

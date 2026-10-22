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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Mapper.SeatAvailability;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class GdsSeatAvailabilityRSMapperTest
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;
       
        public GdsSeatAvailabilityRSMapperTest()
        {
            _hostEnvironment = new Mock<IHostEnvironment>();
        }

        [Fact]
        public void Should_map_NDC_Seat_response_with_GDS_Seat_MS_Response()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            OrderSearchResponse orderSearchResp = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOffer\\SeatAvailability\\OrderSearch");
            var seatMsResponse = TestDataProvider.GetTestData<SeatMSAvailabilityResponse>("NDCOffer\\SeatAvailability\\SeatMSResponse");
            IEnumerable<NDCWarning> warningList= Enumerable.Empty<NDCWarning>();
            var paxList = new List<Domain.RequestResponseModels.SeatAvailabilityRS.PaxType>();
            var seatMSRS = GDSSeatAvailabilityRSMapper.MapSeatAvailaibilityResponse(seatMsResponse, orderSearchResp,paxList,warningList, _hostEnvironment.Object);
            var result = (SeatAvailResponseType)seatMSRS.Items[0];
            
            Assert.NotNull(seatMSRS);
            Assert.Equal("DOS1", result.DataLists.DatedMarketingSegmentList[0].DatedOperatingSegmentRefId);
            Assert.Equal("SEG1", result.DataLists.PaxSegmentList[0].PaxSegmentID);
            Assert.Equal("0454", result.DataLists.DatedOperatingSegmentList[0].OperatingCarrierFlightNumberText);

        }       
               
        [Fact]
        public void Should_map_SeatRow_Toilet_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetColumnIDForToilet(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "AC|DEFG|HK");
            
            Assert.NotNull(seatMSRS);
            Assert.Equal("A", seatMSRS[0]);
        }

        [Fact]
        public void Should_map_SeatRow_Galley_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetColumnIDForGalley(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "AC|DEFG|HK");

            Assert.NotNull(seatMSRS);
            Assert.Equal("E", seatMSRS[1].ToUpper());
            
        }

        [Fact]
        public void Should_map_SeatRow_Closet_with_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetColumnIDForCloset(flightSeatMap.Seatmap.First().SeatRows.First().SeatColumns, "A|G|K");

            Assert.NotNull(seatMSRS);
            Assert.Equal("|", seatMSRS[0].ToUpper().ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_RowNumber_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetRowNumber(flightSeatMap.Seatmap.First().SeatRows, 1);

            Assert.NotNull(seatMSRS);
            Assert.Equal("1", seatMSRS[0].ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_PositionCode_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetPositionCode(flightSeatMap.Seatmap.First().SeatRows, 1,1);

            Assert.NotNull(seatMSRS);
            Assert.Equal("Before", seatMSRS.ToString());
        }

        [Fact]
        public void Should_map_SeatRow_With_RowCharacteristics_In_Seat_MS_Response_using_OfferDoc()
        {
            var flightSeatMap = TestDataProvider.GetTestData<FlightSegmentSeatMap>("NDCOffer\\SeatAvailability\\FlightSeatMap");
            var seatMSRS = GDSSeatAvailabilityRSMapper.GetRowCharacteristics(flightSeatMap.Seatmap.First().SeatRows.First());

            Assert.NotNull(seatMSRS);
            Assert.Equal("X", seatMSRS[0]);
        }
    }

}

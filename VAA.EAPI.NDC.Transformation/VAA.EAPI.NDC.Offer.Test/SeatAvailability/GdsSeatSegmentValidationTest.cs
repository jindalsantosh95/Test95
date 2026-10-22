using Gremlin.Net.Process.Traversal;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json;
using System.Data;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class GdsSeatSegmentValidationTest
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;

        public GdsSeatSegmentValidationTest()        
        {
            _hostEnvironment = new Mock<IHostEnvironment>();

        }


        [Fact]
        public void Should_throw_Warning_when_Non_VS_MarketingCarrier_Flight_Segment_is_found_in_OrderDoc()
        {
            OrderSearchResponse orderDoc = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOffer\\SeatAvailability\\OrderSearch");
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            orderDoc.SegmentDetails.First().MarketingCarrier = "AF";
            orderDoc.SegmentDetails.First().OperatingCarrier = "AF";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);
            var validationErrors = seatAvailabilityValidationsObj.PopulateGDSNonVSSegmentWarnings(orderDoc, _hostEnvironment.Object);

            Assert.True(validationErrors.Any());
            Assert.Equal("200",validationErrors.First().Code);
            Assert.Equal("Advance Seat Selection Not Available for this Flight - AF", validationErrors.First().ShortText);
            Assert.Equal("Seatmap not Available", validationErrors.First().TagText);
            Assert.Equal("Warning", validationErrors.First().Type);
            
        }

        [Fact]
        public void Should_not_throw_Warning_when_Non_VS_MarketingCarrier_Flight_Segment_is_found_in_OrderDoc()
        {
            OrderSearchResponse orderDoc = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOffer\\SeatAvailability\\OrderSearch");
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            orderDoc.SegmentDetails.First().MarketingCarrier = "DL";
            orderDoc.SegmentDetails.First().OperatingCarrier = "DL";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);
            var validationErrors = seatAvailabilityValidationsObj.PopulateGDSNonVSSegmentWarnings(orderDoc, _hostEnvironment.Object);

            Assert.True(validationErrors.Any());
            Assert.Null(validationErrors.First());

        }

        [Fact]
        public void Should_not_throw_Warning_when_VS_Flight_Segment_is_found_in_OrderDoc()
        {
            OrderSearchResponse orderDoc = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOffer\\SeatAvailability\\OrderSearch");
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateGDSNonVSSegmentWarnings(orderDoc, _hostEnvironment.Object);

            Assert.True(validationErrors.Any());
            Assert.Null(validationErrors.First());
        }

        [Fact]
        public void Should_throw_Warning_when_Non_VS_Flight_Segment_is_found_in_SeatResponse()
        {
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.First().FlightSegment.OperatingCarrier = "DL";
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.First().FlightSegment.MarketingCarrier = "DL";
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.Last().FlightSegment.OperatingCarrier = "DL";
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.Last().FlightSegment.MarketingCarrier = "DL";
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateGDSSeatMapResponseWarnings(seatMsResponse, _hostEnvironment.Object);

            Assert.NotNull(validationErrors);
            Assert.Equal("200", validationErrors.First().Code);
            Assert.Equal("Advance Seat Selection Not Available for this Flight - DL301", validationErrors.First().ShortText);
            Assert.Equal("Seatmap not Available", validationErrors.First().TagText);
            Assert.Equal("Warning", validationErrors.First().Type);
            Assert.Equal("200", validationErrors.Last().Code);
            Assert.Equal("Advance Seat Selection Not Available for this Flight - DL137", validationErrors.Last().ShortText);
            Assert.Equal("Seatmap not Available", validationErrors.Last().TagText);
            Assert.Equal("Warning", validationErrors.Last().Type);
        }

        [Fact]
        public void Should_throw_Warning_when_VS_MarketingCarrier_Flight_Segment_is_found_in_OrderDoc()
        {
            OrderSearchResponse orderDoc = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOffer\\SeatAvailability\\OrderSearch");
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            orderDoc.SegmentDetails.First().MarketingCarrier = "DL";
            orderDoc.SegmentDetails.First().OperatingCarrier = "DL";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);
            var validationErrors = seatAvailabilityValidationsObj.PopulateGDSVSSegmentWarnings(orderDoc, _hostEnvironment.Object);

            Assert.True(validationErrors.Any());
            Assert.Equal("207", validationErrors.First().Code);
            Assert.Equal("One or more passenger(s) within the order have already reserved seats - Change seat is not allowed", validationErrors.First().ShortText);
            Assert.Equal("Request Validation Error", validationErrors.First().TagText);
            Assert.Equal("Warning", validationErrors.First().Type);

        }
    }
}

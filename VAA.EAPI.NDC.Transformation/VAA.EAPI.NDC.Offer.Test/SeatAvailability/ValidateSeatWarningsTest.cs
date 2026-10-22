using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using Moq;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateSeatWarningsTest
    {
        readonly Mock<IHostEnvironment> _hostEnvironment = new Mock<IHostEnvironment>();


        [Fact]
        public void Should_not_throw_Error_Whe_DL_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "DL";

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(orderDoc);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Error_When_VS_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = null;

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(orderDoc);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Error_When_NonVS_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "AF";

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(orderDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("200_1", validationErrors.First().Code);

        }

        [Fact]
        public void Should_not_throw_Warning_When_VS_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = null;


            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateVSSegmentWarnings(orderDoc);
            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Warning_When_DL_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "DL";


            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateVSSegmentWarnings(orderDoc);
           
            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Warning_When_NonVS_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "AF";

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateVSSegmentWarnings(orderDoc);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_throw_Warning_When_VS_And_NonVS_Flight_Segment_is_found_in_OrderDoc()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDocMultiSegments");
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = null ;
            orderDoc.DataList.SegmentDetails.Last().OperatingCarrier = "AF";

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(orderDoc);

            Assert.True(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Warning_When_VS_Flight_Segment_is_found_in_OfferDoc()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(offerDoc);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_not_throw_Warning_When_DL_Flight_Segment_is_found_in_OfferDoc()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.DataList.FlightSegments.First().OperatingCarrier = "DL";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(offerDoc);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_throw_Warning_When_Non_VS_Flight_Segment_is_found_in_OfferDoc()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.DataList.FlightSegments.First().OperatingCarrier = "AF";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(offerDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("200_1", validationErrors.First().Code);

        }

        [Fact]
        public void Should_throw_Warning_When_VS_And_Non_VS_Flight_Segment_is_found_in_OfferDoc()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.DataList.FlightSegments.First().OperatingCarrier = null;
            offerDoc.DataList.FlightSegments.Last().OperatingCarrier = "AF";
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateNonVSSegmentWarnings(offerDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("200_1", validationErrors.First().Code);

        }

        [Fact]
        public void Should_throw_Warning_When_No_Catalogue_Found_in_Flight_SeatMap_of_SeatMSResponse()
        {
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
          
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateSeatMapResponseWarningsPostSale(seatMsResponse);

            Assert.True(validationErrors.Any());
            Assert.Equal("200_1", validationErrors.First().Code);

        }

        [Fact]
        public void Should_not_throw_Warning_When_Flight_SeatMap_is_null_of_SeatMSResponse()
        {
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap = null;
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateSeatMapResponseWarningsPostSale(seatMsResponse);

            Assert.False(validationErrors.Any());

        }

        [Fact]
        public void Should_throw_Warning_When_Flight_SeatRow_is_Available_in_SeatMap_of_SeatMSResponse()
        {
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateSeatMapResponseWarnings(seatMsResponse);

            Assert.True(validationErrors.Any());
            Assert.Equal("200_1", validationErrors.First().Code);

        }

        [Fact]
        public void Should_Not_throw_Warning_When_Flight_SeatRow_is_Available_in_SeatMap_of_SeatMSResponse()
        {
            var seatMsResponse = TestDataProvider.GetTestData<SeatRS>("NDCOffer\\SeatAvailability\\SeatMsResponse");
            seatMsResponse.SeatAvailabilityResponse.FlightSegmentSeatMap = null;

            SeatAvailabilityValidations<T> seatAvailabilityValidationsObj = new SeatAvailabilityValidations<T>(_hostEnvironment.Object);

            var validationErrors = seatAvailabilityValidationsObj.PopulateSeatMapResponseWarnings(seatMsResponse);

            Assert.False(validationErrors.Any());

        }

    }
}

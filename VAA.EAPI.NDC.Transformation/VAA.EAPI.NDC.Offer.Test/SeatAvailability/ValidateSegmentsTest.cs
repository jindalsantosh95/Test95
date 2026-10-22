using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateSegmentsTest
    {
        [Fact]
        public void Should_throw_Error_When_Segment_is_nul()
        {
            IEnumerable<Segment> segmentList = Enumerable.Empty<Segment>();

            var validationErrors = ValidateSeatAvailability.ValidateEligibleSegmentSeatsWithOrderId(segmentList);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("197_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_not_throw_Error_When_Segment_is_valid()
        {
            IEnumerable<Segment> segmentList = new Segment[] { new() { OperatingCarrier = "DL" } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleSegmentSeatsWithOrderId(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_not_throw_Error_When_VSS_Segment_is_valid()
        {
            IEnumerable<Segment> segmentList = new Segment[] { new() { OperatingCarrier = null } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleVssSegment(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_not_throw_Error_When_DL_Segment_is_Found()
        {
            IEnumerable<Segment> segmentList = new Segment[] { new() { OperatingCarrier = "DL" } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleVssSegment(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_throw_Error_When_Non_VS_Segment_is_found()
        {
            IEnumerable<Segment> segmentList = new Segment[] { new() { OperatingCarrier = "AF" } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleVssSegment(segmentList);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("200_1", validationErrors.First().ErrorCode);

        }


        [Fact]
        public void Should_throw_Error_When_Non_VS_Flight_Segment_is_found()
        {
            IEnumerable<Domain.Model.Order.FlightSegment> segmentList = new Domain.Model.Order.FlightSegment[] { new() { OperatingCarrier = "AF" } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleSegment(segmentList);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("200_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_not_throw_Error_When_VS_FlightSegment_is_found()
        {
            IEnumerable<Domain.Model.Order.FlightSegment> segmentList = new Domain.Model.Order.FlightSegment[] { new() { OperatingCarrier = "DL" } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleSegment(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_not_throw_Error_When_DL_FlightSegment_is_found()
        {
            IEnumerable<Domain.Model.Order.FlightSegment> segmentList = new Domain.Model.Order.FlightSegment[] { new() { OperatingCarrier = null } };

            var validationErrors = ValidateSeatAvailability.ValidateEligibleSegment(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_throw_Error_When_GDSSegment_is_null()
        {
            IEnumerable<Segment> segmentList = Enumerable.Empty<Segment>();

            var validationErrors = ValidateSeatAvailability.ValidateGDSEligibleSegment(segmentList);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("197_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_not_throw_Error_When_GDSSegment_is_valid()
        {
            IEnumerable<Segment> segmentList = new Segment[] { new() { OperatingCarrier = "DL" } };

            var validationErrors = ValidateSeatAvailability.ValidateGDSEligibleSegment(segmentList);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_throw_Error_When_GDSFlownSegment_is_invalid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            orderChangeRequest.OrderSearchResponse.OrderItems.First().AccountableDocumentRefs.First().AllFlown = true;

            var validationErrors = ValidateSeatAvailability.ValidateGDSFlownSegments(orderChangeRequest);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("208_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_not_throw_Error_When_GDSFlownSegment_is_valid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            orderChangeRequest.OrderSearchResponse.OrderItems.First().AccountableDocumentRefs.First().AllFlown = false;

            var validationErrors = ValidateSeatAvailability.ValidateGDSFlownSegments(orderChangeRequest);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_not_throw_Error_When_OrderSearchResponse_is_valid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");

            var validationErrors = ValidateSeatAvailability.ValidateOrderSearchNullResponse(orderChangeRequest);

            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_throw_Error_When_OrderSearchResponse_is_invalid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            orderChangeRequest = null;

            var validationErrors = ValidateSeatAvailability.ValidateOrderSearchNullResponse(orderChangeRequest);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("914_11", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_throw_Error_When_OrderSearchResponse_is_invalidvalue()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            orderChangeRequest.OrderSearchResponse = null;

            var validationErrors = ValidateSeatAvailability.ValidateOrderSearchNullResponse(orderChangeRequest);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("914_11", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void Should_not_throw_Error_When_GDSAgent_is_valid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            var iataNumber = "37200424";

            var validationErrors = ValidateSeatAvailability.ValidateGdsAgentBooking(orderChangeRequest, iataNumber);
       
            Assert.NotNull(validationErrors);
            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Should_throw_Error_When_GDSAgent_is_invalid()
        {
            var orderChangeRequest = TestDataProvider.GetTestData<Air4OrderSearchMSResponse>("NDCOffer\\SeatAvailability\\Air4OrderSearchResponse");
            var iataNumber = "37200644";

            var validationErrors = ValidateSeatAvailability.ValidateGdsAgentBooking(orderChangeRequest, iataNumber);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("368_2", validationErrors.First().ErrorCode);
        }
    }
}

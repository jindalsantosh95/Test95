using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateSeatAvailabilityOrderDetailsTest
    {
        [Fact]
        public void Should_throw_error_when_seatOrder_is_null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            (request.Request?.SeatAvailCoreRequest?.Item as SeatAvailOrderRequestType).Order = null;

            var responseMessage = ValidateSeatAvailability.ValidateSeatOrderDetails(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("912_1", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_seatOrder_is_Empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order.OrderID = string.Empty;
            var responseMessage = ValidateSeatAvailability.ValidateSeatOrderDetails(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("912_1", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrderId_is_null_And_SeatOrder_is_Null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order.OrderID = null;

            var responseMessage = ValidateSeatAvailability.ValidateSeatOrderDetails(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("912_1", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrderId_is_null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order.OrderID = null;

            var responseMessage = ValidateSeatAvailability.ValidateSeatOrderDetails(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("912_1", responseMessage.Last().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_is_null()
        {
            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(null);

            Assert.True(responseMessage.Any());
            Assert.Equal("129_1", responseMessage.Last().ErrorCode);

        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_has_Warning_Code_as_129()
        {
            Warning orderWarning = new() { Code = "129" };
            List<Warning> orderWarnings = new();
            orderWarnings.Add(orderWarning);
            OrderDocument orderDoc = new() { Warnings = orderWarnings };

            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.True(responseMessage.Any());
            Assert.Equal("129_1", responseMessage.Last().ErrorCode);

        }      
        
        
        [Fact]
        public void Should_throw_error_when_OrderDoc_has_Warnings_Other_than_129()
        {
            Warning orderWarning = new() { Code = "120" };
            List<Warning> orderWarnings = new();
            orderWarnings.Add(orderWarning);
            OrderDocument orderDoc = new() { Warnings = orderWarnings };

            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_22", responseMessage.Last().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_has_All_Flown_Status()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");

            orderDoc.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "FLOWN/USED";
            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.True(responseMessage.Any());
            Assert.Equal("208_1", responseMessage.Last().ErrorCode);
        }

        [Fact]
        public void Should_not_throw_error_when_OrderDoc_has_all_Segements_has_NonVSS_Segments()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "Open";
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "DL";

            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.False(responseMessage.Any());
        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_has_all_Segements_has_NonVSS_Segments()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "Open";
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = "AF";

            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.True(responseMessage.Any());
            Assert.Equal("219_1", responseMessage.Last().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_has_all_Segments_has_VS_Segments()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "Open";
            orderDoc.DataList.SegmentDetails.First().OperatingCarrier = null;

            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.False(responseMessage.Any());
        }

        [Fact]
        public void Should_Not_throw_error_when_OrderDoc_has_MatchingAllCases()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            var responseMessage = ValidateSeatAvailability.ValidateOrderDetails(orderDoc);

            Assert.False(responseMessage.Any());
        }

        [Fact]
        public void Should_throw_error_when_OrderDoc_has_Warnings_in_OrderDoc()
        {
            Warning orderWarning = new() { Code = "120" };
            List<Warning> orderWarnings = new();
            orderWarnings.Add(orderWarning);
            OrderDocument orderDoc = new() { Warnings = orderWarnings };

            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOrderWarnings(orderDoc);

            Assert.True(responseMessage.Any());
            Assert.Equal("120", responseMessage.First().Code);
        }
    }
}

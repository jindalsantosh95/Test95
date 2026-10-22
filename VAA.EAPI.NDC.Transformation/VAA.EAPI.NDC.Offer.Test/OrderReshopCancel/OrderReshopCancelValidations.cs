using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using VAA.EAPI.NDC.Offer.Validations.OrderReshopCancel;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;


namespace VAA.EAPI.NDC.Offer.Tests.OrderReshopCancel
{
    public class OrderReshopCancelValidationsTests
    {
        private readonly IHostEnvironment _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

        [Fact]
        public void ValidateHoldBooking_HoldBooking_ReturnsUnpaidOrderError()
        {
            var response = OrderReshopCancelRequestValidations.ValidateHoldBooking<ErrorType>(true, _hostEnvironment);

            Assert.NotNull(response);
            Assert.Contains(response.ResponseMessages?.Errors ?? [], error => error.Id == "915_3");
        }

        [Fact]
        public void Validate_RFC_With_Incorrect_Agent()
        {

            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            Participant participantRequest = new Participant() { AgencyID = "12345678" , PartyType = Participant.PartyTypeEnum.Seller };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateRFCAgent(orderReshopRequest, orderDocRequest);

            Assert.Equal("368_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_Correct_Agent()
        {

            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            Participant participantRequest = new Participant() { AgencyID = "91281035" , PartyType = Participant.PartyTypeEnum.Seller };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateRFCAgent(orderReshopRequest, orderDocRequest);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Validate_RFC_With_Correct_AllSegments()
        {

            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            orderDocRequest.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "Open";

            var validationErrors = ValidateOrderReshop.ValidateSegmantStatus(orderDocRequest);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Validate_RFC_With_Correct_AllSegmentsFlown()
        {

            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            orderDocRequest.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "FLOWN/USED";

            var validationErrors = ValidateOrderReshop.ValidateSegmantStatus(orderDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("452_1", validationErrors.Last().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_InCorrect_OfferRefID()
        {

            var orderReshoCancelRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
            orderReshoCancelRequest.Request.OrderRefID = "$56cfd";
            var validationErrors = ValidateOrderReshop.ValidateRequestOrderId(orderReshoCancelRequest);

            Assert.Equal("911_16", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_Empty_OfferRefID()
        {

           var orderReshoCancelRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
           orderReshoCancelRequest.Request.OrderRefID = "";

           var validationErrors = ValidateOrderReshop.ValidateRequestOrderId(orderReshoCancelRequest);

            Assert.Equal("914_4", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_Diffe_OfferRefID()
        {

            var orderReshoCancelRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
            orderReshoCancelRequest.Request.OrderRefID = "VS9327ACA4824";
            var validationErrors = ValidateOrderReshop.ValidateRequestOrderRefIds(orderReshoCancelRequest);

            Assert.Equal("911_17", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_Different_OfferRefID()
        {

            var orderReshopRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
            orderReshopRequest.Request.OrderRefID = "VS9328365424";
           
            var validationErrors = ValidateOrderReshop.ValidateRequestOrderRefIds(orderReshopRequest);

            Assert.Equal("911_17", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_null_OfferRefID()
        {

            var orderReshopRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
            orderReshopRequest.Request.OrderRefID = " ";

            var validationErrors = ValidateOrderReshop.ValidateRequestOrderRefIds(orderReshopRequest);

            Assert.Equal("911_17", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_RFC_With_same_OfferRefID()
        {

            var orderReshopRequest = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");

            var validationErrors = ValidateOrderReshop.ValidateRequestOrderRefIds(orderReshopRequest);

            Assert.False( validationErrors.Any());
        }
    }
}

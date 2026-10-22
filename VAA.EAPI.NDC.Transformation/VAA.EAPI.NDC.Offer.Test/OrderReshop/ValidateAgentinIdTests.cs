using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class ValidateAgentIdTests
    {
        [Fact]
        public void ValidateAgentID_Request()
        {
            var agentRequest = TestDataProvider.GetTestData<AgencyValidateResponse>("NDCOffer\\OrderReshop\\AgencyValidationMSRes");
            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc");
            var validationErrors = ValidateOrderReshop.ValidateAgentIDReshop(agentRequest, orderDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateNullPNR_Request()
        {
            BookingReference bookingReference = new BookingReference() { RLOC = "CMR437" };
            var orderReshopRequest = new OrderReshopRequest() { BookingReferences = bookingReference };
            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc");
            var validationErrors = ValidateOrderReshop.ValidateReshopPNR(orderReshopRequest, orderDocRequest);

            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void ValidateEmptyOrderID()
        {
            OrderReshopRequest orderReshopRequest = new OrderReshopRequest() { OrderId = " " };
            var validationErrors = ValidateOrderReshop.ValidateOrderIDFormat(orderReshopRequest);

            Assert.Equal("914_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateOrderStatus()
        {
            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc");
            orderDocRequest.OrderItems.First().AccountableDocumentRefs = null;

            var validationErrors = ValidateOrderReshop.ValidateOrderStatus(orderDocRequest);

            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void ValidateReshopServiceID()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            var orderRequest = TestDataProvider.GetTestData<OrderReshopRequest>("NDCOffer\\OrderReshop\\ReshopRQMultiPax");

            var validationErrors = ValidateOrderReshop.ValidateReshopServiceID(orderRequest , orderDoc);
            Assert.Equal("129_4", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateReshopServiceID_CorrectRequest()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            var orderRequest = TestDataProvider.GetTestData<OrderReshopRequest>("NDCOffer\\OrderReshop\\ReshopRQMultiPax");
            orderRequest.ReshopOrder.First().Services = [new() { ServiceId = "SV1"}, new() { ServiceId = "SV3"}];

            var validationErrors = ValidateOrderReshop.ValidateReshopServiceID(orderRequest, orderDoc);
            Assert.Empty(validationErrors);
        }

        [Fact]
        public void ValidateReshopServiceID_InvalidOrderItemId()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            var orderRequest = TestDataProvider.GetTestData<OrderReshopRequest>("NDCOffer\\OrderReshop\\ReshopRQMultiPax");
            orderRequest.ReshopOrder.First().OrderItemId = "OI3";
        
            var validationErrors = ValidateOrderReshop.ValidateReshopServiceID(orderRequest, orderDoc);
            Assert.Empty(validationErrors);
        }
    }
}

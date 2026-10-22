using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderDataMapperTests
    {
        public OrderDataMapperTests()
        {
            OrderDataTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void OrderDataCommandToEventMapper_ShouldSucceed_WithGoodRequest()
        {
            var TicketVoidCommand = TestDataProvider.GetTestData<OrderCreateRequestCommand>("TicketMS\\VoidTicketEventCommand");
            var orderEvent = OrderDataCommandToEventMapper.CommandToEventMapper(TicketVoidCommand);
            Assert.NotNull(orderEvent);
            Assert.True(orderEvent.Type == "VAA.AIS.Air4.VoidTicketFeeds");
            Assert.True(orderEvent.Id == "12fe1e9e-4ca0-46c3-b48e-45a917bebad7");
        }
        [Fact]
        public void OrderNotificationEventMapper_ShouldSucceed_WithGoodRequest()
        {
            var TicketVoidCommand = TestDataProvider.GetTestData<OrderCreateRequestCommand>("TicketMS\\VoidTicketEventCommand");
            var request = TestDataProvider.GetTestData<OrderCreateResponse>("PaymentMs\\CreateOrderResponse");
            var result = OrderNotificationEventMapper.OrderDetailMapper(TicketVoidCommand, request);
            Assert.NotNull(result);
            Assert.True(result.Id == "12fe1e9e-4ca0-46c3-b48e-45a917bebad7");
        }
        [Fact]
        public void PaymentUpdateEventMapper_ShouldSucceed_WithGoodRequest()
        {

            var request2 = TestDataProvider.GetTestData<PaymentUpdateCommand>("PaymentMS\\OrderPaymentCommandRequest");
            var request3 = TestDataProvider.GetTestData<PaymentUpdateRequestData>("PaymentMS\\PaymentUpdateRequestData");
            var result = PaymentUpdateEventMapper.EventMapper(PaymentUpdateStatus.Succeeded, request2, request3);
            Assert.NotNull(result);
            //Assert.True(result.Type == "VAA.Order.Payment.InitiateVoidOrderUpdate");
        }
        [Fact]
        public void VoidOrderEventMapper_ShouldSucceed_WithGoodRequest()
        {
            var request = TestDataProvider.GetTestData<OrderReadRequestQuery>("PaymentMS\\VoidOrderDetails-CPD");
            var request1 = TestDataProvider.GetTestData<VoidOrderDetails>("PaymentMs\\VoidOrderDetailsData");
            var result = VoidOrderEventMapper.OrderDetailMapper(request1, request);
            Assert.NotNull(result);
            Assert.True(result.Id == "11120980-bb96-415e-9860-03900777a192");
        }
        [Fact]
        public void VoidOrderDetailsMapper_ShouldSucceed_WithGoodRequest()
        {
            var request = TestDataProvider.GetTestData<PnrSearch>("PaymentMS\\VoidOrderDetails-CPD");
            var request1 = TestDataProvider.GetTestData<OrderReadRequestData>("PaymentMs\\PaymentUpdateRequestData");

            var result = VoidOrderDetailsMapper.MapVoidOrderDetails(request, request1);
            Assert.NotNull(result);
            //Assert.True(result.IssueDate == "20220908");
        }



    }
}

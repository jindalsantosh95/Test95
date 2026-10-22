using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Tests
{
    public class OrderTicketMapperTests
    {
        public OrderTicketMapperTests()
        {
            OrderTicketTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void TicketVoidCommandToEventMapper_ShouldSucceed_WithGoodRequest()
        {
            var TicketVoidCommand = TestDataProvider.GetTestData<VoidTicketDetails>("TicketMS\\VoidTicketEventCommand");
            var correlationId = "123345666";
            var orderEvent = TicketVoidCommandToEventMapper.CommandToEventMapper(TicketVoidCommand, correlationId);
            Assert.NotNull(orderEvent);
            Assert.True(orderEvent.Type == "VAA.Order.Ticket.GetOrderDetail");
            Assert.True(orderEvent.Id == "123345666");
        }
        [Fact]
        public void TicketVoidCommandToEventMapper_ShouldFail_WithNullCorrelationId()
        {
            var ticketDetails = TestDataProvider.GetTestData<VoidTicketDetails>("TicketMS\\VoidTicketDetails");
            var result = TicketVoidCommandToEventMapper.CommandToEventMapper(ticketDetails, null);
            Assert.NotNull(result);
            Assert.True(result.Id == null);
        }
        [Fact]
        public void OrderRefundEventMapper_ShouldSucceed_WithGoodRequest()
        {
            var OrderRefundEvent = TestDataProvider.GetTestData<VoidOrderDetailEventProcessCommand>("TicketMS\\VoidTicketEventCommand");
            var result = OrderRefundEventMapper.MapToOrderRefundEvent(OrderRefundEvent);
            Assert.NotNull(result);
            Assert.NotNull(result.Time);
        }
        [Fact]
        public void OrderRefundEventMapper_ShouldFail_WithNullId()
        {
            var orderTicketCommand = TestDataProvider.GetTestData<VoidOrderDetailEventProcessCommand>("TicketMS\\VoidTicketEventCommand");
            orderTicketCommand.Id = null;
            var result = OrderRefundEventMapper.MapToOrderRefundEvent(orderTicketCommand);
            Assert.True(result.Id == null);
        }
    }
}

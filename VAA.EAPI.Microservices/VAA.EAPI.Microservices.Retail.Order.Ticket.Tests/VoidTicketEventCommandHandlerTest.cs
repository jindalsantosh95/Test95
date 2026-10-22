using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Helper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Tests
{
    public class VoidTicketEventCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<VoidTicketEventCommandHandler>> _logger;
        private readonly VoidTicketEventCommandHandler _handler;


        public VoidTicketEventCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<VoidTicketEventCommandHandler>>();
            _handler = new VoidTicketEventCommandHandler(_mediator.Object, _logger.Object);

            OrderTicketTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTicketEventCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTicketEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task VoidTicketEventCommandHandler_ShouldFail_WhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new VoidTicketEventCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task VoidTicketEventCommandHandler_ShouldReturnOKResponse_WithGoodRequest()
        {

            var request = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand");

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task VoidTicketEventCommandHandler_ShouldReturnSuccessStatusResponse_WithMockResponseEnabledAsY()
        {
            var request = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand");
            ConfigReader.Set("MockResponseEnabled", "Y");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task VoidTicketEventCommandHandler_ShouldReturnSuccessStatusResponse_WithMockResponseEnabledAsN()
        {
            var request = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand");
            ConfigReader.Set("MockResponseEnabled", "N");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public void TicketFeedHelperT_ShouldFail_WithGoodRequest()
        {
            var ticketFeed = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand");
            var ticketFeedVoidRequestData = ticketFeed.Data.ToString();
            var result = TicketFeedHelper.GetPnrTicketDetails(ticketFeedVoidRequestData);
            Assert.True(result.IsFailure);
        }
        [Fact]
        public void TicketFeedHelper_ShouldSucceed_WithSingleRowData()
        {
            var ticketFeed = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand-Success");
            var ticketFeedVoidRequestData = ticketFeed.Data.ToString();
            var result = TicketFeedHelper.GetPnrTicketDetails(ticketFeedVoidRequestData);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void TicketFeedHelper_ShouldSucceed_WitMultiRowData()
        {
            var ticketFeed = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand-multi-rows");
            var ticketFeedVoidRequestData = ticketFeed.Data.ToString();
            var result = TicketFeedHelper.GetPnrTicketDetails(ticketFeedVoidRequestData);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task VoidTicketEventCommandHandler_ShouldReturnSuccessStatusResponse_WithGoodRequest()
        {
            var request = TestDataProvider.GetTestData<VoidTicketEventCommand>("TicketMS\\VoidTicketEventCommand-Success");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);


        }
    }
}

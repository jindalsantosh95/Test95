using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Seat.Application;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Seat.Tests
{
    public class SeatFulfilmentEventProcessCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<SeatFulfilmentEventSubscriberCommandHandler>> _logger;
        private readonly SeatFulfilmentEventSubscriberCommandHandler _handler;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailService> _emailLogger;

        public SeatFulfilmentEventProcessCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<SeatFulfilmentEventSubscriberCommandHandler>>();
            _emailLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<EmailService>.Instance;
            _emailService = new EmailService(_mediator.Object, _emailLogger);
            _handler = new SeatFulfilmentEventSubscriberCommandHandler(_mediator.Object, _logger.Object, _emailService);
            ConfigReader.Set("OrderChangeServiceBusConnectionString", "dummyConnString");            
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SeatFulfilmentEventSubscriberCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SeatFulfilmentEventSubscriberCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            var seatFulfilmentRequest = TestDataProvider.GetTestData<SeatFulfilmentRequest>("SeatMS\\SeatFulfilmentRequest-Cash");
            var command = new SeatFulfilmentEventProcessCommand() { Data = Json.Serialize(seatFulfilmentRequest), CorrelationId = Guid.NewGuid(), SessionId = Guid.NewGuid(), Channel = ["API"] };
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            
            //Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, new CancellationToken()));
        }

        [Fact]
        public async Task Handler_FaultedResponseShouldNotFail()
        {
            //Arrange
            var seatFulfilmentRequest = TestDataProvider.GetTestData<SeatFulfilmentRequest>("SeatMS\\SeatFulfilmentRequest");
            var seatFulfilmentResponse = TestDataProvider.GetXmlTestData<AddResponseType>("SeatMS\\Seatv8AddResponseIncomplete");

            var command = new SeatFulfilmentEventProcessCommand() { Data = Json.Serialize(seatFulfilmentRequest) };
            _mediator.Setup(x => x.Send(It.IsAny<SeatV8AddSeatQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AddResponseType> { seatFulfilmentResponse });
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_IncompleteResponseWithErrorShouldNotAddWarningOfSuccess()
        {
            //Arrange
            var seatFulfilmentRequest = TestDataProvider.GetTestData<SeatFulfilmentRequest>("SeatMS\\SeatFulfilmentRequest");
            var seatFulfilmentResponse = TestDataProvider.GetXmlTestData<AddResponseType>("SeatMS\\Seatv8AddResponseIncompleteWithError");

            var command = new SeatFulfilmentEventProcessCommand() { Data = Json.Serialize(seatFulfilmentRequest) };
            _mediator.Setup(x => x.Send(It.IsAny<SeatV8AddSeatQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AddResponseType> { seatFulfilmentResponse });
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}

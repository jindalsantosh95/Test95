using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderCreateRepoCommandHandlerTests
    {
        private readonly OrderCreateRepoCommandHandler _handler;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderCreateRepoCommandHandler>> _logger;

        public OrderCreateRepoCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderCreateRepoCommandHandler>>();
            _handler = new OrderCreateRepoCommandHandler(_mediator.Object, _logger.Object);
            ConfigReader.Set("ConversationId", "ABC123");
            ConfigReader.Set("FromPartyId", "1");
            ConfigReader.Set("ToPartyId", "2");
        }

        [Fact]
        public void HandlerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateRepoCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateRepoCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task HandlerShouldReturnFailure_WhenTokenServiceReturnsNull()
        {
            //Arrange
            var request = new OrderCreateRepoCommand() { OrderCreateRequest = TestDataProvider.GetTestData<OrderCreateRequest>("NDCOrderMS\\OrderCreateRequest") };
            string? token = null;
            _mediator.Setup(x => x.Send(It.IsAny<CreateSessionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(token);

            //Act
            var result = await _handler.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("G009", result.Error);
        }

        [Fact]
        public async Task HandlerShouldReturnFailure_WhenCreatePNRServiceReturnsError()
        {
            //Arrange
            var request = new OrderCreateRepoCommand() { OrderCreateRequest = TestDataProvider.GetTestData<OrderCreateRequest>("NDCOrderMS\\OrderCreateRequest") };
            _mediator.Setup(x => x.Send(It.IsAny<CreateSessionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync("dummyToken");
            _mediator.Setup(x => x.Send(It.IsAny<GetReservationQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetReservationRS()
            {
                Errors = [new() { Message =  "Error Occurred" }] 
            });

            //Act
            var result = await _handler.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
        }
    }
}

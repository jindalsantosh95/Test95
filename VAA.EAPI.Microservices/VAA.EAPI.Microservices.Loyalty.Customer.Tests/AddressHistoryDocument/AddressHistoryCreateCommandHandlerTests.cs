using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.AddressHistoryCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.AddressHistoryDocument
{
    public class AddressHistoryCreateCommandHandlerTests
    {
        private readonly AddressHistoryCreateCommandHandler _handler;
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<AddressHistoryCreateCommandHandler>> _mockLogger;

        public AddressHistoryCreateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddressHistoryCreateCommandHandler>>();
            _handler = new AddressHistoryCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryCreateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task AddressHistoryCreateCommandHandlerSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressHistoryCreateCommand>("CustomerMS\\AddressHistory\\AddressHistoryCreateEvent");

            _mockMediator.Setup(m => m.Send(It.IsAny<AddressHistoryRepoCreateCommand>(), CancellationToken.None)).ReturnsAsync(Unit.Value);
            var result = await _handler.Handle(eventRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddressHistoryCreateCommandHandlerFailure()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressHistoryCreateCommand>("CustomerMS\\AddressHistory\\AddressHistoryCreateEvent");

            _mockMediator.Setup(m => m.Send(It.IsAny<AddressHistoryRepoCreateCommand>(), CancellationToken.None)).Throws(new InvalidOperationException());
            var result = await _handler.Handle(eventRequest, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }
    }
}

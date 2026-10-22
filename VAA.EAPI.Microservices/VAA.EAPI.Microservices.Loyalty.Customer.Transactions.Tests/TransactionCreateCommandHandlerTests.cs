using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TransactionCreateCommandHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<TransactionCommandResponse>>> _mockLogger;
        private readonly TransactionCreateCommandHandler _handler;

        public TransactionCreateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseCommandHandler<TransactionCommandResponse>>>();
            _handler = new TransactionCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionCreateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void Handler_Success()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<TransactionCreateCommand>("Transaction\\TransactionCreateRequest");
           
            var response = _handler.Handle(eventRequest, CancellationToken.None);
           
            Assert.True(response.Result.IsSuccess);
            Assert.Equal(Status.Accepted, response.Result.Value.Status);
            Assert.Equal("location", response.Result.Value.HeaderParameters.FirstOrDefault().Key);
            Assert.Contains("http://localhost:7071/api/Transactions/status/", response.Result.Value.HeaderParameters.FirstOrDefault().Value);
        }

        [Fact]
        public void Handler_Exception()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<TransactionCreateCommand>("Transaction\\TransactionCreateRequest");
            
            var response = _handler.Handle(eventRequest, CancellationToken.None);
            
            Assert.True(response.Result.IsFailure);
            Assert.NotNull(response.Result.Error);
            Assert.Equal("Exception of type 'System.Exception' was thrown.", response.Result.Error);
        }
    }
}

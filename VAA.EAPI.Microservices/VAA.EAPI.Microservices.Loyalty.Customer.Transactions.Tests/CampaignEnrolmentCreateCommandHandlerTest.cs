using Azure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class CampaignEnrolmentCreateCommandHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<CampaignEnrolmentCommandResponse>>> _mockLogger;
        private readonly CampaignEnrolmentCreateCommandHandler _handler;


        public CampaignEnrolmentCreateCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseCommandHandler<CampaignEnrolmentCommandResponse>>>();
            _handler = new CampaignEnrolmentCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignEnrolmentCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignEnrolmentCreateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<CampaignEnrolmentCreateCommand>("CampaignEnrolmentDoc\\CampaignEnrolDoc");
            var response = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
        }

        [Fact]
        public void Handler_Success_With_Exception()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<CampaignEnrolmentCreateCommand>("CampaignEnrolmentDoc\\CampaignEnrolDoc");
            var response = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}

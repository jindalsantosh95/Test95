using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PromotionCreateRequestCommandHandler>> _mockLogger;
        private readonly PromotionCreateRequestCommandHandler _handler;

        public PromotionCreateRequestCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PromotionCreateRequestCommandHandler>>();
            _handler = new PromotionCreateRequestCommandHandler(_mockMediator.Object, _mockLogger.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateRequestCommandHandler(_mockMediator.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateRequestCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateRequestCommand>("CampaignManagementMS\\PromotionCreateValid");
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithInvalidRequest()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateRequestCommand>("CampaignManagementMS\\PromotionCreateValid");
            promotionCreateRequestCommand.PromotionName = null;
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFaliureWithError()
        {
            var promotionCreateRequestCommand = TestDataProvider.GetTestData<PromotionCreateRequestCommand>("CampaignManagementMS\\PromotionCreateValid");
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(promotionCreateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionLinking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionLinkEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignPromotionLinkEventProcessCommandHandler>> _mockLogger;
        private readonly CampaignPromotionLinkEventProcessCommandHandler _handler;
        public CampaignPromotionLinkEventProcessCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignPromotionLinkEventProcessCommandHandler>>();
            _handler = new CampaignPromotionLinkEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkEventProcessCommandHandler(_mockMediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkEventProcessCommandHandler(null, _mockLogger.Object));
        }
        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var campaignPromotionLinkEventData = TestDataProvider.GetTestData<CampaignPromotionLinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionLinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<InsertCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var response = _handler.Handle(campaignPromotionLinkEventData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.Null(response.Result.Value.Errors);

        }
        [Fact]
        public void Handler_Success_With_Error()
        {
            var campaignPromotionLinkEventData = TestDataProvider.GetTestData<CampaignPromotionLinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionLinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<InsertCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "AlreadyLinkedCampaignAndPromotion" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var response = _handler.Handle(campaignPromotionLinkEventData, System.Threading.CancellationToken.None);
            Assert.False(response.Result.IsSuccess);
            Assert.Equal("AlreadyLinkedCampaignAndPromotion", response.Result.Error);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var campaignPromotionLinkEventData = TestDataProvider.GetTestData<CampaignPromotionLinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionLinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<InsertCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var response = _handler.Handle(campaignPromotionLinkEventData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}

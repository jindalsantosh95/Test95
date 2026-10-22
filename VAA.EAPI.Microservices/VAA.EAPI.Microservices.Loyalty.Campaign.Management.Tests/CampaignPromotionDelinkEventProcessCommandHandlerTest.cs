using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionDelinking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionDelinkEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignPromotionDelinkEventProcessCommandHandler>> _mockLogger;
        private readonly CampaignPromotionDelinkEventProcessCommandHandler _handler;
        public CampaignPromotionDelinkEventProcessCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignPromotionDelinkEventProcessCommandHandler>>();
            _handler = new CampaignPromotionDelinkEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkEventProcessCommandHandler(_mockMediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkEventProcessCommandHandler(null, _mockLogger.Object));
        }
        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var campaignPromotionDelinkEventData = TestDataProvider.GetTestData<CampaignPromotionDelinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionDelinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<DeleteCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var response = _handler.Handle(campaignPromotionDelinkEventData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.Null(response.Result.Value.Errors);

        }
        [Fact]
        public void Handler_Success_With_Error()
        {
            var campaignPromotionDelinkEventData = TestDataProvider.GetTestData<CampaignPromotionDelinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionDelinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<DeleteCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "NoLinkFoundForCampaignAndPromotion" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var response = _handler.Handle(campaignPromotionDelinkEventData, System.Threading.CancellationToken.None);
            Assert.False(response.Result.IsSuccess);
            Assert.Equal("NoLinkFoundForCampaignAndPromotion", response.Result.Error);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var campaignPromotionDelinkEventData = TestDataProvider.GetTestData<CampaignPromotionDelinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionDelinkEventMessage");
            var dbResponse = _mockMediator.Setup(x => x.Send(It.IsAny<DeleteCampaignPromotionLinkCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var response = _handler.Handle(campaignPromotionDelinkEventData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}

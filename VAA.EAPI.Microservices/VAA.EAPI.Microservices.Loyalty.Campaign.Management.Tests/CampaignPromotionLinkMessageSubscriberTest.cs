using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionLinkMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignPromotionLinkMessageSubscriber _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<CampaignPromotionLinkMessageSubscriber>> _logger;

        public CampaignPromotionLinkMessageSubscriberTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignPromotionLinkMessageSubscriber>>();
            _controller = new CampaignPromotionLinkMessageSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkMessageSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task CampaignPromotionLinkMessageSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CampaignPromotionLinkEventProcessCommand>("CampaignManagementMS\\CampaignPromotionLinkEventMessage");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignPromotionLinkEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Campaign details updated successfully" });
            await _controller.Run(request, _context.Object);
        }
    }
}

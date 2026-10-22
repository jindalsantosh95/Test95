using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionUpdateMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PromotionUpdateMessageSubscriber _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<PromotionUpdateMessageSubscriber>> _logger;

        public PromotionUpdateMessageSubscriberTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PromotionUpdateMessageSubscriber>>();
            _controller = new PromotionUpdateMessageSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateMessageSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task PromotionUpdateEventSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<PromotionUpdateEventProcessCommand>("CampaignManagementMS\\PromotionUpdateEventProcessCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionUpdateEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Promotion details updated successfully" });
            await _controller.Run(request, _context.Object);
        }
    }
}

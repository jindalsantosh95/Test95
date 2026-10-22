using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PromotionCreateMessageSubscriber _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<PromotionCreateMessageSubscriber>> _logger;

        public PromotionCreateMessageSubscriberTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PromotionCreateMessageSubscriber>>();
            _controller = new PromotionCreateMessageSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateMessageSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task PromotionUpdateEventSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<PromotionCreateEventProcessCommand>("CampaignManagementMS\\PromotionCreateEventProcessCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionCreateEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Promotion details created successfully" });
            await _controller.Run(request, _context.Object);
        }
    }
}

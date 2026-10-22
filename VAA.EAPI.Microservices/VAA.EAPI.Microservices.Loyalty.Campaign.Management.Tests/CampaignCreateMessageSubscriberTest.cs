using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignCreateMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignCreateMessageSubscriber _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<CampaignCreateMessageSubscriber>> _logger;

        public CampaignCreateMessageSubscriberTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignCreateMessageSubscriber>>();
            _controller = new CampaignCreateMessageSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateMessageSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task CampaignCreateMessageSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CampaignCreateEventProcessCommand>("CampaignManagementMS\\CampaignCreateEventProcessCommand");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignCreateEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Campaign details updated successfully" });
            await _controller.Run(request, _context.Object);
        }
    }
}

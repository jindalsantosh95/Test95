using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignAssignMessageSubscriber _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<CampaignAssignMessageSubscriber>> _logger;

        public CampaignAssignMessageSubscriberTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignAssignMessageSubscriber>>();
            _controller = new CampaignAssignMessageSubscriber(_mockMediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignAssignMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignAssignMessageSubscriber(_mockMediator.Object, null));
        }

        [Fact]
        public async Task CampaignAssignMessageSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Campaign Assign initiated successfully" });
            await _controller.Run(request, _context.Object);
        }
    }
}

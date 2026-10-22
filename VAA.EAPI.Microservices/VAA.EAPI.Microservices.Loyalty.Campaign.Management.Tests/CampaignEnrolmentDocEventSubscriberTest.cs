using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignEnrolmentDocEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignEnrolmentDocEventSubscriber>> _logger;
        private readonly CampaignEnrolmentDocEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public CampaignEnrolmentDocEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignEnrolmentDocEventSubscriber>>();
            _subscriber = new CampaignEnrolmentDocEventSubscriber(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();

        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignEnrolmentDocEventSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task CampaignRegisterEventSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CampaignEnrolmentDocEventProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<CampaignEnrolmentDocEventProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "CampaignEnrolmentDoc event posted successfully" });
            await _subscriber.Run(request, _context.Object);
        }
    }
}

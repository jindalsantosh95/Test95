using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class EmailEventRequestSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<EmailEventRequestSubscriber>> _logger;
        private readonly EmailEventRequestSubscriber _subscriber;

        public EmailEventRequestSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<EmailEventRequestSubscriber>>();
            _subscriber = new EmailEventRequestSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailEventRequestSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailEventRequestSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task EmailEventRequestSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<EmailEventRequestProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "CampaignEnrolmentDoc event posted successfully" });
            await _subscriber.Run(request);
        }
    }
}

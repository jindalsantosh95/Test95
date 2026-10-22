using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class TargetedMembershipDocCreateSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipDocCreateSubscriber>> _logger;
        private readonly TargetedMembershipDocCreateSubscriber _subscriber;

        public TargetedMembershipDocCreateSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipDocCreateSubscriber>>();
            _subscriber = new TargetedMembershipDocCreateSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocCreateSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocCreateSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task TargetedMembershipDocCreateSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipDocCreateCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "CampaignEnrolmentDoc event posted successfully" });
            await _subscriber.Run(request);
        }
    }
}

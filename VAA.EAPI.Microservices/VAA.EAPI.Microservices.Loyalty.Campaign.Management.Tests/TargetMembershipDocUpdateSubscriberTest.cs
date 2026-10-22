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
    public class TargetMembershipDocUpdateSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipDocSubscriber>> _logger;
        private readonly TargetedMembershipDocSubscriber _subscriber;

        public TargetMembershipDocUpdateSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipDocSubscriber>>();
            _subscriber = new TargetedMembershipDocSubscriber(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task TargetedMembershipDocSubscriber_SuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipDocUpdateCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocUpdateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "CampaignEnrolmentDoc event posted successfully" });
            await _subscriber.Run(request);
        }
    }
}

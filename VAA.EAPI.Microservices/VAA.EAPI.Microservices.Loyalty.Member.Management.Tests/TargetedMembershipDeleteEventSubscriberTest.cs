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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class TargetedMembershipDeleteEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipDeleteEventSubscriber>> _logger;
        private readonly TargetedMembershipDeleteEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public TargetedMembershipDeleteEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipDeleteEventSubscriber>>();
            _subscriber = new TargetedMembershipDeleteEventSubscriber(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDeleteEventSubscriber(_mediator.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDeleteEventSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task TargetedMembershipDeleteEventSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipDeleteEventCommand>("CampaignManagementMS\\TargetedMembershipDeleteEventData");
            var request = JsonSerializer.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDeleteEventCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            await _subscriber.Delete(request, _context.Object);
        }
    }
}

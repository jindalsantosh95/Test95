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
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseholdEventsSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<HouseholdEventsSubscriber>> _logger;
        private readonly HouseholdEventsSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public HouseholdEventsSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<HouseholdEventsSubscriber>>();
            _subscriber = new HouseholdEventsSubscriber(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new HouseholdEventsSubscriber(null, _logger.Object));
        }

        [Fact]
        public async Task HouseholdEventsSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<CloudEvent>("MemberManagement\\HouseHoldDissolveCommandEventData");
            var request = JsonSerializer.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<HouseHoldDissolveCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            await _subscriber.Delete(request, _context.Object);
        }
    }
}

using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class LTGEventsSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly LTGEventsSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public LTGEventsSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _subscriber = new LTGEventsSubscriber(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LTGEventsSubscriber(null));
        }

        [Fact]
        public async Task LTGAwardEventSubscriberSuccessAsync()
        {
            var eventRequest = TestDataProvider.GetTestData<LTGAwardCloudEvent>("MemberManagement\\LTGAwardEvent");
            var request = JsonSerializer.Serialize(eventRequest);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<LTGAwardCloudEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Success" });
            await _subscriber.LTGAwardEventSubscriber(request, _context.Object);
        }
    }
}

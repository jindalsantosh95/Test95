using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Reflection;
using System.Text.Json;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class CustomerProfileAuditEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly CustomerProfileAuditEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public CustomerProfileAuditEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _subscriber = new CustomerProfileAuditEventSubscriber(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerProfileAuditEventSubscriber(null));
        }

        [Fact]
        public async Task CustomerProfileAuditEventSubscriberSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<CustomerProfileAuditCreateEvent>("AuditMS\\CustomerProfileAudit\\CustomerProfileAuditCreateEvent");
            var request = JsonSerializer.Serialize(eventRequest);

            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<CustomerProfileAuditCreateEvent>(), CancellationToken.None)).ReturnsAsync(new CSharpFunctionalExtensions.Result<Unit>());

            await _subscriber.Create(request, _context.Object);
        }
    }
}

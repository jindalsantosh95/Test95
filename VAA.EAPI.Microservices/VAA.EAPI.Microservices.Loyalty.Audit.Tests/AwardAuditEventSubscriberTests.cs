using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Reflection;
using System.Text.Json;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.Award;
using VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class AwardAuditEventSubscriberTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly AwardAuditEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public AwardAuditEventSubscriberTests()
        {
            _mediator = new Mock<IMediator>();
            _subscriber = new AwardAuditEventSubscriber(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AwardAuditEventSubscriber(null));
        }

        [Fact]
        public async Task AwardAuditEventSubscriberSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<AwardAuditCreateEvent>("AuditMS\\AwardAuditCreateEvent");
            var request = JsonSerializer.Serialize(eventRequest);

            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<AwardAuditCreateEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(new CSharpFunctionalExtensions.Result<Unit>());

            await _subscriber.Create(request, _context.Object);
        }
    }
}

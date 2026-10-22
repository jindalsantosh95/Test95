using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Reflection;
using System.Text.Json;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.LoyaltyGenericAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class LoyaltyGenericAuditEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly LoyaltyGenericAuditEventSubscriber _subscriber;
        private readonly Mock<FunctionContext> _context;

        public LoyaltyGenericAuditEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _subscriber = new LoyaltyGenericAuditEventSubscriber(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltyGenericAuditEventSubscriber(null));
        }

        [Fact]
        public async Task LoyaltyGenericAuditEventSubscriberSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<LoyaltyGenericAuditCreateEvent>("AuditMS\\LoyaltyGenericAuditCreateEvent");
            var request = JsonSerializer.Serialize(eventRequest);

            _context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mediator.Setup(m => m.Send(It.IsAny<LoyaltyGenericAuditCreateEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(new CSharpFunctionalExtensions.Result<Unit>());

            await _subscriber.Create(request, _context.Object);
        }
    }
}
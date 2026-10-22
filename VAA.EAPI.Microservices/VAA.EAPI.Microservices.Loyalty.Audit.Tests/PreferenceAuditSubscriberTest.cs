using MediatR;
using Moq;
using System.Text.Json;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.PreferenceAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class PreferenceAuditEventSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly PreferenceAuditEventSubscriber _subscriber;

        public PreferenceAuditEventSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _subscriber = new PreferenceAuditEventSubscriber(_mediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PreferenceAuditEventSubscriber(null));
        }

        [Fact]
        public async Task PreferenceAuditEventSubscriberSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<PreferenceAuditCreateEvent>("AuditMS\\PreferenceAudit\\PreferenceAuditCreateEvent");
            var request = JsonSerializer.Serialize(eventRequest);

            _mediator.Setup(m => m.Send(It.IsAny<PreferenceAuditCreateEvent>(), System.Threading.CancellationToken.None)).ReturnsAsync(new CSharpFunctionalExtensions.Result<Unit>());

            await _subscriber.Create(request);
        }
    }
}

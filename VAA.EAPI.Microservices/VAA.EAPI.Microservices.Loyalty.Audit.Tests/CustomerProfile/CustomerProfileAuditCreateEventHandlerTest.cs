using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CustomerProfile;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.CustomerProfile
{

    public class CustomerProfileAuditCreateEventHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        private readonly CustomerProfileAuditCreateEventHandler _handler;

        public CustomerProfileAuditCreateEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new CustomerProfileAuditCreateEventHandler(_mockMediator.Object);
            AuditTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditCreateEventHandler(null));
        }

        [Fact]
        public async Task HandlerShouldSuccessWithNoError()
        {
            var eventRequest = TestDataProvider.GetTestData<CustomerProfileAuditCreateEvent>("AuditMS\\CustomerProfileAudit\\CustomerProfileAuditCreateEvent");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationError()
        {
            var eventRequest = TestDataProvider.GetTestData<CustomerProfileAuditCreateEvent>("AuditMS\\CustomerProfileAudit\\CustomerProfileAuditCreateEventInvalid");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }
    }
}

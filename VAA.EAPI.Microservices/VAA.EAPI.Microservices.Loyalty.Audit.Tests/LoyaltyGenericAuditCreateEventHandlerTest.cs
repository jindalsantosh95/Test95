using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.LoyaltyGenericAudit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.CustomerProfile
{
    public class LoyaltyGenericAuditCreateEventHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        private readonly LoyaltyGenericAuditCreateEventHandler _handler;

        public LoyaltyGenericAuditCreateEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new LoyaltyGenericAuditCreateEventHandler(_mockMediator.Object);
            AuditTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoyaltyGenericAuditCreateEventHandler(null));
        }

        [Fact]
        public async Task HandlerShouldSuccessWithNoError()
        {
            var eventRequest = TestDataProvider.GetTestData<LoyaltyGenericAuditCreateEvent>("AuditMS\\LoyaltyGenericAuditCreateEvent");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationError()
        {
            var eventRequest = TestDataProvider.GetTestData<LoyaltyGenericAuditCreateEvent>("AuditMS\\LoyaltyGenericAuditCreateEvent");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }
    }
}


using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.Award;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Award
{
    public class AwardAuditCreateEventHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        private readonly AwardAuditCreateEventHandler _handler;

        public AwardAuditCreateEventHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new AwardAuditCreateEventHandler(_mockMediator.Object);
            AuditTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardAuditCreateEventHandler(null));
        }

        [Fact]
        public async Task HandlerShouldSuccessWithNoError()
        {
            var eventRequest = TestDataProvider.GetTestData<AwardAuditCreateEvent>("AuditMS\\AwardAuditCreateEvent");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationError()
        {
            var eventRequest = TestDataProvider.GetTestData<AwardAuditCreateEvent>("AuditMS\\AwardAuditCreateEventInvalid");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }
    }
}

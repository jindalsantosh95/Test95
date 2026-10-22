using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.PreferenceAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Preference
{
    public class PreferenceAuditCreateEventHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        private readonly PreferenceAuditCreateEventHandler _handler;

        public PreferenceAuditCreateEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new PreferenceAuditCreateEventHandler(_mockMediator.Object);
            AuditTestHelpers.CreateTestEnvironmentSettings();

            System.Environment.SetEnvironmentVariable(VAA.EAPI.Microservices.Loyalty.Audit.Constants.Environment.PreferenceAuditDocType, "PreferenceAudit");
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditCreateEventHandler(null));
        }

        [Fact]
        public async Task HandlerShouldSuccessWithNoError()
        {
            var eventRequest = TestDataProvider.GetTestData<PreferenceAuditCreateEvent>("AuditMS\\PreferenceAudit\\PreferenceAuditCreateEvent");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationError()
        {
            var eventRequest = TestDataProvider.GetTestData<PreferenceAuditCreateEvent>("AuditMS\\PreferenceAudit\\PreferenceAuditCreateEventInvalid");

            var response = await _handler.Handle(eventRequest, CancellationToken.None);

            Assert.True(response.IsSuccess);
        }

        [Fact]
        public void PreferenceAuditDTO_ShouldMapPreferredPartnerFFP()
        {
            var auditDoc = new PreferenceAudit
            {
                CustomerId = "customer-123",
                PreferenceUpdatedBy = "WEB-AXIS",
                PreferredPartnerFFP = "AF"
            };

            var dto = new PreferenceAuditDTO(auditDoc);

            Assert.Equal("AF", dto.PreferredPartnerFFP);
            Assert.Equal("customer-123", dto.CustomerId);
            Assert.Equal("PreferenceAudit", dto.DocType);
        }

        [Fact]
        public void PreferenceAuditDTO_ShouldKeepPreferredPartnerFFPEmptyForLegacyEvents()
        {
            var auditDoc = new PreferenceAudit
            {
                CustomerId = "customer-123",
                PreferenceUpdatedBy = "WEB-AXIS"
            };

            var dto = new PreferenceAuditDTO(auditDoc);

            Assert.Equal(string.Empty, dto.PreferredPartnerFFP);
            Assert.Equal("customer-123", dto.CustomerId);
            Assert.Equal("PreferenceAudit", dto.DocType);
        }
    }
}

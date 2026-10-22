using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Preference;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Preference
{
    public class PreferenceHistoryQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public PreferenceHistoryQueryHandlerTest()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            _mockMediator.Setup(s => s.Send(It.IsAny<PreferenceAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PreferenceAudit>());
            PreferenceAuditHistoryQueryHandler handler = new PreferenceAuditHistoryQueryHandler(_mockMediator.Object);

            PreferenceAuditHistoryQuery query = new PreferenceAuditHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<PreferenceAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            PreferenceAuditHistoryQueryHandler handler = new PreferenceAuditHistoryQueryHandler(_mockMediator.Object);
            PreferenceAuditHistoryQuery query = new PreferenceAuditHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }


        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<PreferenceAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            PreferenceAuditHistoryQueryHandler handler = new PreferenceAuditHistoryQueryHandler(_mockMediator.Object);
            PreferenceAuditHistoryQuery query = new PreferenceAuditHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }
    }
}

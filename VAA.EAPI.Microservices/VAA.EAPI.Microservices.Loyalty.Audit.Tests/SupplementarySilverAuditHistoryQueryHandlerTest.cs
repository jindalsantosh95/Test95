using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.CustomerProfile
{
    public class SupplementarySilverAuditHistoryQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public SupplementarySilverAuditHistoryQueryHandlerTest()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            _mockMediator.Setup(s => s.Send(It.IsAny<SupplementarySilverAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<LoyaltyGenericAudit>());
            SupplementarySilverAuditHistoryQueryHandler handler = new SupplementarySilverAuditHistoryQueryHandler(_mockMediator.Object);

            SupplementarySilverAuditHistoryQuery query = new SupplementarySilverAuditHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<SupplementarySilverAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<LoyaltyGenericAudit>());
            SupplementarySilverAuditHistoryQueryHandler handler = new SupplementarySilverAuditHistoryQueryHandler(_mockMediator.Object);
            SupplementarySilverAuditHistoryQuery query = new SupplementarySilverAuditHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<SupplementarySilverAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<LoyaltyGenericAudit>());
            SupplementarySilverAuditHistoryQueryHandler handler = new SupplementarySilverAuditHistoryQueryHandler(_mockMediator.Object);
            SupplementarySilverAuditHistoryQuery query = new SupplementarySilverAuditHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }
    }
}
using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Award;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Award
{
    public class AwardAuditHistoryQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public AwardAuditHistoryQueryHandlerTest()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            _mockMediator.Setup(s => s.Send(It.IsAny<AwardAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AwardAudit>());
            AwardAuditHistoryQueryHandler handler = new AwardAuditHistoryQueryHandler(_mockMediator.Object);

            AwardAuditHistoryQuery query = new AwardAuditHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<AwardAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            AwardAuditHistoryQueryHandler handler = new AwardAuditHistoryQueryHandler(_mockMediator.Object);
            AwardAuditHistoryQuery query = new AwardAuditHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<AwardAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            AwardAuditHistoryQueryHandler handler = new AwardAuditHistoryQueryHandler(_mockMediator.Object);
            AwardAuditHistoryQuery query = new AwardAuditHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }
    }
}
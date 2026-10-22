using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.CustomerProfile
{
    public class CustomerProfileAuditHistoryQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public CustomerProfileAuditHistoryQueryHandlerTest()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerProfileAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CustomerProfileAudit>());
            CustomerProfileAuditHistoryQueryHandler handler = new CustomerProfileAuditHistoryQueryHandler(_mockMediator.Object);

            CustomerProfileAuditHistoryQuery query = new CustomerProfileAuditHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerProfileAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            CustomerProfileAuditHistoryQueryHandler handler = new CustomerProfileAuditHistoryQueryHandler(_mockMediator.Object);
            CustomerProfileAuditHistoryQuery query = new CustomerProfileAuditHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerProfileAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            CustomerProfileAuditHistoryQueryHandler handler = new CustomerProfileAuditHistoryQueryHandler(_mockMediator.Object);
            CustomerProfileAuditHistoryQuery query = new CustomerProfileAuditHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
        }
    }
}
using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.AmexCenturion;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.AmexCenturion
{
    public class AmexCenturionAuditHistoryQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public AmexCenturionAuditHistoryQueryHandlerTests()
        {
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            // Arrange
            string customerID = "CustID";
            var response = TestDataProvider.GetTestData<List<CardLinkingAudit>>("AuditMS\\AmexCenturion\\AmexCenturionAuditHistory");
                                                                            
            _mockMediator.Setup(s => s.Send(It.IsAny<AmexCenturionAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            AmexCenturionAuditHistoryQueryHandler handler = new AmexCenturionAuditHistoryQueryHandler(_mockMediator.Object);

            AmexCenturionAuditHistoryQuery query = new AmexCenturionAuditHistoryQuery(customerID);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(response.First().CardLinkingUpdatedBy, result.First().CardLinkingUpdatedBy);
            Assert.Equal(response.First().CardLinkingDocID, result.First().CardLinkingDocID);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Null_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<AmexCenturionAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            AmexCenturionAuditHistoryQueryHandler handler = new AmexCenturionAuditHistoryQueryHandler(_mockMediator.Object);
            AmexCenturionAuditHistoryQuery query = new AmexCenturionAuditHistoryQuery(null);

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
            Assert.Equal("CustomerId", result.ParamName);
            Assert.Equal("Value cannot be null. (Parameter 'CustomerId')", result.Message);
        }

        [Fact]
        public async Task Handle_ThrowException_On_Empty_CustomerId()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<AmexCenturionAuditHistoryRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());
            AmexCenturionAuditHistoryQueryHandler handler = new AmexCenturionAuditHistoryQueryHandler(_mockMediator.Object);
            AmexCenturionAuditHistoryQuery query = new AmexCenturionAuditHistoryQuery("");

            // Act
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.NotNull(result.Message);
            Assert.Equal("CustomerId", result.ParamName);
            Assert.Equal("Value cannot be null. (Parameter 'CustomerId')", result.Message);
        }
    }
}

using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SupplementarySilverAccountQueryTest
    {
        private readonly Mock<ISupplementarySilverAuditHistoryQueryProvider> _auditHistoryQueryProvider;
        private readonly SupplementarySilverAccountQuery _supplementarySilverAccountQuery;
        private readonly Mock<ISupplementarySilverAccountQueryProvider> _provider;
        private readonly Mock<ICustomerProvider> _mockCustomerService;

        public SupplementarySilverAccountQueryTest()
        {
            _supplementarySilverAccountQuery = new SupplementarySilverAccountQuery();
            _provider = new Mock<ISupplementarySilverAccountQueryProvider>();
            _auditHistoryQueryProvider = new Mock<ISupplementarySilverAuditHistoryQueryProvider>();
            _mockCustomerService = new Mock<ICustomerProvider>();
        }

        [Fact]
        public async Task SupplementarySilverAccountAsync_ThrowException_NullReferences()
        {
            // Null cust ID or Memb ID
            await Assert.ThrowsAsync<ArgumentException>(() => _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(null, null, _mockCustomerService.Object, _provider.Object, _auditHistoryQueryProvider.Object));
            // Null Customer service
            await Assert.ThrowsAsync<ArgumentNullException>(() => _supplementarySilverAccountQuery.SupplementarySilverAccountAsync("CustomerId", null, null, _provider.Object, _auditHistoryQueryProvider.Object));
            // Null query provider
            await Assert.ThrowsAsync<ArgumentNullException>(() => _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(null, "MemberId", _mockCustomerService.Object, null, _auditHistoryQueryProvider.Object));
            //Null audit history provider
            await Assert.ThrowsAsync<ArgumentNullException>(() => _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(null, "MemberId", _mockCustomerService.Object, _provider.Object, null));
        }

        [Fact]
        public async Task SupplementarySilverAccountAsync_ShouldSucceed_WithMembId()
        {

            // Arrange
            var MembID = "1000000000";
            var CustomerId = "1000000000";
            var cust = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerService.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);
            _provider.Setup(n => n.Query(It.IsAny<string>())).ReturnsAsync(new SupplementarySilverAccountResponse());

            var auditHistoryResponse = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            _auditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(It.IsAny<string>())).ReturnsAsync(auditHistoryResponse);

            // Act
            var testOutput = await _supplementarySilverAccountQuery.SupplementarySilverAccountAsync("", MembID, _mockCustomerService.Object, _provider.Object, _auditHistoryQueryProvider.Object);

            // Assert
            Assert.NotNull(testOutput);
        }

        [Fact]
        public async Task SupplementarySilverAccountAsync_ShouldSucceed_WithCustomerId()
        {

            // Arrange
            var CustomerId = "1000000000";
            var cust = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerService.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);
            _provider.Setup(n => n.Query(It.IsAny<string>())).ReturnsAsync(new SupplementarySilverAccountResponse());

            var auditHistoryResponse = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            _auditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(It.IsAny<string>())).ReturnsAsync(auditHistoryResponse);

            // Act
            var testOutput = await _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(CustomerId, null, _mockCustomerService.Object, _provider.Object, _auditHistoryQueryProvider.Object);

            // Assert
            Assert.NotNull(testOutput);
        }

        [Fact]
        public async Task SupplementarySilverAccountAsync_ShouldSucceed_WithVaildHeadeMember_Response()
        {
            // Arrange
            var CustomerId = "1000000000";
            var cust = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerService.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);

            var auditHistoryResponse = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            _auditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(It.IsAny<string>())).ReturnsAsync(auditHistoryResponse);

            SupplementarySilverMemberAccount headMember = new()
            {
                MemberNumber = "001",
                CustomerId = "001",
                AssociationDate = null,
                MemberStatus = "Active",
                FirstName = "First",
                LastName = "Last",
                RelationShipType = "Linked Gold"
            };
            SupplementarySilverAccountResponse response = new()
            {
                SsaHeadMember = headMember
            };
            _provider.Setup(n => n.Query(It.IsAny<string>())).ReturnsAsync(response);

            // Act
            SupplementarySilverAccountResponse? testOutput = await _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(CustomerId, null, _mockCustomerService.Object, _provider.Object, _auditHistoryQueryProvider.Object);

            // Assert
            Assert.NotNull(testOutput);
            Assert.NotNull(testOutput.SsaHeadMember);
        }

        [Fact]
        public async Task SupplementarySilverAccountAsync_ShouldSucceed_WithVaildChildMember_Response()
        {
            // Arrange
            var CustomerId = "1000000000";
            var cust = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _mockCustomerService.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);

            var auditHistoryResponse = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            _auditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(It.IsAny<string>())).ReturnsAsync(auditHistoryResponse);

            SupplementarySilverMemberAccount childMember = new()
            {
                MemberNumber = "001",
                CustomerId = "001",
                AssociationDate = null,
                MemberStatus = "Active",
                FirstName = "First",
                LastName = "Last",
                RelationShipType = "Supp Silver"
            };
            SupplementarySilverAccountResponse response = new()
            {
                SsaChildMember = childMember
            };
            _provider.Setup(n => n.Query(It.IsAny<string>())).ReturnsAsync(response);

            // Act
            SupplementarySilverAccountResponse? testOutput = await _supplementarySilverAccountQuery.SupplementarySilverAccountAsync(CustomerId, null, _mockCustomerService.Object, _provider.Object, _auditHistoryQueryProvider.Object);

            // Assert
            Assert.NotNull(testOutput);
            Assert.NotNull(testOutput.SsaChildMember);
            Assert.Null(testOutput.SsaHeadMember);
        }
    }
}
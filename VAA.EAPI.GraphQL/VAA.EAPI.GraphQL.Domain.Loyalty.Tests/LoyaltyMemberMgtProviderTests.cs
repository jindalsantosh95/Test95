using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class LoyaltyMemberMgtProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public LoyaltyMemberMgtProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new LoyaltyMemberMgtProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new LoyaltyMemberMgtProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GenerateStatetmentTextShouldSucceed()
        {
            var stmtRequest = TestDataProvider.GetTestData<TransactionStatementTextRequest>("Loyalty\\TransactionStatementTextRequest");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty\\TransactionStatementTextAPIResponse");

            _mockConnector.Setup(m => m.SendAsync<TransactionStatementTextResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).ReturnsAsync(stmtResponse);

            var provider = new LoyaltyMemberMgtProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var response = await provider.GenerateStatetmentText(stmtRequest);

            Assert.Equal("Point credit for reward voucher redemption", response.StatementText);
        }
    }
}

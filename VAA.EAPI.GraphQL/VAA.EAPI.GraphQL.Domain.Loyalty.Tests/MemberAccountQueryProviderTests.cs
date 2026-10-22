using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberAccountQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mockTokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        IMemberAccountQueryProvider memberAccountService;
        public MemberAccountQueryProviderTests()
        {
            _mockTokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberAccountQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberAccountQueryProvider(_mockTokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<MemberAccountResponse> response = TestDataProvider.GetTestData<APIResponse<MemberAccountResponse>>("MemberAccount\\MemberAccount_APIResponse");
            _mockTokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<MemberAccountResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);
            var provider = new MemberAccountQueryProvider(_mockTokenProvider.Object, _mockConnector.Object);
            var customerId = "D4718748-3560-E0ED-E053-6941D90AAD4B";
            var queryResponse = await provider.Query(customerId);
            Assert.IsType<MemberAccount>(queryResponse);
        }
    }
}

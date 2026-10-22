using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class VredMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public VredMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new VredMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new VredMutationProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task CreateTransactionShouldSucceed()
        {
            var vredCreateRequest = TestDataProvider.GetTestData<VredSyncTransactionRequest>("Loyalty\\Transaction\\VredSyncTransactionRequest");
            var vredCreateResponse = TestDataProvider.GetTestData<APIResponse<VRedTransactionResponse>>("Loyalty\\Transaction\\VredSync_APIResponse");

            _mockConnector.Setup(m => m.SendAsync<VRedTransactionResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).ReturnsAsync(vredCreateResponse);

            var provider = new VredMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.PostTransactionToVred(vredCreateRequest);

            Assert.NotNull(queryResponse);
            Assert.Equal(Status.OK, queryResponse.Status);
            Assert.Contains("171871054135506012", queryResponse.Value);
        }
    }
}

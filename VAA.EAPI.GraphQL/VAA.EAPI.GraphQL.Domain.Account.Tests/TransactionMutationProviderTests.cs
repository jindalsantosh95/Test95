using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class TransactionMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mockTokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly TransactionMutationProvider _mutationProvider;
        private readonly Mock<IRefDataService<ProductDetails>> _mockproductRefService;
        private readonly Mock<IRefDataService<PartnerProfileDetails>> _mockpartnerProfileService;

        public TransactionMutationProviderTests()
        {
            _mockTokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mockproductRefService = new Mock<IRefDataService<ProductDetails>>();
            _mockpartnerProfileService = new Mock<IRefDataService<PartnerProfileDetails>>();
            _mutationProvider = new TransactionMutationProvider(_mockTokenProvider.Object, _mockConnector.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(null, _mockConnector.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(_mockTokenProvider.Object, null, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(_mockTokenProvider.Object, _mockConnector.Object, null, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(_mockTokenProvider.Object, _mockConnector.Object, _mockproductRefService.Object, null));
        }

        [Fact]
        public async Task CreateEarnTransaction_ApiCallSuccess()
        {
            //Arrange            
            var accrualRequest = TestDataProvider.GetTestData<CreateTransactionRequest>("Account//AccrualRequest");
            var response = TestDataProvider.GetTestData<APIResponse<VredTransactionResponse>>("Account//CreateTransactionAPIResponse");

            _mockTokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.SendAsync<VredTransactionResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(response));

            //Act
            var result = await _mutationProvider.CreateEarnTransaction(accrualRequest);

            var vreTransactionDetails = Json.Deserialize<VredTransactionSuccessResponse>(result.Value);

            //Assert
            Assert.Equal("AZ1709894001363", vreTransactionDetails.PartnerTransactionId);
            Assert.Equal("VAA", vreTransactionDetails.PartnerCode);
            Assert.Equal("2105128793", vreTransactionDetails.PartnerUserId);
        }

        [Fact]
        public async Task CreateBurnTransaction_ApiCallSuccess()
        {
            //Arrange           
            var redemptionRequest = TestDataProvider.GetTestData<CreateTransactionRequest>("Account//RedemptionRequest");
            var response = TestDataProvider.GetTestData<APIResponse<VredTransactionResponse>>("Account//CreateTransactionAPIResponse");

            _mockTokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.SendAsync<VredTransactionResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(response));

            //Act
            var result = await _mutationProvider.CreateBurnTransaction(redemptionRequest);

            var vreTransactionDetails = Json.Deserialize<VredTransactionSuccessResponse>(result.Value);

            //Assert
            Assert.Equal("AZ1709894001363", vreTransactionDetails.PartnerTransactionId);
            Assert.Equal("VAA", vreTransactionDetails.PartnerCode);
            Assert.Equal("2105128793", vreTransactionDetails.PartnerUserId);
        }
    }
}

using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class VredPointProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mockTokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly VredPointProvider _mutationProvider;
        private readonly Mock<IRefDataService<ProductDetails>> _mockproductRefService;
        private readonly Mock<IRefDataService<PartnerProfileDetails>> _mockpartnerProfileService;

        public VredPointProviderTests()
        {
            _mockTokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mockproductRefService = new Mock<IRefDataService<ProductDetails>>();
            _mockpartnerProfileService = new Mock<IRefDataService<PartnerProfileDetails>>();
            _mutationProvider = new VredPointProvider(_mockTokenProvider.Object, _mockConnector.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(null, _mockConnector.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutationProvider(_mockTokenProvider.Object, null, _mockproductRefService.Object, _mockpartnerProfileService.Object));
        }

        [Fact]
        public async Task GetVredPointBalance_ApiCallSuccess()
        {
            //Arrange
            var vredPointResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");

            _mockTokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.GetAsync<VRedResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(vredPointResponse);

            //Acts
            var result = await _mutationProvider.VredPointBalance("1234567890");

            //Assert
            Assert.NotNull(result.Usable);
        }
    }
}

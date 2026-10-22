using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class ProductsQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly Mock<ICacheService> _mockCache;

        public ProductsQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mockCache = new Mock<ICacheService>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductsQueryProvider(null, _mockConnector.Object, _mockCache.Object));
            Assert.Throws<ArgumentNullException>(() => new ProductsQueryProvider(_mocktokenProvider.Object, null, _mockCache.Object));
            Assert.Throws<ArgumentNullException>(() => new ProductsQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, null));
        }

        [Fact]
        public async Task ProductRetrievalShouldSucceedWhenNotCached()
        {
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<IEnumerable<ProductDetails>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(productResponse);

            _mockCache.Setup(x => x.GetCacheValue<IEnumerable<ProductDetails>>(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<ProductDetails>>(null));
            _mockCache.Setup(x => x.SetCacheValue(It.IsAny<string>(), It.IsAny<IEnumerable<ProductDetails>>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var provider = new ProductsQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, _mockCache.Object);
            var queryResponse = await provider.GetProductDetails();

            Assert.NotNull(queryResponse);
            Assert.Equal(productResponse.Response.FirstOrDefault()?.RowId, queryResponse.FirstOrDefault()?.RowId);
            Assert.Equal(productResponse.Response.FirstOrDefault()?.ProductName, queryResponse.FirstOrDefault()?.ProductName);
        }

        [Fact]
        public async Task ProductRetrievalShouldSucceedWhenCached()
        {
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<IEnumerable<ProductDetails>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(productResponse);

            _mockCache.Setup(x => x.GetCacheValue<IEnumerable<ProductDetails>>(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<ProductDetails>>(productResponse.Response));
            _mockCache.Setup(x => x.SetCacheValue(It.IsAny<string>(), It.IsAny<IEnumerable<ProductDetails>>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var provider = new ProductsQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, _mockCache.Object);
            var queryResponse = await provider.GetProductDetails();

            Assert.NotNull(queryResponse);
            Assert.Equal(productResponse.Response.FirstOrDefault()?.RowId, queryResponse.FirstOrDefault()?.RowId);
            Assert.Equal(productResponse.Response.FirstOrDefault()?.ProductName, queryResponse.FirstOrDefault()?.ProductName);
        }
    }
}

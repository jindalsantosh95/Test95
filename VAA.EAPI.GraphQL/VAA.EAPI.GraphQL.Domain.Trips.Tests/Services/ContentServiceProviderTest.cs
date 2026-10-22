using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services
{
    public class ContentServiceProviderTest
    {
        private readonly IServiceProvider _serviceProvider;
        public ContentServiceProviderTest()
        {
            ConfigReader.Set(Common.Constants.Environment.ContentServiceUrl, "https://Testresources.virginatlantic.com/DEV/content/");
            ConfigReader.Set(Common.Constants.Environment.ContentServiceOcp, "ocp-key");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IContentServiceProvider, ContentServiceProvider>()
                                                                                                         .BuildServiceProvider();
        }

        [Fact]
        public async Task GetContentAsync_ReturnsContent()
        {
            // Arrange
            var contentQueryProvider = _serviceProvider.GetService<IContentServiceProvider>()!;

            //Act
            var result = await contentQueryProvider.GetContentDetails("fragment123");

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
        }
    }
}

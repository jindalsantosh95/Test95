using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Content;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class ContentQueryTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ContentQueryTests()
        {
            ConfigReader.Set(Common.Constants.Environment.ContentServiceUrl, "https://Testresources.virginatlantic.com/DEV/content/");
            ConfigReader.Set(Common.Constants.Environment.ContentServiceOcp, "ocp-key");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IContentServiceProvider, ContentServiceProvider>()
                                                      .AddSingleton<ContentQuery>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task GetContentDetails_WithContentFragmentId_CallsConnectorAndReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;
            string contentFragmentId = "fragment123";

            // Act
            var result = await contentQuery.GetContentFragment(contentFragmentId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(61, result.Count());
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_WithContentRequest_Constructs_HotelQueryParamsAndReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.HOTEL,
                    ContentId = "123",
                    BrandCode = BrandType.WW
                }
            };

            // Act
            var result = await contentQuery.GetContentFragment(null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_WithContentRequest_Constructs_FragmentQueryParamsAndReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = null,
                Detail = new ContentDetail
                {
                    ContentFragmentName = "fragment123",
                }
            };

            // Act
            var result = await contentQuery.GetContentFragment(null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_WithContentRequest_Constructs_WithoutBrandCode_ReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.HOTEL,
                    ContentId = "123",
                }
            };

            // Act
            var result = await contentQuery.GetContentFragment(null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_WithContentRequest_Constructs_CarQueryParamsAndReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.CAR,
                    ContentId = null,
                    BrandCode = BrandType.WW,
                    SupplierName = "hertz",
                    SippCode = "CCAR"
                }
            };

            // Act
            var result = await contentQuery.GetContentFragment(null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_WithContentID_Constructs_CarQueryParamsAndReturnsResponse()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.CAR,
                    ContentId = "test123",
                    BrandCode = BrandType.WW,
                    SupplierName = null,
                    SippCode = null
                }
            };

            // Act
            var result = await contentQuery.GetContentFragment(null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetContentDetails_Without_CarContentID_SupplierName_ThrowsValidationException()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.CAR,
                    ContentId = null,
                    BrandCode = BrandType.WW,
                    SupplierName = null,
                    SippCode = "123"
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RequestValidationException>(() => contentQuery.GetContentFragment(null, request));
            Assert.Equal("SupplierName and SippCode are required when ContentId is not provided.", ex.Message);
        }


        [Fact]  
        public async Task GetContentDetails_Without_CarContentID_SippCode_ThrowsValidationException()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.CAR,
                    ContentId = null,
                    BrandCode = BrandType.WW,
                    SupplierName = "123",
                    SippCode = null
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RequestValidationException>(() => contentQuery.GetContentFragment(null, request));
            Assert.Equal("SupplierName and SippCode are required when ContentId is not provided.", ex.Message);
        }

        [Fact]
        public async Task GetContentDetails_With_ContentID_Zero_Without_SupplierName_ThrowsValidationException()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            var request = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentType = ContentType.CAR,
                    ContentId = "0",
                    BrandCode = BrandType.WW,
                    SupplierName = null,
                    SippCode = "123"
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RequestValidationException>(() => contentQuery.GetContentFragment(null, request));
            Assert.Equal("SupplierName and SippCode are required when ContentId is not provided.", ex.Message);
        }

        [Fact]
        public async Task ContentQuery_ContentRequest_Null_ThrowsRequestValidationException()
        {
            // Arrange
            var contentQuery = _serviceProvider.GetService<ContentQuery>()!;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RequestValidationException>(() => contentQuery.GetContentFragment(null, null));
            Assert.Equal("ContentRequest is required.", ex.Message);
        }


    }
}

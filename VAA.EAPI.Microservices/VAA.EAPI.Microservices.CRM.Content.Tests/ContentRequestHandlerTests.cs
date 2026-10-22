using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Threading;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Content.Tests
{
    public class ContentRequestHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ContentRequestHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(Constants.Environment.AemGraphEndpointUri, "https://test1-vaahome.webdev.vholsinternal.co.uk/graphql/execute.json/vaa-home/");
            ConfigReader.Set(Constants.Environment.VholsEndPointUri, "https://vsolpublic-test1.webdev.vholsinternal.co.uk/content-api/v3/");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IContentRequestHandler, ContentRequestHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_AEM_Content_Should_Success()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();
            var contentRequest = new ContentRequest
            {
                SourceType = SourceType.AEM,
                Detail = new ContentDetail
                {
                    ContentFragmentName = "vaa-sales-banners"
                }
            };

            /* Act */
            var response = await contentRequestHandler.Handle(contentRequest, CancellationToken.None);

            /* Assert */
            Assert.NotNull(response);
            Assert.NotNull(response["items"]);
            Assert.True(response["items"] is JArray);
        }

        [Fact]
        public async Task Handle_VHOLS_HotelContent_Should_Success()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();
            var contentRequest = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentId = "H1126",
                    BrandCode = BrandType.WW,
                    ContentType = ContentType.HOTEL
               }
            };

            /* Act */
            var response = await contentRequestHandler!.Handle(contentRequest, CancellationToken.None);

            /* Assert */
            Assert.NotNull(response);
            Assert.Equal("H1126", response.SelectToken("items").First()["tbxId"].ToString());
            Assert.Equal("Sandals Halcyon Beach St. Lucia", response.SelectToken("items").First()["name"].ToString());
        }

        [Fact]
        public async Task Handle_VHOLS_CarContent_WithId_Should_Success()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();
            var contentRequest = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentId = "4176",
                    BrandCode = BrandType.WW,
                    ContentType = ContentType.CAR
                }
            };

            /* Act */
            var response = await contentRequestHandler!.Handle(contentRequest, CancellationToken.None);

            /* Assert */
            Assert.NotNull(response);
            Assert.Equal("4176", response.SelectToken("items").First()["tbxId"].ToString());
            Assert.Equal("Nissan Versa", response.SelectToken("items").First()["model"].ToString());
        }

        [Fact]
        public async Task Handle_VHOLS_CarContent_WithoutId_Should_Success()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();
            var contentRequest = new ContentRequest
            {
                SourceType = SourceType.VHOLS,
                Detail = new ContentDetail
                {
                    ContentId = "hertz_CCAR",
                    BrandCode = BrandType.WW,
                    ContentType = ContentType.CAR,
                }
            };

            /* Act */
            var response = await contentRequestHandler!.Handle(contentRequest, CancellationToken.None);

            /* Assert */
            Assert.NotNull(response);
            Assert.Equal("hertz_CCAR", response.SelectToken("items").First()["tbxId"].ToString());
            Assert.Equal("Nissan Versa", response.SelectToken("items").First()["model"].ToString());
        }

        [Fact]
        public async Task Handle_When_ContentRequest_Is_Empty_Should_Throw()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();

            /* Act / Assert */
            await Assert.ThrowsAsync<ArgumentNullException>(() => contentRequestHandler!.Handle((ContentRequest)null!, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_When_SourceType_Unsupported_Should_Throw()
        {
            /* Arrange */
            var contentRequestHandler = _serviceProvider.GetService<IContentRequestHandler>();

            var contentRequest = new ContentRequest
            {
                SourceType = (SourceType)999,
                Detail = new ContentDetail
                {
                    ContentFragmentName = "any"
                }
            };

            /* Act / Assert */
            await Assert.ThrowsAsync<InvalidOperationException>(() => contentRequestHandler!.Handle(contentRequest, CancellationToken.None));
        }
    }
}

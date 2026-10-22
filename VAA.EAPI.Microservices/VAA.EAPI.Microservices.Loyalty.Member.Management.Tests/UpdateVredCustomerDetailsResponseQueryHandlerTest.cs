using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class UpdateVredCustomerDetailsResponseQueryHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        public UpdateVredCustomerDetailsResponseQueryHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("vredmssubscriptionkey", "xxx");
            ConfigReader.Set("Vred:AppId", "xxx");
            ConfigReader.Set("UpdateVredCustomerDetailsResponseUrl", "https://tempuri.org");
            ConfigReader.Set("LoyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("LoyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("LoyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("AzureADEndpoint", "https://dummy.com/123/");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IVredCustomerDetailsResponseQuery, UpdateVredCustomerDetailsResponseQueryHandler>()

                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task UpdateVredCustomerQueryHandler_ValidScenario_ThrowsExpectedException()
        {
            var handler = _serviceProvider.GetService<IVredCustomerDetailsResponseQuery>()!;
            var request = new UpdateVredCustomerDetailsResponseQuery { CorrelationId = " " };

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateVredCustomerQueryHandler_ValidScenario()
        {
            var handler = _serviceProvider.GetService<IVredCustomerDetailsResponseQuery>()!;
            var request = new UpdateVredCustomerDetailsResponseQuery { CorrelationId = "9bc02ced-91d6-49fd" };

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Completed", result.UpdateVredCustomerDetailsResponse.ProcessingStatus);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task UpdateVredCustomerQueryHandler_BadRequest()
        {
            var handler = _serviceProvider.GetService<IVredCustomerDetailsResponseQuery>()!;
            var Request = new UpdateVredCustomerDetailsResponseQuery { CorrelationId = "1235678909" };

            var result = await handler.Handle(Request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Null((result.UpdateVredCustomerDetailsResponse));
            Assert.Equal("BadRequest", result.Error.FirstOrDefault().Code);
            Assert.NotNull(result.Error);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Tests
{
    public class CheckQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CheckQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ArcCheckBaseUrl", "https://tempuri.org/container/CheckAgentStatus");
            ConfigReader.Set("ArcCheckClientId", "abc");
            ConfigReader.Set("ArcCheckClientSecret", "xxx");
            ConfigReader.Set("ArcCheckTokenUrl", "https://tempuri.org/ArcCheckToken");
            ConfigReader.Set("ArcCheckScope", "arcCheckScope");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<CheckStatusQueryHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<CheckStatusQueryHandler>()!;
            
            var agencyNumber = "12345678";

            var result = await handler.Handle(agencyNumber);
            var response = result.Value.Value;

            Assert.NotNull(response);

            Assert.Equal("Success", response.TransactionMetadata.Message);
            Assert.Equal("Ticketing On", response.TransactionResponse.Ticketing.First().Airline.TicketingStatus);
        }
    }
}
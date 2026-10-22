using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.ARCPaymentMS
{
    public class CheckAgentStatusTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CheckAgentStatusTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ARCPaymentMSUrl", "https://tempuri.org/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "abc");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<CheckAgentStatusQueryHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<CheckAgentStatusQueryHandler>()!;

            var agencyNumber = "12345678";

            var response = await handler.Handle(agencyNumber);
            
            Assert.NotNull(response);

            Assert.Equal("Success", response.TransactionMetadata.Message);
            Assert.Equal("Ticketing On", response.TransactionResponse.Ticketing.First().Airline.TicketingStatus);
        }
    }
}

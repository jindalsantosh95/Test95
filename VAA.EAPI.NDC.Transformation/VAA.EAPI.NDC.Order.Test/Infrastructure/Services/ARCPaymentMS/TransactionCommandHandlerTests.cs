using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.ARCPaymentMS
{
    public class TransactionCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ARCPaymentMSUrl", "https://tempuri.org/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "abc");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<TransactionCommandHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<TransactionCommandHandler>()!;

            var response = await handler.Handle(new Order.Infrastructure.Model.ARCPaymentMS.TransactionServiceRequest());

            Assert.NotNull(response);

            Assert.Equal("SUCCESS", response.Status);
            Assert.Equal("5e7bb423-ff1b-4784-81cf-7daa5b39b0cd", response.RequestId);
        }
    }
}

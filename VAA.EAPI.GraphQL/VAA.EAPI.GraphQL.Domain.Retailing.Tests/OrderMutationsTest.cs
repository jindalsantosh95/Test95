using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Retailing.Mutations;
using VAA.EAPI.GraphQL.Domain.Retailing.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Tests
{
    public class OrderMutationsTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderMutationsTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("OrderDataProvider:MsUrl", "https://tempuri.org/orderdata");
            ConfigReader.Set("OrderDataProvider:OcpKey", "12345678");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOrderDataProvider, OrderDataProvider>()
                                                      .AddSingleton<OrderMutations>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Execute_OrderMutations_UpdateSSRs()
        {
            var orderMutations = _serviceProvider.GetService<OrderMutations>()!;

            var request = new Models.OrderMutation.OrderMutationRequest()
            {
                ShipmentRequestId = "TestShipmentRequestId"
            };

            var result = await orderMutations.UpdateSSRs(request);

            Assert.NotNull(result);
            Assert.Equal("Confirmed", result.Status);
            Assert.Equal("1234567890", result.ConfirmationNumber);
        }
    }
}
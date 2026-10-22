using HotChocolate;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Mutations;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests
{
    public class ClientIpFallbackTests
    {
        public static IEnumerable<object?[]> Cases()
        {
            foreach (var order in new[] { true, false })
            {
                yield return [order, null, " 203.0.113.5 , 198.51.100.7 ", "203.0.113.5"];
                yield return [order, "", " 203.0.113.5 , 198.51.100.7 ", "203.0.113.5"];
                yield return [order, " \t ", " , 203.0.113.5 , 198.51.100.7 ", "203.0.113.5"];
                yield return [order, "192.0.2.9", "203.0.113.5", "192.0.2.9"];
                yield return [order, "192.0.2.9", null, "192.0.2.9"];
                yield return [order, null, null, ""];
                yield return [order, "", "", ""];
                yield return [order, " \t ", " , , ", ""];
            }
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Mutation_ResolvesClientIpBeforeDownstreamWork(
            bool order, string? suppliedIp, string? forwardedHeader, string expectedIp)
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            var httpRequest = await AsyncContext.Current.Context.GetHttpRequestDataAsync();
            httpRequest!.Headers.Remove("X-Forwarded-For");
            if (forwardedHeader is not null)
                httpRequest.Headers.Add("X-Forwarded-For", forwardedHeader);

            ConfigReader.Set(Constants.Environment.BasketMSRetrieveUrl, "https://tempuri.org/client-ip/basket/{basket}");
            ConfigReader.Set(Constants.Environment.BasketMSOcpKey, "test-key");
            ConfigReader.Set(Constants.Environment.DelayTimeInMS, "0");
            ConfigReader.Set(Constants.Application.VoucherRedemptionFeatureFlag, "false");

            using var services = CreateServices();
            string? actualIp;
            // Stop at the first HTTP dependency to isolate boundary normalization from booking orchestration.
            // Reaching the same mocked failure also pins the existing no-IP behavior: no new rejection.
            if (order)
            {
                var request = new OrderCreateGraphQLRequest
                {
                    BasketId = "ip-fallback",
                    PaymentStatus = "SUCCESS",
                    IpAddress = suppliedIp
                };
                await Assert.ThrowsAsync<HttpRequestException>(() => services.GetRequiredService<OrderMutation>().CreateOrder(request));
                actualIp = request.IpAddress;
            }
            else
            {
                var request = new OrderPaymentRequest { BasketId = "ip-fallback", IpAddress = suppliedIp };
                var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<PaymentMutation>().OrderPayment(request));
                Assert.Equal("500", Assert.Single(error.Errors).Code);
                actualIp = request.IpAddress;
            }

            Assert.Equal(expectedIp, actualIp);
            var factory = (Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
            var response = Assert.Single(factory.GetResponses("https://tempuri.org"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("/client-ip/basket/ip-fallback", response.RequestMessage!.RequestUri!.AbsolutePath);
        }

        private static ServiceProvider CreateServices() => new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDistributedCache, Core.Xunit.Caching.DistributedCache>()
            .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
            .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
            .AddSingleton<IOrderMutationProvider, OrderMutationProvider>()
            .AddSingleton<IOfferServiceProvider, OfferServiceProvider>()
            .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
            .AddSingleton<IRefServiceProvider, ReferenceServiceProvider>()
            .AddSingleton<ISeatServiceProvider, SeatServiceProvider>()
            .AddSingleton<ICustomerProvider, CustomerServiceProvider>()
            .AddSingleton<ICustomerServiceProvider, CustomerServiceProvider>()
            .AddSingleton<IVoucherRedemptionProvider, VoucherRedemptionProvider>()
            .AddSingleton<IAnalyticsService, NoOpAnalytics>()
            .AddSingleton<OrderMutation>()
            .AddSingleton<PaymentMutation>()
            .BuildServiceProvider();

        // Analytics is unrelated to IP normalization; all business HTTP services use the real connector.
        private sealed class NoOpAnalytics : IAnalyticsService
        {
            public Task TrackAnalytics(TrackAnalyticsMessage message) => Task.CompletedTask;
        }
    }
}

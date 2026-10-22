using System;
using System.Net.Http;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.RefData.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.RefData.Tests
{
    public sealed class MobileFeaturesQueryTests
    {
        [Theory]
        [InlineData("configured", "true")]
        [InlineData("empty", null)]
        public async Task QueryReturnsDictionaryAsJsonObject(string scenario, string? expected)
        {
            using var services = CreateServices(scenario);
            var executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
            var result = JObject.Parse((await executor.ExecuteAsync("{ mobileFeatures { features } }")).ToJson());
            Assert.Null(result["errors"]);
            var features = Assert.IsType<JObject>(result["data"]!["mobileFeatures"]!["features"]);
            Assert.Equal(expected, (string?)features["nativeBooking"]);
            if (expected == null)
            {
                Assert.Empty(features);
            }
        }

        [Theory]
        [InlineData("failure")]
        [InlineData("missing")]
        [InlineData("null")]
        [InlineData("not-found")]
        public async Task QueryReportsErrorsInsteadOfReturningConfiguration(string scenario)
        {
            using var services = CreateServices(scenario);
            var executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
            var result = JObject.Parse((await executor.ExecuteAsync("{ mobileFeatures { features } }")).ToJson());
            Assert.NotNull(result["errors"]);
            if (scenario == "not-found")
            {
                Assert.Equal("500", (string?)result["errors"]![0]!["extensions"]!["code"]);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ResolverObservesCancellationWhileAcquiringToken(bool cancelDuringAcquisition)
        {
            using var cancellation = new CancellationTokenSource();
            var pendingToken = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tokenProvider = new TokenProvider { Token = pendingToken.Task };
            using var services = CreateServices("configured", tokenProvider);
            if (!cancelDuringAcquisition)
            {
                cancellation.Cancel();
            }
            var operation = new MobileFeaturesQuery().GetMobileFeaturesAsync(services.GetRequiredService<IRefServiceProvider>(), cancellation.Token);
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
            Assert.Equal(cancelDuringAcquisition, tokenProvider.WasCalled);
            var factory = (VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
            Assert.Empty(factory.GetResponses("https://tempuri.org/mobile-features/configured"));
        }

        [Fact]
        public async Task DownstreamRequestObservesCancellationAfterTokenAcquisition()
        {
            using var cancellation = new CancellationTokenSource();
            var tokenProvider = new TokenProvider { OnAcquire = cancellation.Cancel };
            using var services = CreateServices("configured", tokenProvider);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new MobileFeaturesQuery()
                .GetMobileFeaturesAsync(services.GetRequiredService<IRefServiceProvider>(), cancellation.Token));

            Assert.True(tokenProvider.WasCalled);
            var factory = (VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
            Assert.Empty(factory.GetResponses("https://tempuri.org/mobile-features/configured"));
        }

        private static ServiceProvider CreateServices(string scenario, TokenProvider? tokenProvider = null)
        {
            ConfigReader.Set(Constants.Environment.ReferenceMSMobileFeaturesUrl, $"https://tempuri.org/mobile-features/{scenario}");
            ConfigReader.Set(Constants.Environment.ReferenceMSOcpKey, "test-key");
            ConfigReader.Set(Constants.Environment.ReferenceMSAppId, "test-reference-resource");
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(tokenProvider ?? new TokenProvider())
                .AddSingleton<IRefServiceProvider, ReferenceServiceProvider>();
            services.AddGraphQL().AddQueryType(d => d.Name("Query")).AddTypeExtension<MobileFeaturesQuery>();
            return services.BuildServiceProvider();
        }

        private sealed class TokenProvider : IOAuthTokenProvider
        {
            public Task<string> Token { get; init; } = Task.FromResult("synthetic-token");
            public bool WasCalled { get; private set; }
            public Action? OnAcquire { get; init; }

            public Task<string> AcquireTokenAsync(string resource)
            {
                WasCalled = true;
                OnAcquire?.Invoke();
                Assert.Equal("test-reference-resource", resource);
                return Token;
            }
        }
    }
}

using System;
using System.Net;
using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;
using VAA.EAPI.GraphQL.Framework.Tests;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;
using Settings = VAA.EAPI.GraphQL.Domain.Account.Constants.AddressLookupV3Settings;
using SharedSettings = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class AddressLookupV3ProviderTests
    {
        private const string Endpoint = "https://address-v3.example.test";
        private readonly TestTokenProvider tokenProvider = new();

        public AddressLookupV3ProviderTests()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development");
            ConfigReader.Set("WEBSITE_INSTANCE_ID", "");
            ConfigReader.Set(SharedSettings.EnrolmentMSOcpKey, "existing-enrolment-key");
            ConfigReader.Set(SharedSettings.AddressLookupAPIAppId, "existing-address-app");
            ConfigReader.Set(Settings.AddressLookupV3ApimUrl, Endpoint);
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            AsyncContext.Current.SetSessionId(Guid.Empty);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Operations_UseV2ApplicationTokenAndSubscription_WithoutUserTokenOrCustomContext(bool selection, bool local)
        {
            var endpoint = local ? "http://localhost:7071/api" : Endpoint;
            ConfigReader.Set(Settings.AddressLookupV3ApimUrl, endpoint);
            ConfigReader.Set("AddressLookupV3:ApimSubscriptionKey", "ignored-retired-key");
            using var services = CreateServices();
            var provider = services.GetRequiredService<AddressLookupV3Provider>();

            if (selection)
            {
                var result = await provider.GetSelectionAsync(new() { Reference = "opaque-reference" }, CancellationToken.None);
                Assert.Equal("London", result.Address.City);
                Assert.Equal("test-flow", result.FlowId);
            }
            else
            {
                var result = await provider.GetSuggestionsAsync(new() { Query = "10 Downing Street", CountryCode = "GB" }, CancellationToken.None);
                Assert.Empty(result.Candidates);
                Assert.Equal("test-flow", result.FlowId);
            }

            var request = Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(endpoint)).RequestMessage!;
            Assert.Equal(endpoint + "/v3/address-" + (selection ? "selections" : "suggestions"), request.RequestUri!.AbsoluteUri);
            Assert.Equal("existing-address-app", Assert.Single(tokenProvider.Resources));
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("server-app-token", request.Headers.Authorization.Parameter);
            Assert.Equal("existing-enrolment-key", Assert.Single(request.Headers.GetValues("Ocp-Apim-Subscription-Key")));
            Assert.False(request.Headers.Contains("X-Address-Lookup-V3-Context"));
            Assert.False(request.Headers.Contains("x-functions-key"));
            Assert.Equal("True", Assert.Single(request.Headers.GetValues("x-nowait")));
        }

        [Theory]
        [InlineData("address", " 4 ", " RH10 6DF ", null)]
        [InlineData("postcode", " ", "RH10 6DF", null)]
        [InlineData("line", "123 Test Street", null, null)]
        [InlineData("container", null, null, "opaque-container-reference")]
        public async Task Suggestions_MapStructuredInputOrContainer_WithoutRetrieving(string mode, string? line, string? postcode, string? reference)
        {
            // Each fixture matches the exact combined query or reference in the outbound JSON.
            ConfigReader.Set(Settings.AddressLookupV3ApimUrl, Endpoint + "/" + mode);
            using var services = CreateServices();
            var result = await services.GetRequiredService<AddressLookupV3Provider>().GetSuggestionsAsync(
                new() { AddressLine1 = line, PostCode = postcode, Reference = reference, CountryCode = "GB" }, CancellationToken.None);
            Assert.Equal("test-flow", result.FlowId);
            Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Endpoint));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Suggestions_RejectMixedInputBeforeAuthenticationOrDispatch(bool reference)
        {
            using var services = CreateServices();
            var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<AddressLookupV3Provider>().GetSuggestionsAsync(
                new() { Query = reference ? null : "other address", Reference = reference ? "opaque-reference" : null, PostCode = "RH10 6DF" },
                CancellationToken.None));
            Assert.Equal("400", Assert.Single(error.Errors).Code);
            Assert.Empty(tokenProvider.Resources);
            Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Endpoint));
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public async Task Operations_RejectMissingServerCredentialsBeforeDispatch(bool selection, bool missingSubscription)
        {
            if (missingSubscription)
                ConfigReader.Set(SharedSettings.EnrolmentMSOcpKey, "");
            else
                tokenProvider.Token = "";
            using var services = CreateServices();
            var provider = services.GetRequiredService<AddressLookupV3Provider>();
            var exception = await Assert.ThrowsAsync<GraphQLException>(async () =>
            {
                if (selection)
                    await provider.GetSelectionAsync(new() { Reference = "opaque-reference" }, CancellationToken.None);
                else
                    await provider.GetSuggestionsAsync(new() { Query = "10 Downing Street", CountryCode = "GB" }, CancellationToken.None);
            });
            Assert.Equal("503", Assert.Single(exception.Errors).Code);
            Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Endpoint));
            Assert.Equal(missingSubscription ? 0 : 1, tokenProvider.Resources.Count);
        }

        [Fact]
        public async Task Selection_TokenAcquisitionFailure_DoesNotDispatchOrRetry()
        {
            tokenProvider.Fail = true;
            using var services = CreateServices();
            await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<AddressLookupV3Provider>()
                .GetSelectionAsync(new() { Reference = "opaque-reference" }, CancellationToken.None));
            Assert.Single(tokenProvider.Resources);
            Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Endpoint));
        }

        [Theory]
        [InlineData(401)]
        [InlineData(403)]
        [InlineData(429)]
        [InlineData(503)]
        public async Task Selection_PreservesFailureStatus_WithoutRetry(int status)
        {
            ConfigReader.Set(Settings.AddressLookupV3ApimUrl, Endpoint + "/" + status);
            using var services = CreateServices();
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => services.GetRequiredService<AddressLookupV3Provider>()
                .GetSelectionAsync(new() { Reference = "opaque-reference" }, CancellationToken.None));
            Assert.Equal((HttpStatusCode)status, exception.StatusCode);
            Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(Endpoint));
        }

        [Theory]
        [InlineData("Production", "", "http://localhost:7071/api")]
        [InlineData("Development", "azure-instance", "http://localhost:7071/api")]
        [InlineData("Development", "", "http://address-v3.example.test")]
        public async Task Operations_RejectHostedLoopbackOrInsecureRemoteTarget(string environment, string instance, string endpoint)
        {
            ConfigReader.Set("AZURE_FUNCTIONS_ENVIRONMENT", environment);
            ConfigReader.Set("WEBSITE_INSTANCE_ID", instance);
            ConfigReader.Set(Settings.AddressLookupV3ApimUrl, endpoint);
            using var services = CreateServices();
            await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<AddressLookupV3Provider>()
                .GetSuggestionsAsync(new() { Query = "Test address", CountryCode = "GB" }, CancellationToken.None));
            Assert.Empty(tokenProvider.Resources);
        }

        private ServiceProvider CreateServices() =>
            new ServiceCollection()
                .AddLogging()
                .AddSingleton<TestHttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(services => services.GetRequiredService<TestHttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(tokenProvider)
                .AddSingleton<AddressLookupV3Provider>()
                .BuildServiceProvider();

        private sealed class TestTokenProvider : IOAuthTokenProvider
        {
            public string Token { get; set; } = "server-app-token";
            public bool Fail { get; set; }
            public List<string> Resources { get; } = [];

            public Task<string> AcquireTokenAsync(string resource)
            {
                Resources.Add(resource);
                if (Fail) throw new InvalidOperationException("Synthetic token acquisition failure.");
                return Task.FromResult(Token);
            }
        }
    }
}

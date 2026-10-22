using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Infrastructure.Interfaces;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class GbgV3ServiceTests
    {
        public GbgV3ServiceTests()
        {
            ConfigReader.Set(Constants.Environment.GbgV3FindEndpoint, "https://provider.example.test/Capture/Interactive/Find/v1.20/json6.ws");
            ConfigReader.Set(Constants.Environment.GbgV3RetrieveEndpoint, "https://provider.example.test/Capture/Interactive/Retrieve/v1.30/json6.ws");
            ConfigReader.Set(Constants.Environment.GbgServiceKeyMap, "[{\"clientId\":\"another-client\",\"gbgKey\":\"client-specific-key\"},{\"clientId\":\"\",\"gbgKey\":\"provider-key\"}]");
        }

        [Fact]
        public async Task FindAsync_UsesEncodedTextAndNeverAddsContainer()
        {
            using var services = CreateServices();
            var service = services.GetRequiredService<GbgV3Service>();

            var response = await service.FindAsync(new GbgV3FindRequest
            {
                Text = "12 High & Main",
                CountryCode = "GB",
                Limit = 6
            }, CancellationToken.None);

            Assert.NotNull(response);
            var sentRequest = Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses("https://provider.example.test")).RequestMessage!;
            Assert.Equal(HttpMethod.Get, sentRequest.Method);
            Assert.Contains("Text=12%20High%20%26%20Main", sentRequest.RequestUri!.Query, StringComparison.Ordinal);
            Assert.DoesNotContain("Container=", sentRequest.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("Countries=GB", sentRequest.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("Key=provider-key", sentRequest.RequestUri.Query, StringComparison.Ordinal);
            Assert.DoesNotContain("client-specific-key", sentRequest.RequestUri.Query, StringComparison.Ordinal);
            Assert.True(sentRequest.IsRequestUriLoggingSuppressed());
        }

        [Fact]
        public async Task FindAsync_UsesContainerInsteadOfTextWhenDrillingDown()
        {
            using var services = CreateServices();
            var service = services.GetRequiredService<GbgV3Service>();

            var response = await service.FindAsync(new GbgV3FindRequest
            {
                Container = "container / id",
                CountryCode = "GB",
                Limit = 6
            }, CancellationToken.None);

            Assert.NotNull(response);
            var sentRequest = Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses("https://provider.example.test")).RequestMessage!;
            Assert.Contains("Container=container%20%2F%20id", sentRequest.RequestUri!.Query, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=", sentRequest.RequestUri.Query, StringComparison.Ordinal);
        }

        [Fact]
        public async Task RetrieveAsync_UsesExistingDefaultKeyWithoutCallerKeyMapping()
        {
            using var services = CreateServices();
            var service = services.GetRequiredService<GbgV3Service>();

            await service.RetrieveAsync("address / id", CancellationToken.None);

            var sentRequest = Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses("https://provider.example.test")).RequestMessage!;
            Assert.Contains("Key=provider-key", sentRequest.RequestUri!.Query, StringComparison.Ordinal);
            Assert.Contains("Id=address%20%2F%20id", sentRequest.RequestUri.Query, StringComparison.Ordinal);
            Assert.True(sentRequest.IsRequestUriLoggingSuppressed());
            Assert.Equal("true", Assert.Single(sentRequest.Headers.GetValues("x-nowait")));
        }

        [Theory]
        [InlineData("")]
        [InlineData("{")]
        [InlineData("null")]
        [InlineData("[]")]
        [InlineData("[null]")]
        [InlineData("[{\"clientId\":\"another-client\",\"gbgKey\":\"client-specific-key\"}]")]
        [InlineData("[{\"clientId\":\"\",\"gbgKey\":\"\"}]")]
        public async Task MissingOrInvalidDefaultKey_RejectsFindAndRetrieveBeforeProviderDispatch(string keyMap)
        {
            ConfigReader.Set(Constants.Environment.GbgServiceKeyMap, keyMap);
            using var services = CreateServices();
            var service = services.GetRequiredService<GbgV3Service>();

            var findError = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.FindAsync(
                new GbgV3FindRequest { Text = "12 High Street", CountryCode = "GB", Limit = 6 }, CancellationToken.None));
            var retrieveError = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.RetrieveAsync("address-id", CancellationToken.None));

            Assert.Equal(HttpStatusCode.ServiceUnavailable, findError.StatusCode);
            Assert.Equal("ProviderKeyUnavailable", findError.Code);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, retrieveError.StatusCode);
            Assert.Equal("ProviderKeyUnavailable", retrieveError.Code);
            Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses("https://provider.example.test"));
        }

        private static ServiceProvider CreateServices() =>
            new ServiceCollection()
                .AddLogging()
                .AddSingleton<TestHttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(services => services.GetRequiredService<TestHttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<GbgV3Service>()
                .BuildServiceProvider();
    }
}

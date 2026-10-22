using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Inflight.Connectivity.Application;
using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;
using VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces;
using VAA.EAPI.Microservices.Inflight.Connectivity.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Tests
{

    public sealed class RegisterDeviceServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RegisterDeviceServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                          .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                          .AddSingleton<IHttpConnector, HttpConnector>()
                                          .AddSingleton<RegisterDeviceService>()
                                          .BuildServiceProvider();

            InflightConnectivityITestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task RegisterClientAsync_ReturnsResponseBody()
        {
            var _starlinkService = _serviceProvider.GetService<RegisterDeviceService>()!;

            var response = await _starlinkService.RegisterClientAsync(new StarlinkClientRequest
            {
                DeviceId = "device-1",
                SandboxId = "sandbox-1",
                Expiry = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }, CancellationToken.None);

            Assert.True(response.IsValid);
        }

        [Fact]
        public async Task RegisterClientAsync_ReturnsFalied()
        {
            var _starlinkService = _serviceProvider.GetService<RegisterDeviceService>()!;

            ConfigReader.Set("StarlinkSandboxUrl", "https://starlink.sandboxfail/");

            var response = await _starlinkService.RegisterClientAsync(new StarlinkClientRequest
            {
                DeviceId = "device-12",
                SandboxId = "sandbox-1",
                Expiry = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }, CancellationToken.None);

            Assert.False(response.IsValid);
            Assert.NotEmpty(response.Errors);
            Assert.Contains("400", response.Errors.First());
        }

        [Fact]
        public async Task RegisterClientAsync_ReturnsFaliedForExpiredToken()
        {
            var _starlinkService = _serviceProvider.GetService<RegisterDeviceService>()!;

            ConfigReader.Set("StarlinkSandboxUrl", "https://starlink.sandboxtokenexpired/");

            var response = await _starlinkService.RegisterClientAsync(new StarlinkClientRequest
            {
                DeviceId = "device-12",
                SandboxId = "sandbox-1",
                Expiry = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }, CancellationToken.None);

            Assert.False(response.IsValid);
            Assert.NotEmpty(response.Errors);
            Assert.Contains("401", response.Errors.First());
        }
    }
}

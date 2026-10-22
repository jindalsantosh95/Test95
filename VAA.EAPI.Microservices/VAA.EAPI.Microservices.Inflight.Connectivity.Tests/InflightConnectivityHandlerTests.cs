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

    public sealed class InflightConnectivityHandlerTests: IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public InflightConnectivityHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                          .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                          .AddSingleton<IHttpConnector, HttpConnector>()
                                          .AddSingleton<IRegisterDeviceService, RegisterDeviceService>()
                                          .AddSingleton<InflightConnectivityHandler>()
                                          .BuildServiceProvider();

            InflightConnectivityITestsCommon.CreateTestEnvironmentSettings();

        }

        [Fact]
        public async Task RegisterClientAsync_ThrowsWhenRequestIsNull()
        {
            var _handler = _serviceProvider.GetService<InflightConnectivityHandler>()!;

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.RegisterClientAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task RegisterClientAsync_ShouldSendDeviceSandboxAndOneMonthExpiry()
        {
            var _handler = _serviceProvider.GetService<InflightConnectivityHandler>()!;

            var result = await _handler.RegisterClientAsync(new InflightConnectivityRequest
            {
                DeviceId = "device-1",
                SandboxId = "sandbox-1",
                Expiry = DateTime.UtcNow.AddDays(30)
            }, CancellationToken.None);

            Assert.Equal("Registered", result.Status);
        }

        [Fact]
        public async Task RegisterClientAsync_ShouldFailIfDeviceDoesNotMatch()
        {
            var _handler = _serviceProvider.GetService<InflightConnectivityHandler>()!;

            ConfigReader.Set("StarlinkSandboxUrl", "https://starlink.sandboxfail/");

            var result = await _handler.RegisterClientAsync(new InflightConnectivityRequest
            {
                DeviceId = "device-12",
                SandboxId = "sandbox-1",
                Expiry = DateTime.UtcNow.AddDays(1)
            }, CancellationToken.None);

            Assert.Equal("Failed", result.Status);
        }
    }
}

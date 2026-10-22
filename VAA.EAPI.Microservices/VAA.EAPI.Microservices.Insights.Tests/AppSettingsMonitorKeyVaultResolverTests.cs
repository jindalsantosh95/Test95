using Microsoft.Extensions.Logging;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public class AppSettingsMonitorKeyVaultResolverTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;

        public AppSettingsMonitorKeyVaultResolverTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
        }

        [Fact]
        public async Task FullCheckKeyVaultResolverStatus_LogsWarningOnlyForUnresolvedReferences()
        {
            var logger = new CapturingLogger<AppSettingsMonitor>();
            var httpConnector = new HttpConnector(new LoggerFactory().CreateLogger<HttpConnector>(), _httpClientFactoryFixture.Current);
            var tokenProvider = new StubTokenProvider();
            var azureResources = new AzureResources(httpConnector, new LoggerFactory().CreateLogger<AzureResources>(), tokenProvider);
            var monitor = new AppSettingsMonitor(azureResources, httpConnector, logger, tokenProvider);

            await monitor.FullCheckKeyVaultResolverStatus();

            var warning = Assert.Single(logger.Entries, entry => entry.LogLevel == LogLevel.Warning);
            Assert.Contains("[KEYVAULT-REFERENCE-UNRESOLVED]", warning.Message);
            Assert.Contains("UnresolvedSetting", warning.Message);
            Assert.Contains("SecretNotFound", warning.Message);
            Assert.DoesNotContain("ResolvedSetting", warning.Message);
            Assert.DoesNotContain("PlainSetting", warning.Message);
        }

        private sealed class StubTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("KEYVAULT_SCAN_TOKEN");
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            public List<(LogLevel LogLevel, string Message)> Entries { get; } = [];

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add((logLevel, formatter(state, exception)));
            }

            private sealed class NoopDisposable : IDisposable
            {
                public static NoopDisposable Instance { get; } = new NoopDisposable();

                public void Dispose()
                {
                }
            }
        }
    }
}

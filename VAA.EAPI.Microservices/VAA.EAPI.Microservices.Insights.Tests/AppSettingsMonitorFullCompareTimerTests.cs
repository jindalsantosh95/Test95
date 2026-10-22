using VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor;
using VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor;
using VAA.EAPI.Microservices.Insights.Triggers.Timer;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public class AppSettingsMonitorFullCompareTimerTests
    {
        [Fact]
        public async Task Run_ExecutesCombinedFullCompareWorkflow()
        {
            var monitor = new CapturingAppSettingsMonitor();
            var timer = new AppSettingsMonitorFullCompareTimer(monitor);

            await timer.Run(null!);

            Assert.Equal(1, monitor.FullCompareCallCount);
            Assert.Equal(0, monitor.FullCheckKeyVaultResolverStatusCallCount);
        }

        private sealed class CapturingAppSettingsMonitor : IAppSettingsMonitor
        {
            public int FullCompareCallCount { get; private set; }

            public int FullCheckKeyVaultResolverStatusCallCount { get; private set; }

            public Task<IEnumerable<AppInsightAlert>> FindRecentDiffAlerts(TimeSpan timeframe) =>
                Task.FromResult(Enumerable.Empty<AppInsightAlert>());

            public Task<AppSettingCompareResponse> Snapshot(string subscriptionId, string resourceGroupName, string functionAppName) =>
                Task.FromResult(new AppSettingCompareResponse());

            public Task<AppSettingCompareResponse> Compare(string subscriptionId, string resourceGroupName, string functionAppName) =>
                Task.FromResult(new AppSettingCompareResponse());

            public Task FullCompare()
            {
                FullCompareCallCount++;
                return Task.CompletedTask;
            }

            public Task FullCheckKeyVaultResolverStatus()
            {
                FullCheckKeyVaultResolverStatusCallCount++;
                return Task.CompletedTask;
            }
        }
    }
}

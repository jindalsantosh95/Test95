using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor;

namespace VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor
{
    public interface IAppSettingsMonitor
    {
        Task<IEnumerable<AppInsightAlert>> FindRecentDiffAlerts(TimeSpan timeframe);

        Task<AppSettingCompareResponse> Snapshot(string subscriptionId, string resourceGroupName, string functionAppName);

        Task<AppSettingCompareResponse> Compare(string subscriptionId, string resourceGroupName, string functionAppName);

        Task FullCompare();

        Task FullCheckKeyVaultResolverStatus();
    }
}

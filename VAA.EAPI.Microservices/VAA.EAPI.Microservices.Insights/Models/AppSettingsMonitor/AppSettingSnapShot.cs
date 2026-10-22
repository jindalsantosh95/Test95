using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;

namespace VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor
{
    internal class AppSettingSnapshot
    {
        public DateTime SnapshotDateTime { get; set; }
        public IReadOnlyCollection<AzureAppSetting> AppSettings { get; set; } = new List<AzureAppSetting>();
    }
}

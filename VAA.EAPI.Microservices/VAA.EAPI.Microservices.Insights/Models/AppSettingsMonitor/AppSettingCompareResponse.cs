using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor
{
    public class AppSettingCompareResponse
    {
        public DateTime SnapshotDateTime { get; set; }
        public IReadOnlyCollection<AppSettingsState> AppSettingsState { get; set; } = new List<AppSettingsState>();
    }
}

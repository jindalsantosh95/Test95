using System;

namespace VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor
{
    public class AppInsightAlert
    {
        public DateTime Timestamp { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string AppName { get; set; }

    }
}

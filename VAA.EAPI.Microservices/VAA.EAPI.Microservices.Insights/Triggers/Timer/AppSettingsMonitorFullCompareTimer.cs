using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor;

namespace VAA.EAPI.Microservices.Insights.Triggers.Timer
{
    public sealed class AppSettingsMonitorFullCompareTimer(IAppSettingsMonitor monitor)
    {
        private readonly IAppSettingsMonitor _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));

        [Function("AppSettingsMonitorFullCompareTimer")]
        public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = false)] TimerInfo timer)
        {
            await _monitor.FullCompare();
        }
    }
}

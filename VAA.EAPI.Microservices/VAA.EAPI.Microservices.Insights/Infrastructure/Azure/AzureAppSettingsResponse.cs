using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure
{
    public class AzureAppSettingsResponse
    {
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
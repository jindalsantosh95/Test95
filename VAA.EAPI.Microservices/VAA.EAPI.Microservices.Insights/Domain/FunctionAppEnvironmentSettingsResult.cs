using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public sealed class FunctionAppEnvironmentSettingsResult
    {
        public string ResourceId { get; set; }

        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
    }
}

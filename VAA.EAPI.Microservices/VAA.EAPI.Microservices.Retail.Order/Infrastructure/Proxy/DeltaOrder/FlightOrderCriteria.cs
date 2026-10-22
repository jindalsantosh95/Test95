using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FlightOrderCriteria
    {
        public string UpgradePreferenceText { get; set; }
        public IEnumerable<string> CustomerConsentInfo { get; set; }
    }
}

using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Warning
    {
        public string WarningCode { get; set; }
        public string WarningStatusCodeDesc { get; set; }
        public string WarningStatusResultText { get; set; }
        public IEnumerable<MoreMessage> MoreInfo { get; set; }
    }
}

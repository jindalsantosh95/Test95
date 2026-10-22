using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    public class TrackIdHistory
    {
        public string TrackId { get; set; }
        public IEnumerable<string> PaxRefIds { get; set; }
    }
}

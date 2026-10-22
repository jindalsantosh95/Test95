using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class TrackIdHistory
    {
        public string TrackId { get; set; }
        public IEnumerable<string> PaxRefIds { get; set; }
    }
}

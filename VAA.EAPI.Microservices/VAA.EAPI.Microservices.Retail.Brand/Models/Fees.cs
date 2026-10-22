using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Fees
    {
        public IEnumerable<ChangePolicy> Change { get; set; }
        public IEnumerable<CancelPolicy> Cancellation { get; set; }
    }
}
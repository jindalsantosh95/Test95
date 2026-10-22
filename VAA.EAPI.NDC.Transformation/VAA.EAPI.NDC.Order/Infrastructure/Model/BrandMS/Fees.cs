using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class Fees
    {
        public IEnumerable<ChangePolicy> Change { get; set; }
        public IEnumerable<CancelPolicy> Cancellation { get; set; }
    }

}

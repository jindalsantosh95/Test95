using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Fees
    {
        public IEnumerable<ChangePolicy> Change { get; set; }
        public IEnumerable<CancelPolicy> Cancellation { get; set; }
    }
}
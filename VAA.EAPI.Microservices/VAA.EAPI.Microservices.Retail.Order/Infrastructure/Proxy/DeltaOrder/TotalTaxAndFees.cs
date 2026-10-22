using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class TotalTaxAndFees
    {
        public TotalTaxAmt TotalTaxAmt { get; set; }
        public List<Taxis> Taxes { get; set; }
    }
}

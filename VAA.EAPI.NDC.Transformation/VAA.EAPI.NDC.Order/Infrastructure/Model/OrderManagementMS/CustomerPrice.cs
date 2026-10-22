using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class CustomerPrice
    {
        public string Ptc { get; set; }
        public string FareCalc { get; set; }
        public IEnumerable<PriceComponent> PriceDetails { get; set; }
    }
}

using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FarePrice
    {
        public BaseFare BaseFare { get; set; }
        public BaseFareOtherCurrency BaseFareOtherCurrency { get; set; }
        public SalesPrice SalesPrice { get; set; }
        public List<TotalFarePassengerPrice> TotalFarePassengerPrice { get; set; }
        public TotalFarePrice TotalFarePrice { get; set; }
        public TotalFarePricePerPtc TotalFarePricePerPtc { get; set; }
        public TotalTaxAndFees TotalTaxAndFees { get; set; }
    }
}

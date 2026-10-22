using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class PriceDetail
    {
        public string PriceType { get; set; }
        public FareDetail AirDoc { get; set; }
        public IEnumerable<PriceComponent> GenericPrice { get; set; }
        public IEnumerable<CustomerPrice> CustomerPrices { get; set; }

    }
}

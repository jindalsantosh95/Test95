using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class CustomerPrice
    {
        public string CustId { get; set; }
        public string PTCCode { get; set; }
        public string FareCalc { get; set; }
        public string CustomerName { get; set; }
        public string CustomerIdentifier { get; set; }
        public IEnumerable<PriceComponent> PriceParts { get; set; }
        public decimal? LoyaltySchemePoints { get; set; }
        public PriceOption TotalPriceOption { get; set; }


    }
}

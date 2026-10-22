using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Penalty
    {
        public string Type { get; set; }
        public CurrencyAmount Amount { get; set; }
        public decimal? Percentage { get; set; }
        public Link Link { get; set; }
        public IEnumerable<string> Text { get; set; }


    }
}

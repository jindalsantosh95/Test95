using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS
{
    public class OfferItem
    {
        public string OfferItemId { get; set; }
        public string OfferItemDesc { get; set; }
        public string SSRCode { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public CurrencyAmount Price { get; set; }
        public IEnumerable<string> PTC { get; set; }

    }
}

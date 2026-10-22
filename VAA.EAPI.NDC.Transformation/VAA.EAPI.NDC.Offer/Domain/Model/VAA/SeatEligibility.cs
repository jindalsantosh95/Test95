using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{

    public class SeatEligibility
    {
        public string EligibilityReason { get; set; }

        public string CatalogueRefId { get; set; }

        public IEnumerable<string> CustomerId { get; set; }

        public IEnumerable<string> CustomerPTC { get; set; }

        public IEnumerable<OfferItemNumber> OfferItemId { get; set; }

        public CurrencyAmount SeatPrice { get; set; }
       
    }
}

using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class FlexibleDatePriceCalendar
    {
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<PriceItems> PriceItems { get; set; }
        public IEnumerable<OriginDestination> OriginDestinations { get; set; }
        public bool Availability { get; set; }
        public bool LowestPriceOfferIndicator { get; set; }
    }
}

using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch
{
    public class FlexibleDatePriceCalendar
    {
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<PriceItems> PriceItems { get; set; }
        public IEnumerable<OriginDestination> OriginDestinations { get; set; }
        public bool Availability { get; set; }
        public bool LowestPriceOfferIndicator { get; set; }
        public bool LowestPriceInWeekIndicator { get; set; }
        public bool DirectFlightIndicator { get; set; }
        public string OfferId { get; set; }
    }
}
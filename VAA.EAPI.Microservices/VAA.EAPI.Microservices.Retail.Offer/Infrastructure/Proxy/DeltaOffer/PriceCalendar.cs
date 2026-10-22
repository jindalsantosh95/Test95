using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PriceCalendar
    {
        public IEnumerable<string> TripIds { get; set; }
        public string PriceCalendarDate { get; set; }
    }
}

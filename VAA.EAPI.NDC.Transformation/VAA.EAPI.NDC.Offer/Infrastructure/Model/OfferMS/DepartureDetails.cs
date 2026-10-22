using System;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class DepartureDetails
    {
        public DateTime DepartureDate { get; set; }
        public IEnumerable<FlexibleDatePriceCalendar> FlexibleDatePriceCalendarCell { get; set; }
    }
}

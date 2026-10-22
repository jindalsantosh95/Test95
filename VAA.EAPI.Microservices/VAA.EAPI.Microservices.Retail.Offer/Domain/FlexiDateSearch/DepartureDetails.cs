using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch
{
    public class DepartureDetails
    {
        public DateTime DepartureDate { get; set; }
        public IEnumerable<FlexibleDatePriceCalendar> FlexibleDatePriceCalendarCell { get; set; }
    }
}
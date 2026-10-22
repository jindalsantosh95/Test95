using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch
{
    public class Calendar
    {
        public IEnumerable<CalendarFromPrice> FromPrices { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}
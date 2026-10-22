using System;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch
{
    public class CalendarFromPrice
    {
        public DateOnly FromDate { get; set; }
        public CalendarPrice Price { get; set; }
    }
}
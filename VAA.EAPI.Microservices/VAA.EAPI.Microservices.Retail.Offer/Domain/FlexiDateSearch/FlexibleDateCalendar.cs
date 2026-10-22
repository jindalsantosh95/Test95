using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch
{
    public class FlexibleDateCalendar
    {
        public IEnumerable<string> ColumnText { get; set; }
        public IEnumerable<DepartureDetails> DepartureDetails { get; set; }
    }
}
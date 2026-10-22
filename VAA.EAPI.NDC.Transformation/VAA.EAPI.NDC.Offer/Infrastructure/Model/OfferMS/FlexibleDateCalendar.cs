using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class FlexibleDateCalendar
    {
        public IEnumerable<string> ColumnText { get; set; }
        public IEnumerable<DepartureDetails> DepartureDetails { get; set; }
    }
}

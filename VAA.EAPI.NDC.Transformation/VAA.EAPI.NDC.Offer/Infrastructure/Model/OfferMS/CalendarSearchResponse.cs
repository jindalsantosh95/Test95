using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class CalendarSearchResponse
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public FlexibleDateCalendar FlexibleDateCalendar { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}

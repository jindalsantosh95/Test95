using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class ItemsForAddition
    {
        public string OfferRefID { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
    }
}

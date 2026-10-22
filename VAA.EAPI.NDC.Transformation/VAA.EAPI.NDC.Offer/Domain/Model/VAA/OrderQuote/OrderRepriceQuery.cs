using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public class OrderRepriceQuery 
    {
        public Pos Pos { get; set; }

        public Parties Parties { get; set; }

        public PricedOffer PricedOffer { get; set; }

        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }

        public RepriceQueries RepriceQueries { get; set; }

        public string OrderId { get; set; }

        public string OwnerCode { get; set; }

        public bool IsHeldBooking { get; set; }

    }
}

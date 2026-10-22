using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public class RepriceRequest
    {
        public Pos Pos { get; set; }

        public Parties Parties { get; set; }

        public RepriceQueries RepriceQueries { get; set; }

        public PricedOffer PricedOffer { get; set; }

        public IEnumerable<Customer> CustomerDetails { get; set; }

        public bool IsReshopDisruption { get; set; }
        public bool IsReshopHoldBooking { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
    }
}

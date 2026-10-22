using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS
{
    public class SearchFilterCriteria
    {
        public Parties Parties { get; set; }
        public BookingReferenceCriteria BookingReferenceCriteria { get; set; }
        public AccountableDocumentFilterCriteria AccountableDocumentFilterCriteria { get; set; }
        public OrderReferenceCriteria OrderReferenceCriteria { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
    }
}

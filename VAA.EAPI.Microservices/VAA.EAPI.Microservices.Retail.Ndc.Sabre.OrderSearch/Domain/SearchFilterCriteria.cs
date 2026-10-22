using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain
{
    public class SearchFilterCriteria
    {
        public Parties Parties { get; set; }
        public BookingReferenceCriteria BookingReferenceCriteria { get; set; }
        public AccountableDocumentFilterCriteria AccountableDocumentFilterCriteria { get; set; }
        public OrderReferenceCriteria OrderReferenceCriteria { get; set; }
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public bool HistoryFlag { get; set; }
        public bool IsCancelledAndRebooked { get; set; }
    }
}

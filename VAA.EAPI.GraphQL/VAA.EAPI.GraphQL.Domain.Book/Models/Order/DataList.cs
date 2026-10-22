using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using DomainCustomer = VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Order
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<DomainCustomer.CustomerInfo> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
    }
}

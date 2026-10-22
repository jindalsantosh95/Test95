using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{

    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<Penalty> Penalty { get; set; }

    }
}

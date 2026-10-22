using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class OrderDetails
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinations { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<AccountableDocumentReference> AccountableDocumentRefs { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
    }
}

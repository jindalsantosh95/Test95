using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderDetails
    {
        public IEnumerable<OriginDestination> OriginDestinations { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<CustomerDetails> Customers { get; set; }
        public IEnumerable<AccountableDocumentReference> AccountableDocumentRefs { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }

    }
}

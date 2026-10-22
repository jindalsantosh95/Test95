using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<BaggageAllowances> BaggageAllowances { get; set; }
        public IEnumerable<Penalty> Penalty { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public IEnumerable<ServiceDefinition> ServiceDefinitions { get; set; }
        public IEnumerable<Journey> Journey { get; set; }

    }
}

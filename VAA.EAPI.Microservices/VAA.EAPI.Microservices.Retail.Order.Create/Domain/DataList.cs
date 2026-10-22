using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Domain
{
    public class DataList
    {
        public List<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        public List<Segment> SegmentDetails { get; set; }
        public List<BaggageAllowances> BaggageAllowances { get; set; }
        public List<PriceClass> PriceClass { get; set; }

    }
}

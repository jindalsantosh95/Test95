using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<EAPI.Domain.Virgin.Models.Customer.Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public IEnumerable<OfferSegment> OfferSegments { get; set; }
        public IEnumerable<BaggageAllowances> BaggageAllowances { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public IEnumerable<Penalty> Penalty { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
        public IEnumerable<ProductComponent> ProductComponents { get; set; }
    }
}

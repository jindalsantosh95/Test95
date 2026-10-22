using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;


namespace VAA.EAPI.GraphQL.Domain.Book.Models.Offer
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer.CustomerInfo> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public IEnumerable<OfferSegment> OfferSegments { get; set; }
        public IEnumerable<BaggageAllowance> BaggageAllowances { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public IEnumerable<Penalty> Penalty { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
        public IEnumerable<ProductComponent> ProductComponents { get; set; }
    }
}

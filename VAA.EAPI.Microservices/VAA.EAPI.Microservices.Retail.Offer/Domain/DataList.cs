using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class DataList
    {
        internal Dictionary<(string Origin, string Destination, string DepartureDate), OriginDestinationAssociation> OriginDestinationsByKey { get; } = new();
        internal Dictionary<(string OdRefId, string Origin, string Destination, string FlightNumber), Segment> FlightSegmentsByKey { get; } = new();
        internal Dictionary<(string ClassOfService, string FlightSegmentRefId, string CabinCode), OfferSegment> OfferSegmentsByKey { get; } = new();
        internal Dictionary<(string FareBasisCode, string FareTypeCode), PriceClass> PriceClassesByKey { get; } = new();
        internal Dictionary<string, Journey> JourneysByKey { get; } = new();

        internal void ClearLookupIndexes()
        {
            OriginDestinationsByKey.Clear();
            OriginDestinationsByKey.TrimExcess();
            FlightSegmentsByKey.Clear();
            FlightSegmentsByKey.TrimExcess();
            OfferSegmentsByKey.Clear();
            OfferSegmentsByKey.TrimExcess();
            PriceClassesByKey.Clear();
            PriceClassesByKey.TrimExcess();
            JourneysByKey.Clear();
            JourneysByKey.TrimExcess();
        }

        public List<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        public List<Segment> FlightSegments { get; set; }
        public List<OfferSegment> OfferSegments { get; set; }
        public List<BaggageAllowances> BaggageAllowances { get; set; }
        public List<PriceClass> PriceClass { get; set; }
        public List<Penalty> Penalty { get; set; }
        public List<Journey> Journey { get; set; }
        public List<ProductComponent> ProductComponents { get; set; }
    }
}

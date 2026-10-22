using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class OfferSegment
    {
        public string OfferSegmentId { get; set; }
        public string FlightSegmentRefId { get; set; }
        public string ClassOfService { get; set; }
        public CabinType CabinType { get; set; }
        public IEnumerable<string> ProductComponentRefIds { get; set; }
        public string TicketNotValidAfter { get; set; }
        public string TicketNotValidBefore { get; set; }
        public string PriceClassRefIds { get; set; }
    }
}

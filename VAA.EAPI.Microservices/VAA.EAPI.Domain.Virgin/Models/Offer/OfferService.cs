using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
{
    public class OfferService
    {
        public string ServiceId { get; set; }
        public Product Product { get; set; }
        public OfferProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<string> JourneyRefIds { get; set; }
        public IEnumerable<string> PriceClassRefIds { get; set; }
        public PenaltyRef PenaltyRef { get; set; }
        public IEnumerable<string> BaggageRefIds { get; set; }
        public IEnumerable<string> FlightSegmentsRefIds { get; set; }
        public FareDetails FareDetails { get; set; }
        public SeatInformation SeatInformation { get; set; }
        public IEnumerable<string> ServiceDefinitionRefIds { get; set; }


    }
}
namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OfferService
    {
        public string? ServiceId { get; set; }
        public Product? Product { get; set; }
        public OfferProductSegmentAssociation? ProductSegmentAssociation { get; set; }
        public IEnumerable<string>? ServiceBeneficiaries { get; set; }
        public IEnumerable<string>? JourneyRefIds { get; set; }
        public IEnumerable<string>? PriceClassRefIds { get; set; }
        public PenaltyRef? PenaltyRef { get; set; }
        public IEnumerable<string>? BaggageRefIds { get; set; }
        public IEnumerable<string>? FlightSegmentsRefIds { get; set; }
        public FareDetails? FareDetails { get; set; }
        public Content? Content { get; set; }
    }
}
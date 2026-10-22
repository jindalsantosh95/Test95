namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OfferItem
    {
        public string OfferItemId { get; set; }
        public string? OfferItemDesc { get; set; }
        public IEnumerable<OfferService> Services { get; set; }
        public IEnumerable<OfferItemPricing>? OfferItemPricing { get; set; }
        public IEnumerable<string>? ServiceBeneficiaries { get; set; }
        public PenaltyRef? PenaltyRef { get; set; }
        public IEnumerable<AirlineTaxonomy>? AirlineTaxonomies { get; set; }
    }
}
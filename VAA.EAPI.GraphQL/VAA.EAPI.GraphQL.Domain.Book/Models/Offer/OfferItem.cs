using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Offer
{
    public class OfferItem
    {
        public string OfferItemId { get; set; }
        public string OfferItemDesc { get; set; }
        public IEnumerable<OfferService> Services { get; set; }
        public IEnumerable<OfferitemPricing> OfferitemPricing { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public BaggageAllowance BaggageAllowance { get; set; }
        public PenaltyRef PenaltyRef { get; set; }
        public IEnumerable<AirlineTaxonomy> AirlineTaxonomies { get; set; }
    }
}
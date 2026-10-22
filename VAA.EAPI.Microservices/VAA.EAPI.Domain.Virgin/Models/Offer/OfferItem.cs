using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
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
        public PaxNameChange PaxNameChangeDetails { get; set; }
        public string TicketDesigCode { get; set; }
    }
}
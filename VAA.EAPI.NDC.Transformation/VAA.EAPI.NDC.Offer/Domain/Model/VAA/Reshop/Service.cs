using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class Service
    {
        public string ServiceId { get; set; }
        public Products Product { get; set; }
        public ProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
    }
}

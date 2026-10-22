using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS
{
    public class Service
    {
        public string ServiceId { get; set; }
        public Product ProductOrder { get; set; }
        public ProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<string> OrderInstructionRefs { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<string> ProductComponentRefIds { get; set; }
    }
}
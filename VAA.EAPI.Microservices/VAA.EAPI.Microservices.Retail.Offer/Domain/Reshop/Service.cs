using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class Service
    {
        public string ServiceId { get; set; }
        public Product Product { get; set; }
        public ProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public List<string> ServiceBeneficiaries { get; set; }
    }
}

using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class Service
    {
        public string ServiceId { get; set; }
        public Product Product { get; set; }
        public OfferProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public List<string> ServiceBeneficiaries { get; set; }
    }
}

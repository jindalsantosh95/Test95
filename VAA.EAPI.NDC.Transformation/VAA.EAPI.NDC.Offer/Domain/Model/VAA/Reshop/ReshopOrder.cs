using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class ReshopOrder
    {
        public string ProductType { get; set; }
        public string OrderItemId { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public IEnumerable<string> RetainServices { get; set; }
        public IEnumerable<string> DeleteServices { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public string OrderProductType { get; set; }
    }
}

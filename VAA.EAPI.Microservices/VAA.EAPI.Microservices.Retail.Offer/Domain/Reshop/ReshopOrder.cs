using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{

    public class ReshopOrder
    {
        public string ProductType { get; set; }
        public string OrderItemId { get; set; }
        public List<Service> Services { get; set; }
        public List<PriceDetail> priceDetails { get; set; }
        public List<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public IEnumerable<string> RetainServices { get; set; }
        public IEnumerable<string> DeleteServices { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public string OrderProductType { get; set; }
    }

}
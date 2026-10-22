using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class AccountableDocumentRef
    {
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string FareCalculation { get; set; }
        public string IssueDate { get; set; }
        public string IssueTime { get; set; }
        public List<Coupon> Coupons { get; set; }
    }

}
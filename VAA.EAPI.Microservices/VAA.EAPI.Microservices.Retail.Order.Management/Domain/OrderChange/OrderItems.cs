using System.Collections;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderItems
    {
        public ProductTypes? ProductType { get; set; }
        public string CurrentSeatAssignment {  get; set; }
        public string SegmentRefId { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocuments { get; set; }
    }
}

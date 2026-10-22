using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS
{
    public class OrderItems
    {
        public string OrderItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OfferIdReference { get; set; }
        public string OfferItemIdReference { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string PNRReferences { get; set; }
        public State ItemStatus { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<string> PaymentReferenceIDs { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class CreateRefundDocumentRequest
    {
        public string TransactionId { get; set; }
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public string Acquirer { get; set; }
        public string AIR4PNR { get; set; }
        public string PaymentCardMaskedNumber { get; set; }
        public string PaymentApprovalCode { get; set; }
        public IEnumerable<RefundPayment> PaymentDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string Status { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}

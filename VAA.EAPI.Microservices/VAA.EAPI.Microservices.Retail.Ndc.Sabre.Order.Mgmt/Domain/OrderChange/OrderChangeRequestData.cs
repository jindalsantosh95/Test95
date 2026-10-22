using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class OrderChangeRequestData
    {
        public enum Operations
        {
            OrderCreate,
            OrderChange,
            OrderCancel
        }

        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string Rloc { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<Remark> OrderRemarks { get; set; }
        public IEnumerable<Offers> SelectedOffers { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string ChannelId { get; set; }
        public bool IsRFC { get; set; }
        public Operations Operation { get; set; }
        public bool CancelPnr { get; set; }
        public IEnumerable<string> TransactionId { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public bool IsCancelDisruption { get; set; }
        public bool? IsCancelHoldBooking { get; set; } = false;
        public bool IsChangeReshopConfirmHoldBooking { get; set; }
        public bool IsChangeReshopHoldBooking { get; set; }
        public bool IsReshopDisruption { get; set; }
    }
}

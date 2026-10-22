using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class OrderChangeRequest
    {
        public enum Operations
        {
            OrderCreate,
            OrderChange,
            OrderCancel
        }

        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string RLOC { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OriginDestinationAssociation> OriginDestinations { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<Remark> OrderRemarks { get; set; }
        public IEnumerable<Offers> SelectedOffers { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
        public string ChannelId { get; set; }
        public bool? IsRFC { get; set; }
        public bool CancelPnr { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public IEnumerable<string> TransactionId { get; set; }
        public Operations Operation { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public bool IsCancelDisruption { get; set; }
        public bool? IsCancelHoldBooking { get; set; } = false;
        public bool IsChangeReshopConfirmHoldBooking { get; set; }
        public bool IsChangeReshopHoldBooking { get; set; }
        public bool IsReshopDisruption { get; set; }
    }
}

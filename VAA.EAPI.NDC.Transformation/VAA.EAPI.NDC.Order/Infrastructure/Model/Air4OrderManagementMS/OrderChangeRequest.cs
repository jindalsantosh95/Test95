using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class OrderChangeRequest
    {
        public EAPI.Domain.Virgin.Models.Common.Pos Pos { get; set; }
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
        public IEnumerable<SeatTransactionId> SeatTransactionId { get; set; }
        public string ChannelId { get; set; }
        public string ExternalRloc { get; set; }
    }
}

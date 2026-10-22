using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderChangeRequestData
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string Rloc { get; set; }
        public IEnumerable<Payment> PaymentDetails { get; set; }
        public IEnumerable<CustomerDetails> CustomerDetails { get; set; }
        public IEnumerable<OriginDestination> OriginDestinations { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
        public IEnumerable<SeatTransactionId> SeatTransactionId { get; set; }
        public string ChannelId { get; set; }
        public string ExternalRloc { get; set; }
    }
}

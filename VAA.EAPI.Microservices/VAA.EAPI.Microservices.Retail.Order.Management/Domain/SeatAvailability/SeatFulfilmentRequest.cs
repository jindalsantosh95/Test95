using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability
{
    public class SeatFulfilmentRequest
    {
        public Pos pos { get; set; }
        public Parties parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<SelectedSeatInfo> SelectedSeatInfo { get; set; }
        public IEnumerable<PaymentDetails> paymentDetails { get; set; }
        public string ChannelId { get; set; }
    }
}

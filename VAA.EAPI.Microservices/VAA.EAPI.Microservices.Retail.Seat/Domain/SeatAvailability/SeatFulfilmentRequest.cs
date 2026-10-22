using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SeatFulfilmentRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<SelectedSeatInfo> SelectedSeatInfo { get; set; }
        public IEnumerable<PaymentDetails> paymentDetails { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<SeatTransactionId> SeatTransactionIds { get; set; }
    }
}

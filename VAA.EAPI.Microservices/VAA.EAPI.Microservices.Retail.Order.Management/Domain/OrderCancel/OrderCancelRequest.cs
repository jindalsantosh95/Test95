using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderCancel
{
    public class OrderCancelRequest
    {
        public Pos Pos { get; set; }
        public Parties Carties { get; set; }
        public CancelCriteria CancelCriteria { get; set; }
        public string OrderId { get; set; }
        public BookingReference BookingReferences { get; set; }
        public CurrencyAmount ExpectedRefundAmount { get; set; }
        public CancelledReshopOffer CancelledReshopOffer { get; set; }
    }
}

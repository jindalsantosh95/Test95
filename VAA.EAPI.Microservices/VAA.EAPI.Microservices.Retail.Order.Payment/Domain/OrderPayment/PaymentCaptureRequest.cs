using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment
{
    public class PaymentCaptureRequest
    {

        public string PaymentReferenceId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string VendorId { get; set; }
        public string RLOC { get; set; }
        public string TicketNumber { get; set; }

    }
}

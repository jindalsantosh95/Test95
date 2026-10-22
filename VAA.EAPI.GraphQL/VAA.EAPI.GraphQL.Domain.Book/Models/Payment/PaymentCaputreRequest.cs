using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class PaymentCaputreRequest
    {
        public string PaymentReferenceId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string VendorId { get; set; }
        public string RLOC { get; set; }
        public string TicketNumber { get; set; }

    }
}

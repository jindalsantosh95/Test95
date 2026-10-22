using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
{
    public class UpdatePaymentStatusRequest
    {
        public string BasketId { get; set; }
        public string PaymentReferenceId { get; set; }
        public EnumPaymentStatus HppPaymentStatus { get; set; }
    }
}

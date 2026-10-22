using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment
{
    public class UpdatePaymentStatusResponse
    {
        public EnumPaymentStatus PaymentStatus { get; set; }
    }
}

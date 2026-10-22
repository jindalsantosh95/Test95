using VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment
{
    public class PaymentOrderResponse
    {
        public string Type { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
        public decimal TotalAmountOnSessions { get; set; }
        public decimal MinDeposit { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountToPay { get; set; }
        public Summary Summary { get; set; }
    }
}

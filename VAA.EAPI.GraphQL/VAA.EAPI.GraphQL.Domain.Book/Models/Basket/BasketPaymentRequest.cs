using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketPaymentRequest
    {
        public string BasketId { get; set; }
        public IEnumerable<PaymentSessionBasketMSRequest> PaymentSessionsBasketMS { get; set; }
    }
}

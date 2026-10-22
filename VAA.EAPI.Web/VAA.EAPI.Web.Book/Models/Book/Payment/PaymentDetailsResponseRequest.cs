using MediatR;

namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentDetailsResponseRequest : IRequest<PaymentResponse>
    {
        public string BasketId { get; set; }
        public string AuthToken { get; set; }

        public PaymentDetailsResponseRequest(string basketId, string authToken)
        {
            BasketId = basketId;
            AuthToken = authToken;
        }
    }
}

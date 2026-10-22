using MediatR;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class OrderConfirmationResponseRequest : IRequest<ConfirmationResponse>
    {
        public string BasketId { get; set; }
        public string AuthToken { get; set; }

        public OrderConfirmationResponseRequest(string basketId, string authToken)
        {
            BasketId = basketId;
            AuthToken = authToken;
        }
    }
}
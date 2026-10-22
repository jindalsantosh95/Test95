using MediatR;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;

namespace VAA.EAPI.Web.Book.Domain.Book.Requests
{
    public class OrderResponseRequest : IRequest<ConfirmationResponse>
    {
        public string BasketId { get; set; }
        public string AuthToken { get; set; }
    }
}

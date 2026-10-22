using MediatR;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common.Entities;

namespace VAA.EAPI.Web.Book.Domain.Book.Requests
{
    public class OrderConfirmationRequest : IRequest<Response<ConfirmationResponse>>
    {
        public Request Request { get; set; }
        public string AuthToken { get; set; }
    }
}

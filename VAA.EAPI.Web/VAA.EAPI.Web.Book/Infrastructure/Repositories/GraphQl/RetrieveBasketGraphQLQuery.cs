using MediatR;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Infrastructure.Repositories.GraphQl
{
    public class RetrieveBasketGraphQLQuery : IRequest<YourDetailsResponse>
    {
        public string BasketId { get; set; }
        public string AuthToken { get; set; }
    }
}
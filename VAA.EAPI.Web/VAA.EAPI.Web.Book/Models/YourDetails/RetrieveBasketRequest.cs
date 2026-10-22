using MediatR;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Models.YourDetails
{
    public class RetrieveBasketRequest : IRequest<YourDetailsResponse>
    {
        public string BasketId { get; set; }
        public string AuthToken { get; set; }

        public RetrieveBasketRequest(string basketId, string authToken)
        {
            BasketId = basketId;
            AuthToken = authToken;
        }
    }
}
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IOfferServiceProvider
    {
        Task<OfferResponse> OfferRetrieveAsync(string offerId);
    }
}

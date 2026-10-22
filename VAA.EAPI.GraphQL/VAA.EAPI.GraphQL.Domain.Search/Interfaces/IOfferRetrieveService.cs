using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IOfferRetrieveService
    {
        Task<FlightOfferResponse> OfferRetrieveAsync(OfferSearchRequest searchRequest);
    }
}

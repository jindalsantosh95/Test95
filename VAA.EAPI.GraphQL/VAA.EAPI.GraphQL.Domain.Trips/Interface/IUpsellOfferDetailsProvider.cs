using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IUpsellOfferDetailsProvider
    {
        Task<UpsellOfferResponse> QueryUpsellOfferDetails(string tripResevationID);
    }
}
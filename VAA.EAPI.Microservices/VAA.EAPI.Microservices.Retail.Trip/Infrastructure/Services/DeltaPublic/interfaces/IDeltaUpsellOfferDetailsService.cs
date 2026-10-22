using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces
{
    public interface IDeltaUpsellOfferDetailsService
    {
        Task<UpsellOfferResponse> RetrieveUpsellOfferDetails(string tripReservationId);

    }
}

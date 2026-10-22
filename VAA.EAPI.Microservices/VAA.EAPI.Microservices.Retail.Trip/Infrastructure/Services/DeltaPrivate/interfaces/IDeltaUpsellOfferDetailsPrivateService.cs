using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IDeltaUpsellOfferDetailsPrivateService
    {
        Task<UpsellOfferResponse> RetrieveUpsellOfferDetails(string tripReservationId);

    }
}

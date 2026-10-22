using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public interface IUpsellOfferDetailsService
    {
        Task<Result<Response<MSSchema.UpsellOfferResponse>>> GetUpsellOfferDetails(string tripBookingId);
    }
}

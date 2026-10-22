using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.SeatAvailability
{
    public class SeatAvailabilityQuery : SearchCriteria, IRequest<Result<Response<SeatAvailabilityResponse>>>
    {

    }
}

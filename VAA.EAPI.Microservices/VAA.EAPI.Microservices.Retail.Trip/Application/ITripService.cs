using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public interface ITripService
    {
        Task<Result<Response<TripsDetailsResponse>>> GetTripsDetails(TripsDetailsRequest request);
        Task<Result<Response<TripListResponseStore>>> GetTripsDetailsList(IEnumerable<TripsDetailsRequest> request);
    }
}

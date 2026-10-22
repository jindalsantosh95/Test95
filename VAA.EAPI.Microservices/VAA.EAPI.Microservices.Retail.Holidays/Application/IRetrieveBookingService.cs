using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;


namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{
    public interface IRetrieveBookingService
    {
        Task<Domain.Trips.TripsDetailsResponse> Handle(TripsDetailsRequest request, CancellationToken cancellationToken);
        Task<TripListResponseStore> HandleList(IEnumerable<TripsDetailsRequest> requestList, CancellationToken cancellationToken);
    }
}
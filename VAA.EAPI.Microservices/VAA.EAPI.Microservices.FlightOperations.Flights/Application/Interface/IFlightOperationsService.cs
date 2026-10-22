using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface
{
    public interface IFlightOperationsService
    {
        Task<Result<Response<OperationalScheduleResponse>>> GetOperationalScheduleAsync(OperationalScheduleRequest request, CancellationToken cancellationToken);
    }
}

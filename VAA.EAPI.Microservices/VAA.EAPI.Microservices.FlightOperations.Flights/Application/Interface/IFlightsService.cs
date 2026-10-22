using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface
{
    public interface IFlightsService
    {
        public Task<Result<Response<IEnumerable<FlightDetail>>>> ScheduleAsync(FlightScheduleRequest request);

        public Task<Result<Response<IEnumerable<FlightDetail>>>> DetailAsync(FlightDetailRequest request);

        public Task<Result<Response<IEnumerable<FlightCrewMember>>>> RosterAsync(FlightCrewRosterRequest request);
    }
}
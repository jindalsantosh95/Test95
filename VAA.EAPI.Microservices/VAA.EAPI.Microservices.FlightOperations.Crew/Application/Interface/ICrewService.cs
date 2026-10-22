using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Crew.Application.Interface
{
    public interface ICrewService
    {
        public Task<Result<Response<IEnumerable<CrewMember>>>> RosterAsync();
    }
}
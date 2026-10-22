using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface
{
    public interface ICheckAircraftCategoryService
    {
        Task<Result<Response<CheckAircraftCategoryResponse>>> CheckAircraftCategoryAsync(CheckAircraftCategoryRequest request, CancellationToken cancellationToken);
    }
}

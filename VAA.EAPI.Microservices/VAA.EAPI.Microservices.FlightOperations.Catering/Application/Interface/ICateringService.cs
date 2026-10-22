using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Interface
{
    public interface ICateringService
    {
        Result<Response<BookingInfo>> ValidateSegmentsMealEligibility(BookingInfo request);
        Task<Result<Response<IEnumerable<Meal>>>> GetFlightMealsAsync(MealsRequest request);
        Task<Result<Response<string>>> AddMealOrderAsync(MealOrders mealOrder);
        Task<Result<Response<string>>> UpdateMealOrderAsync(MealOrders mealOrder);
    }
}
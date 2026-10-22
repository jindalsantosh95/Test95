using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Services
{
    public interface IPopsDbService
    {
        Task<Result<Response<IEnumerable<Meal>>>> GetMealData(MealsRequest mealRequest);
        Task<int> AddMealOrderAsync(MealOrders mealOrder);
        Task<int> UpdateMealOrderAsync(MealOrders mealOrder);
        Task<PopsEligibility> CheckPopsEligibility(string cabinCode, DateTime departureDate, string operatingAirline, string operatingFlightNum, string orgin, string destination);
    }
}
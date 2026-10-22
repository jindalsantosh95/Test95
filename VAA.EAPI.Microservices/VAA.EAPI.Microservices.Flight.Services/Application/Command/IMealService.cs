using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Command
{
    public interface IMealService
    {
        Task<Result<Response<MealOperationResponse>>> AddMealAsync(MealOrderRequest request);
        Task<Result<Response<MealOperationResponse>>> CancelMealAsync(MealCancelRequest request);
    }
}
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Command
{
    public class MealService : IMealService
    {
        private readonly IMealRepository _mealRepository;

        public MealService(IMealRepository mealRepository)
        {
            _mealRepository = mealRepository;
        }

        public async Task<Result<Response<MealOperationResponse>>> AddMealAsync(MealOrderRequest request)
        {
            return await _mealRepository.AddMealAsync(request);
        }

        public async Task<Result<Response<MealOperationResponse>>> CancelMealAsync(MealCancelRequest request)
        {
            return await _mealRepository.CancelMealAsync(request);
        }
    }
}
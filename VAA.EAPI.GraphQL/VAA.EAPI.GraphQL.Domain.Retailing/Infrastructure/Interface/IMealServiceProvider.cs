using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface
{
    public interface IMealServiceProvider
    {
        Task<MealOperationResponse> AddMeal(AddMealRequest request);
        Task<MealOperationResponse> CancelMeal(CancelMealRequest request);
    }
}
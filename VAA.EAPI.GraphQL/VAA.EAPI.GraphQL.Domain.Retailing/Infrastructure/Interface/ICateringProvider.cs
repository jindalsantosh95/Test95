using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface
{
    public interface ICateringProvider
    {
        Task<PNR> IsEligible(PNR bookingInfo);
        Task<IEnumerable<Meal>> GetFlightMeals(DateOnly flightDate, string carrier, int flightNumber, string cabin);
        Task<string> AddMealOrder(MealOrders mealOrder);
        Task<string> UpdateMealOrder(MealOrders mealOrder);

    }
}

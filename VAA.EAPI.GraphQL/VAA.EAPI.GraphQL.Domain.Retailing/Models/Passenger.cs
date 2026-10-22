using HotChocolate;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.Passenger")]
    public class Passenger
    {
        public static explicit operator Passenger(JObject source)
        {
            return new()
            {
                PassengerId = source["PassengerId"].ToString(),
                FirstName = source["FirstName"].ToString(),
                LastName = source["LastName"].ToString(),
                FrequentFlyerNumber = source["FrequentFlyerNumber"].ToString(),
                SelectedMeal = source["SelectedMeal"]?.ToString(),
                Meals = source["Meals"]?.ToArray().Select(x => (Meal)x)
            };
        }

        public string PassengerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FrequentFlyerNumber { get; set; }
        public string SelectedMeal { get; set; }    // This is used to capture the meal code during the order mutation, and may be the current meal selection on the PNR during queries
        public IEnumerable<Meal> Meals { get; set; }
    }
}
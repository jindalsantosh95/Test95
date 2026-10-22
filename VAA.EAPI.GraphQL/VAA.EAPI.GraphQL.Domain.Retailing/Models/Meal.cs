using HotChocolate;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.Meal")]
    public class Meal
    {

        public string DishId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public static explicit operator Meal(JObject source)
        {
            return new Meal
            {
                DishId = source["DishId"]?.ToString(),
                Code = source["Code"]?.ToString(),
                Description = source["Description"]?.ToString()
            };
        }

    }
}
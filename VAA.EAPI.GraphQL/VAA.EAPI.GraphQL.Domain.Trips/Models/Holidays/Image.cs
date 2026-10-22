using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayContentImage")]
    public class Image
    {
        public int Order { get; set; }
        public string Description { get; set; }
        public Variants Variants { get; set; }
    }
}

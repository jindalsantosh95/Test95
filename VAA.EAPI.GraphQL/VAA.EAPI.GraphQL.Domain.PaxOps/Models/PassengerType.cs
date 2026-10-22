using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:PassengerType")]
    public class PassengerType
    {
        public string Code { get; set; }

        [GraphQLDescription("Passenger type display name.")]
        public string Name { get; set; }
        public string Category { get; set; }

        public static explicit operator PassengerType(Infrastructure.Models.CheckInMS.PassengerType source) => new()
        {
            Code = source.Code,
            Name = source.Name,
            Category = source.Category
        };
    }
}

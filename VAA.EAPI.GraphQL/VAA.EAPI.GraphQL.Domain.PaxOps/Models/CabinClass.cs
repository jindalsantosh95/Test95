using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:CabinClass")]
    public class CabinClass
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator CabinClass(Infrastructure.Models.CheckInMS.CabinClass source) => new()
        {
            Code = source.Code,
            Name = source.Name
        };
    }
}
using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:BookedCabin")]
    public class BookedCabinClass
    {
        public string Code { get; set; }
        public string ProductCode { get; set; }

        public static explicit operator BookedCabinClass(Infrastructure.Models.CheckInMS.BookedCabinClass source) => new()
        {
            Code = source.Code,
            ProductCode = source.ProductCode
        };
    }
}
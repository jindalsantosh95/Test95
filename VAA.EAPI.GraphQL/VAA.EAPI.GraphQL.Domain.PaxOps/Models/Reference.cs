using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Reference")]
    public class Reference
    {
        [GraphQLDescription("Booking reference.")]
        public string PnrLocator { get; set; }

        [GraphQLDescription("Booking creation date.")]
        public LocalDate? CreatedDate { get; set; }

        public static explicit operator Reference(Infrastructure.Models.CheckInMS.Reference source) => new()
        {
            PnrLocator = source.PnrLocator,
            CreatedDate = source.CreatedDate
        };
    }
}

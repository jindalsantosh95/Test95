using HotChocolate;
using VAA.EAPI.GraphQL.Common.Middleware;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    [VAAFilterConvention(filterName: "VHolsContactDetailsFilter")]
    [GraphQLName("VHolsContactDetails")]
    public class ContactDetails
    {
        public string? Email { get; set; }
        public string? PreferredPhone { get; set; }
        public string? AlternativePhone { get; set; }
    }
}


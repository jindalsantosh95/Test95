using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketContactDetails")]
    public class ContactDetails
    {
        public string EmailAddress { get; set; }
    }
}
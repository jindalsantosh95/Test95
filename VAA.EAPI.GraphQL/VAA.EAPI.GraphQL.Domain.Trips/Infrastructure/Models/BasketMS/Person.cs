using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketPassenger")]
    public class Person
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerId { get; set; }
        public FQTMembership MembershipDetails { get; set; } = new();

        public static explicit operator Person(Passenger source)
        {
            return new Person
            {
                Id = source.PassengerId,
                Type = source.PassengerType?.Code,
                FirstName = source.FirstName,
                LastName = source.LastName,
                CustomerId = $"{source.LastNameNumber}-{source.FirstNameNumber}",
                MembershipDetails = new()
                {
                    FQTNumber = source.LoyaltyMembership?.Number,
                    FQTProgramme = source.LoyaltyMembership?.ProgrammeCode
                }
            };
        }
    }
}
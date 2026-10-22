using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class PassengerDetail
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public FQTMembership MembershipDetails { get; set; }
        public BookingClass? BookingClass { get; set; }
    }
}

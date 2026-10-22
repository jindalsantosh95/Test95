using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class Passenger
    {
        public string PassengerId { get; set; }
        public string? PassengerFirstNin { get; set; }
        public string? PassengerLastNin { get; set; }
        public PassengerType PassengerType { get; set; }
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public LocalDate? DateOfBirth { get; set; }
        public string? AssociatedAdultFullName { get; set; }
        public string? AssociatedAdultPassengerId { get; set; }
        public bool? IsInfantPassenger { get; set; }
        public string? Email { get; set; }
        public Phone? Phone { get; set; }
        public LoyaltyMembership LoyaltyMembership { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<CheckInRequirements>? SliceRequirements { get; set; }
        public CheckInRequirements Requirements { get; set; }
        public EmergencyContact EmergencyContact { get; set; }
        public ContactTracing ContactTracing { get; set; }

    }
}
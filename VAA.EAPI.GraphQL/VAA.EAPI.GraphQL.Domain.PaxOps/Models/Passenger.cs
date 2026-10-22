using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Passenger")]
    public class Passenger
    {
        [GraphQLDescription("Passenger identifier for use within one Check In or Travel Requirements session.")]
        public string PassengerId { get; set; }

        [GraphQLDescription("First passenger NIN value. The two NINs can be used to identify a specific passenger on a given booking across different APIs and sessions.")]
        public string PassengerFirstNin { get; set; }

        [GraphQLDescription("Last passenger NIN value. The two NINs can be used to identify a specific passenger on a given booking across different APIs and sessions.")]
        public string PassengerLastNin { get; set; }

        [GraphQLDescription("Passenger type metadata, when available.")]
        public PassengerType PassengerType { get; set; }

        [GraphQLDescription("The given name(s) of the passenger the booking was created for.")]
        public string GivenNames { get; set; }

        [GraphQLDescription("The surname of the passenger the booking was created for.")]
        public string Surname { get; set; }

        [GraphQLDescription("For infant passengers, the full name of the adult that they will be travelling with.")]
        public string? AssociatedAdultFullName { get; set; }

        [GraphQLDescription("For infant passengers, the PassengerId of the adult that they will be travelling with.")]
        public string? AssociatedAdultPassengerId { get; set; }

        [GraphQLDescription("Indicates whether the passenger is an infant, when available.")]
        public bool? IsInfantPassenger { get; set; }

        [GraphQLDescription("Passenger date of birth in ISO yyyy-MM-dd format, when available.")]
        public LocalDate? DateOfBirth { get; set; }

        [GraphQLDescription("Passenger email, when available.")]
        public string Email { get; set; }

        [GraphQLDescription("Passenger phone details, when available.")]
        public Phone? Phone { get; set; }

        [GraphQLDescription("Loyalty programme details, when available.")]
        public LoyaltyMembership LoyaltyMembership { get; set; }

        [GraphQLDescription("Travel documents known for the passenger.")]
        public IEnumerable<Document> Documents { get; set; }

        [GraphQLDeprecated("Not used.")]
        public IEnumerable<Slice> Slices { get; set; }

        [GraphQLDescription("Check-in flow requirements object, with the status for the current slice. Will be null in travel requirements flow.")]
        public CheckInRequirements Requirements { get; set; }

        [GraphQLDescription("Per-slice requirements, used for the travel requirements flow. Will be null in the Check-in flow.")]
        public IEnumerable<CheckInRequirements>? SliceRequirements { get; set; }

        [GraphQLDescription("Emergency contact details, when available.")]
        public EmergencyContact.EmergencyContact EmergencyContact { get; set; }

        [GraphQLDescription("Contact tracing details, when available.")]
        public ContactTracing.ContactTracing ContactTracing { get; set; }


        public static explicit operator Passenger(Infrastructure.Models.CheckInMS.Passenger source) => new()
        {
            PassengerId = source.PassengerId,
            PassengerFirstNin = source.PassengerFirstNin,
            PassengerLastNin = source.PassengerLastNin,
            PassengerType = source.PassengerType != null ? (PassengerType)source.PassengerType : null,
            GivenNames = source.GivenNames,
            Surname = source.Surname,
            DateOfBirth = source.DateOfBirth,
            AssociatedAdultFullName = source.AssociatedAdultFullName,
            IsInfantPassenger = source.IsInfantPassenger,
            AssociatedAdultPassengerId = source.AssociatedAdultPassengerId,
            Email = source.Email,
            Phone = source.Phone != null ? (Phone)source.Phone : null,
            LoyaltyMembership = source.LoyaltyMembership != null ? (LoyaltyMembership)source.LoyaltyMembership : null,
            Documents = source.Documents?.Select(d => (Document)d),
            Requirements = source.Requirements != null ? (CheckInRequirements)source.Requirements : null,
            SliceRequirements = source.SliceRequirements?.Select(sr => (CheckInRequirements)sr),
            EmergencyContact = source.EmergencyContact,
            ContactTracing = source.ContactTracing
        };
    }
}

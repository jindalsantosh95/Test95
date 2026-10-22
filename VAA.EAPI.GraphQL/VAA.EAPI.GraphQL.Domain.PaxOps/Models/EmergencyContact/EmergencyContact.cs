using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact
{
    public class EmergencyContact
    {
        [GraphQLDescription("Passenger ID the contact applies to.")]
        public string PassengerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [GraphQLDescription("ISO-2 country code for the emergency contact phone number, using ISO 3166-1 alpha-2 format, for example GB or US.")]
        public string PhoneCountryCode { get; set; }
        public string PhoneNumber { get; set; }

        [GraphQLDescription("Whether the passenger refused to provide emergency contact details.")]
        public bool PassengerRefused { get; set; }
    }
}

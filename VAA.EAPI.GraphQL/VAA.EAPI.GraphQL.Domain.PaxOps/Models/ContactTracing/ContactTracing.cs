using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing
{
    public class ContactTracing
    {
        [GraphQLDescription("Passenger ID the contact tracing data applies to.")]
        public string PassengerId { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }

        [GraphQLDescription("Whether the passenger is in transit.")]
        public bool InTransit { get; set; }

        [GraphQLDescription("Whether the passenger has no email, when supplied.")]
        public bool? NoEmail { get; set; }

        [GraphQLDescription("Whether no secondary phone number is supplied, when supplied.")]
        public bool? NoSecondaryPhoneNumber { get; set; }
        public string PrimaryPhoneNumber { get; set; }

        [GraphQLDescription("International dialing code for the primary phone number.")]
        public string PrimaryPhoneNumberCountryCode { get; set; }
        public string SecondaryPhoneNumber { get; set; }

        [GraphQLDescription("International dialing code for the secondary phone number.")]
        public string SecondaryPhoneNumberCountryCode { get; set; }
    }
}

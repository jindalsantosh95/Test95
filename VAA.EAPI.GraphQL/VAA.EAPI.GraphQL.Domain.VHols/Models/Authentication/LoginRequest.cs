using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.Authentication
{
    [GraphQLDescription("Login Request: Input object containing details of a VHols booking to create a session.")]
    public class LoginRequest
    {
        [GraphQLDescription("Surname of the lead passenger.")]
        public string Surname { get; set; }
        [GraphQLDescription("Date of birth of the lead passenger. [Format: yyyy-MM-dd]")]
        public string DateOfBirth { get; set; }
        [GraphQLDescription("VHols booking reference number. [Format: 7 digit number]")]
        public string BookingReference { get; set; }
    }
}

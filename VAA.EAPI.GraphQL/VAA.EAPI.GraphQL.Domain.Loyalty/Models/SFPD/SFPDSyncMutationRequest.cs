using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD
{
    public class SFPDSyncMutationRequest
    {
        [GraphQLDescription("First Name")]
        public string FirstName { get; set; }
        [GraphQLDescription("Last Name")]
        public string LastName { get; set; }
        [GraphQLDescription("Gender")]
        public string Gender { get; set; }
        [GraphQLDescription("Date Of Birth")]
        public string DateOfBirth { get; set; }
        [GraphQLDescription("Customer Id")]
        public string CustomerId { get; set; }
        [GraphQLDescription("Fc Number")]
        public string FcNumber { get; set; }
        [GraphQLDescription("Channel")]
        public string EnrollmentChannel { get; set; }
        [GraphQLDescription("Customer Type")]
        public string CustomerType { get; set; }

    }
}

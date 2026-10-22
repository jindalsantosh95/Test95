namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request
{
    public class UserAccountDetails
    {
        [GraphQLDescription("First Name")]
        public string FirstName { get; set; } = null!;

        [GraphQLDescription("Last Name")]
        public string LastName { get; set; } = null!;

        [GraphQLDescription("Email Address")]
        public string EmailAddress { get; set; } = null!;
    }
}

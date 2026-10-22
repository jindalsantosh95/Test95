using HotChocolate;
namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
{
    public class Email
    {
        [GraphQLName("EmailAddress")]
        public string emailId { get; set; }
        [GraphQLName("Flag to indicate if preferred email")]
        public string PreferredFlag { get; set; }
        [GraphQLName("Description")]
        public string Description { get; set; }
        [GraphQLName("Flag to indicate if Email is valid")]
        public string Valid { get; set; }
        public string Usage { get; set; }
    }
}

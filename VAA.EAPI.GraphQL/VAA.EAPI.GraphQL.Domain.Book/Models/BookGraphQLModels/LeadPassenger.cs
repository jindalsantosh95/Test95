namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    public class LeadPassenger
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public GQLAddress Address { get; set; }
        public ContactDetails Contact { get; set; }
    }
}

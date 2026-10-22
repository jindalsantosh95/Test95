namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    [GraphQLName("CustomerSearch")]
    public class Customer
    {
        public string CustId { get; set; }
        public string? Title { get; set; }
        public string Ptc { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public Contacts? Contacts { get; set; }
        public Address? Addresses { get; set; }
        public LoyaltyMemberDetails? LoyaltyMemberDetails { get; set; }
        public IEnumerable<TravelDocument>? TravelDocuments { get; set; }
        public string? FirstNameNumber { get; set; }
        public string? LastNameNumber { get; set; }
    }
}
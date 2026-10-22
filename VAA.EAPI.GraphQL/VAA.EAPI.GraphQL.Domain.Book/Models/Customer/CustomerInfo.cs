using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Customer
{
    public class CustomerInfo
    {
        public string CustId { get; set; }
        public string Title { get; set; }
        public string Ptc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
        public Address Addresses { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
        public IEnumerable<TravelDocument> TravelDocuments { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public Preferences Preferences { get; set; }
        public string LinkedInfantId { get; set; }
    }
}

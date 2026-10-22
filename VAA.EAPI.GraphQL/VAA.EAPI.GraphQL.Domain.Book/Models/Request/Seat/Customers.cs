using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat
{
    public class Customer
    {
        public string CustId { get; set; }
        public string Title { get; set; }
        public string Ptc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
    }
}

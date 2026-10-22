namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public sealed class FlyingClubCustomer
    {
        public string CustomerId { get; set; }
        public string LoyaltyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Status { get; set; }
    }
}

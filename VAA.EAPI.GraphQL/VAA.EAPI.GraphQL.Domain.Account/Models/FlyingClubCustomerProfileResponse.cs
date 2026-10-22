namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public sealed class FlyingClubCustomerProfileResponse
    {
        public FlyingClubCustomer Customer { get; set; }
        public FlyingClubProfile Profile { get; set; }
        public FlyingClubAccount Account { get; set; }
    }
}

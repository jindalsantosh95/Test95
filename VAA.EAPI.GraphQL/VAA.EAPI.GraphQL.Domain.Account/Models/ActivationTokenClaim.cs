namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class ActivationTokenClaim
    {
        public string ObjectId { get; set; }
        public string FcNumber { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerId { get; set; }
        public string CountryCode { get; set; }
    }
}

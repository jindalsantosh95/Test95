namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch
{
    public class CustomerProfileRequest
    {
        public string Email { get; set; }
        public string MemberNumber { get; set; }
        public string CustomerId { get; set; }
    }
}

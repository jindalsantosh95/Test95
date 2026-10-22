namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class ProfileUpdateResponse
    {
        public ProfileUpdateResponse(string status = default)
        {
            Status = status;
        }

        public string Status { get; set; }
    }
}

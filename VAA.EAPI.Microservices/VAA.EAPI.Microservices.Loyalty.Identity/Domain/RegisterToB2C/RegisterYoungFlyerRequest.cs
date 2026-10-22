using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

public sealed class RegisterYoungFlyerRequest
{
    public CustomerAccountDetails CustomerDetails { get; set; }
    public bool AccountEnabled { get; set; } = true;
    public bool SuppressCommunication { get; set; }
}

using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class AccountSignInRequest
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public string FlyingClubNumber { get; set; }
        public string OTP { get; set; }
        public AccountSignInCustomerProfile CustomerProfile { get; set; }
    }

}

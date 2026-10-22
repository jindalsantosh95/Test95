using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class AccountResetRequest
    {
        public string Password { get; set; }
        public string Username { get; set; }
    }
}

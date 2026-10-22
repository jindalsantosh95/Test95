using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class NewPasswordRequest
    {
        public string ContinuationToken { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}

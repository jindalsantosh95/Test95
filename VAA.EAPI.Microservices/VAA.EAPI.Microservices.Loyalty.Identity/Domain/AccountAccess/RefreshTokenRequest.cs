using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public IEnumerable<string> Scopes { get; set; }

    }
}

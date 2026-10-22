using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}

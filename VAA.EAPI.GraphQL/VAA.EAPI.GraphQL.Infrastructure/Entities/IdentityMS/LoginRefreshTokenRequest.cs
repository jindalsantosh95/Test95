using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class LoginRefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public IEnumerable<string> Scopes { get; set; }

        public static explicit operator LoginRefreshTokenRequest(RefreshTokenRequest request)
        {
            return new LoginRefreshTokenRequest()
            {
                RefreshToken = request?.Token,
                Scopes = request?.Scopes

            };
        }
    }
}

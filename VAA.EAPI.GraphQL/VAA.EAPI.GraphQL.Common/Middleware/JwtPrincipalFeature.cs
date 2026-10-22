using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Common.Middleware
{
    /// <summary>
    /// Holds Token and Principle to allow it to be carried through from Authentication Context to Authorization Context
    /// </summary>
    public class JwtPrincipalFeature
    {
        public JwtPrincipalFeature(ClaimsPrincipal principal, AuthToken accessToken)
        {
            Principal = principal;
            AccessToken = accessToken;
        }

        public ClaimsPrincipal Principal { get; }

        /// <summary>
        /// The access token that was used for this
        /// request. Can be used to acquire further
        /// access tokens with the on-behalf-of flow.
        /// </summary>
        public AuthToken AccessToken { get; }
    }
}

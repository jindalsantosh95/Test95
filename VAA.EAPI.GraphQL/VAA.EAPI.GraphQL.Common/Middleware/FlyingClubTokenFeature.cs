using System;

namespace VAA.EAPI.GraphQL.Common.Middleware
{
    /// <summary>
    /// Carries an untrusted Flying Club broker token to the resolver that
    /// delegates authoritative validation to the Loyalty Identity service.
    /// </summary>
    public sealed class FlyingClubTokenFeature
    {
        public FlyingClubTokenFeature(string token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public string Token { get; }
    }
}

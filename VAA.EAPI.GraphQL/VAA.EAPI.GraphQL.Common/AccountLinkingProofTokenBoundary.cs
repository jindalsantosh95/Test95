using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;

namespace VAA.EAPI.GraphQL.Common
{
    public static class AccountLinkingProofTokenBoundary
    {
        private static readonly JwtSecurityTokenHandler Handler = new();

        public static bool MatchesAuthenticatedToken(string authorization, JwtPrincipalFeature feature)
        {
            if (authorization.IsNullOrWhiteSpace() || authorization.Contains(',') ||
                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) || authorization.Length <= 7 ||
                authorization.AsSpan(7).Contains(' ')) return false;
            var token = authorization[7..];
            return feature?.Principal?.Identity?.IsAuthenticated == true &&
                string.Equals(feature.AccessToken?.Token, token, StringComparison.Ordinal) && HasTrustedIssuerAudience(token);
        }

        public static bool HasTrustedIssuerAudience(string token)
        {
            if (token.IsNullOrWhiteSpace() || !Handler.CanReadToken(token)) return false;
            JwtSecurityToken jwt;
            try { jwt = Handler.ReadJwtToken(token); }
            catch (Exception exception) when (exception is ArgumentException or SecurityTokenException) { return false; }
            if (jwt.Issuer.IsNullOrWhiteSpace() || jwt.Audiences.Count() != 1) return false;
            var aadV2Issuer = $"{ConfigReader.Get(Constants.Environment.AADAuthority)}/{ConfigReader.Get(Constants.Environment.AADTenant)}/v2.0";
            var aadV1Issuer = $"{ConfigReader.Get(Constants.Environment.AADTokenV1Issuer)}/{ConfigReader.Get(Constants.Environment.AADTenant)}/";
            var b2cIssuer = $"{ConfigReader.Get(Constants.Environment.AB2CAuthority)}/{ConfigReader.Get(Constants.Environment.AB2CTenant)}/v2.0/";
            var entraIssuer = $"https://{ConfigReader.Get(Constants.Environment.EntraTenant)}.ciamlogin.com/{ConfigReader.Get(Constants.Environment.EntraTenant)}/v2.0";
            string configuredAudiences;
            if (jwt.Issuer == aadV1Issuer || jwt.Issuer == aadV2Issuer || jwt.Issuer == ConfigReader.Get(Constants.Environment.IdentityIssuer))
                configuredAudiences = ConfigReader.Get(Constants.Environment.EAPIGraphAppID);
            else if (jwt.Issuer == b2cIssuer || jwt.Issuer == entraIssuer)
                configuredAudiences = ConfigReader.Get(Constants.Environment.AB2CAudience);
            else return false;
            return new HashSet<string>(configuredAudiences.Split(Constants.Application.CommaDelimiter, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal)
                .Contains(jwt.Audiences.Single());
        }
    }
}

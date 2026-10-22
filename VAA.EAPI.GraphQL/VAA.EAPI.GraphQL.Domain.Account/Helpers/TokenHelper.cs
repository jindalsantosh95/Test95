using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Entities;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Helpers
{
    public static class TokenHelper
    {
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
        private const string EmailAddressClaim = "emailAddress";
        private const string ContinuationTokenSourceClaim = "continuationTokenSource";
        private const string CustomOtpContinuationToken = "custom_otp";

        private static readonly Tenant[] Tenants =
        {
            new()
            {
                Type = IssuerType.Identity,
                Issuer = ConfigReader.Get(Environment.IdentityActivationTokenIssuer)
            }
        };

        public static bool HasTokenExpired(string jwtToken)
        {
            return CheckTokenIsExpired(jwtToken);
        }

        public static bool CheckValidIssuer(string token)
        {
            return Tenants.FirstOrDefault(x => x.Issuer.Equals(JwtSecurityTokenHandler.ReadToken(token).Issuer, StringComparison.OrdinalIgnoreCase)) != null;
        }

        private static long GetTokenExpirationTime(string token)
        {
            JwtSecurityToken jwtSecurityToken = JwtSecurityTokenHandler.ReadJwtToken(token);
            string tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;

            long.TryParse(tokenExp, out var ticks);

            return ticks;
        }

        private static bool CheckTokenIsExpired(string token)
        {
            return DateTime.UtcNow.ToUniversalTime() > DateTimeOffset.FromUnixTimeSeconds(GetTokenExpirationTime(token)).UtcDateTime;
        }

        public static ActivationTokenClaim GetClaims(string token)
        {
            return JwtSecurityTokenHandler.CanReadToken(token) ? Json.Deserialize<ActivationTokenClaim>(JwtSecurityTokenHandler.ReadJwtToken(token)?.Claims?.ToList().FirstOrDefault(x => x.Type == "Claim")?.Value) : default;
        }

        public static string GetEmailFromToken(string token)
        {
            return JwtSecurityTokenHandler.ReadJwtToken(token).Claims.FirstOrDefault(claim =>
                string.Equals(claim.Type, Constants.Application.Email, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(claim.Type, EmailAddressClaim, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string GetFcNumberFromToken(string token)
        {
            return JwtSecurityTokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type.Is(Constants.Application.FcNumber)).Value;
        }

        public static bool CanReadToken(string token)
        {
            return JwtSecurityTokenHandler.CanReadToken(token);
        }

        public static bool IsCustomOtpContinuationToken(string token)
        {
            return HasCustomOtpContinuationMarker(token);
        }

        public static bool HasCustomOtpContinuationMarker(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !CanReadToken(token))
                return false;

            try
            {
                return HasCustomOtpContinuationClaim(JwtSecurityTokenHandler.ReadJwtToken(token).Claims);
            }
            catch
            {
                return false;
            }
        }

        private static bool HasCustomOtpContinuationClaim(IEnumerable<Claim> claims)
        {
            return claims.Any(claim => claim.Type.Is(ContinuationTokenSourceClaim) && claim.Value.Is(CustomOtpContinuationToken));
        }
    }
}

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper
{
    public class ValidationTokenHelper : BaseIdHelper
    {
        private static readonly string jwtKey = ConfigReader.Get(Constants.Environment.ValidationJwtKey);
        private const string EmailAddressClaim = "emailAddress";
        private const string EmailClaim = "Email";
        private const string ContinuationTokenSourceClaim = "continuationTokenSource";
        private const string CustomOtpContinuationToken = "custom_otp";
        private const string MyAccountActivationTokenIssuer = "MyAccountActivationTokenIssuer";
        private const int MinimumHmacSha256KeySizeInBytes = 32;
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

        public static async Task<string> CreateToken(string emailAddress, bool isContinuationToken = false)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address is required to create validation token.", nameof(emailAddress));

            var claims = await MergeClaim(new List<Claim>(), emailAddress, jwtKey, EmailAddressClaim, emailAddress);
            claims = await MergeClaim(claims, emailAddress, jwtKey, EmailClaim, emailAddress);

            if (isContinuationToken)
                claims.Add(new Claim(ContinuationTokenSourceClaim, CustomOtpContinuationToken));

            return CreateTokenString(claims, DateTime.UtcNow.AddMinutes(ConfigReader.Get<int>(Constants.Environment.OtpTokenExpiryTime)));
        }

        public static Task<string> GetEmailAddress(string otpValidationToken)
        {
            var principal = JwtSecurityTokenHandler.ValidateToken(otpValidationToken, CreateValidationParameters(), out _);
            return Task.FromResult(principal.Claims.FirstOrDefault(claim => claim.Type == EmailAddressClaim)?.Value ?? string.Empty);
        }

        private static string CreateTokenString(List<Claim> claims, DateTime expiry)
        {
            var issuer = ConfigReader.Try(
                Constants.Environment.IdentityActivationTokenIssuer,
                ConfigReader.Try(MyAccountActivationTokenIssuer, string.Empty));

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("Identity activation token issuer is not configured.");

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(GetJwtKeyBytes()), SecurityAlgorithms.HmacSha256)
            };

            descriptor.Issuer = issuer;

            return JwtSecurityTokenHandler.WriteToken(JwtSecurityTokenHandler.CreateToken(descriptor));
        }

        private static TokenValidationParameters CreateValidationParameters()
        {
            var issuer = ConfigReader.Try(
                Constants.Environment.IdentityActivationTokenIssuer,
                ConfigReader.Try(MyAccountActivationTokenIssuer, string.Empty));

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("Identity activation token issuer is not configured.");

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(GetJwtKeyBytes()),
                ValidateIssuer = true,
                IssuerValidator = (tokenIssuer, _, __) =>
                    string.IsNullOrWhiteSpace(tokenIssuer) || string.Equals(tokenIssuer, issuer, StringComparison.Ordinal)
                        ? issuer
                        : throw new SecurityTokenInvalidIssuerException($"Invalid issuer '{tokenIssuer}'."),
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
            };
        }
        private static byte[] GetJwtKeyBytes()
        {
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("ValidationJwtKey is not configured.");

            var jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);

            if (jwtKeyBytes.Length < MinimumHmacSha256KeySizeInBytes)
                throw new InvalidOperationException("ValidationJwtKey must be at least 32 bytes for HS256 signing.");

            return jwtKeyBytes;
        }
    }
}

using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public sealed class KeyVaultFlyingClubTokenValidator : IFlyingClubTokenValidator
    {
        private const int JwtSegmentCount = 3;
        private const int ClockSkewSeconds = 30;
        private const string MalformedTokenMessage = "The broker token is malformed.";
        private readonly CryptographyClient _cryptographyClient;
        private readonly TimeProvider _timeProvider;

        public KeyVaultFlyingClubTokenValidator(TimeProvider timeProvider)
            : this(new CryptographyClient(new Uri(ConfigReader.Get(Environment.FlyingClubTokenKeyId)), new DefaultAzureCredential()), timeProvider)
        {
        }

        internal KeyVaultFlyingClubTokenValidator(CryptographyClient cryptographyClient, TimeProvider timeProvider)
        {
            _cryptographyClient = cryptographyClient ?? throw new ArgumentNullException(nameof(cryptographyClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<string> ValidateAsync(string token, string authenticatedCallerApplicationId, CancellationToken cancellationToken)
        {
            if (token.IsNullOrWhiteSpace())
                throw new SecurityTokenValidationException("The broker token is required.");
            if (authenticatedCallerApplicationId.IsNullOrWhiteSpace())
                throw new SecurityTokenValidationException("The authenticated calling application is required.");

            var expectedAuthorizedParty = ConfigReader.Get(Environment.FlyingClubTokenAuthorizedParty);
            if (expectedAuthorizedParty.IsNullOrWhiteSpace())
                throw new InvalidOperationException("The Flying Club token authorized party is not configured.");
            if (!authenticatedCallerApplicationId.Is(expectedAuthorizedParty))
                throw new SecurityTokenValidationException("The authenticated calling application is invalid.");

            var segments = token.Split('.');
            if (segments.Length != JwtSegmentCount)
                throw new SecurityTokenMalformedException(MalformedTokenMessage);

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
                throw new SecurityTokenMalformedException(MalformedTokenMessage);

            JwtSecurityToken jwt;
            byte[] signature;
            try
            {
                jwt = tokenHandler.ReadJwtToken(token);
                signature = Base64UrlEncoder.DecodeBytes(segments[2]);
            }
            catch (Exception exception)
            {
                throw new SecurityTokenMalformedException(MalformedTokenMessage, exception);
            }

            if (!string.Equals(jwt.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
                throw new SecurityTokenInvalidAlgorithmException("The broker token algorithm is invalid.");
            if (!jwt.Header.Kid.Is(_cryptographyClient.KeyId))
                throw new SecurityTokenInvalidSigningKeyException("The broker token signing key is invalid.");

            var verification = await _cryptographyClient.VerifyDataAsync(SignatureAlgorithm.RS256, Encoding.UTF8.GetBytes($"{segments[0]}.{segments[1]}"), signature, cancellationToken);
            if (!verification.IsValid)
                throw new SecurityTokenInvalidSignatureException("The broker token signature is invalid.");

            var now = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
            var expectedIssuer = ConfigReader.Get(Environment.FlyingClubTokenIssuer);
            var expectedAudience = ConfigReader.Get(Environment.FlyingClubTokenAudience);
            if (!string.Equals(jwt.Issuer, expectedIssuer, StringComparison.Ordinal) ||
                !jwt.Audiences.Contains(expectedAudience, StringComparer.Ordinal) ||
                jwt.Payload.NotBefore is null || jwt.Payload.Expiration is null ||
                jwt.Payload.NotBefore > now + ClockSkewSeconds || jwt.Payload.Expiration <= now - ClockSkewSeconds)
                throw new SecurityTokenValidationException("The broker token claims are invalid.");

            var authorizedParty = jwt.Claims.FirstOrDefault(x => x.Type.Is(Constants.Application.AuthorizedPartyClaim))?.Value;
            if (!authorizedParty.Is(expectedAuthorizedParty))
                throw new SecurityTokenValidationException("The broker token calling application is invalid.");
            var isLinkedBrokerToken = jwt.Claims.Any(x => x.Type.Is("token_use") && x.Value.Is("linked"));
            if (isLinkedBrokerToken)
                throw new SecurityTokenValidationException("The broker token cannot be used here.");

            var accountClaimType = ConfigReader.Get(Environment.FlyingClubTokenClaimType);
            var accountNumber = jwt.Claims.FirstOrDefault(x => x.Type.Is(accountClaimType))?.Value;
            if (accountNumber.IsNullOrWhiteSpace())
                throw new SecurityTokenValidationException("The broker token account claim is missing.");

            return accountNumber;
        }
    }
}

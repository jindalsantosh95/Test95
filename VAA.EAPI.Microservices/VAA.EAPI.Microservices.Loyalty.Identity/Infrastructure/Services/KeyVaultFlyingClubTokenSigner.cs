using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public sealed class KeyVaultFlyingClubTokenSigner : IFlyingClubTokenSigner
    {
        private readonly CryptographyClient _cryptographyClient;

        public KeyVaultFlyingClubTokenSigner()
            : this(new CryptographyClient(
                new Uri(ConfigReader.Get(Environment.FlyingClubTokenKeyId)),
                new DefaultAzureCredential()))
        {
        }

        internal KeyVaultFlyingClubTokenSigner(CryptographyClient cryptographyClient)
        {
            _cryptographyClient = cryptographyClient ?? throw new ArgumentNullException(nameof(cryptographyClient));
        }

        public async Task<string> SignAsync(IReadOnlyDictionary<string, object> claims, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(claims);

            var header = Base64UrlEncoder.Encode(Json.Serialize(new Dictionary<string, object>
            {
                ["alg"] = SecurityAlgorithms.RsaSha256,
                ["typ"] = "JWT",
                ["kid"] = _cryptographyClient.KeyId
            }));
            var payload = Base64UrlEncoder.Encode(Json.Serialize(claims));
            var signingInput = $"{header}.{payload}";
            var signature = await _cryptographyClient.SignDataAsync(
                SignatureAlgorithm.RS256,
                Encoding.UTF8.GetBytes(signingInput),
                cancellationToken);

            return $"{signingInput}.{Base64UrlEncoder.Encode(signature.Signature)}";
        }
    }
}

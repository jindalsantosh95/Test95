using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public sealed class LinkingTransactionValidator : ILinkingTransactionValidator
    {
        private readonly TimeProvider _timeProvider;

        public LinkingTransactionValidator(TimeProvider timeProvider) =>
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        public ValidatedLinkingTransaction ValidateCreation(CreateLinkingProofRequest request)
        {
            if (request?.Scope != "account:read") throw Invalid();
            var result = Validate(request.TransactionId, request.ClientId, request.Resource);
            // Legacy repository field names are internal only. No signed assertion or browser
            // nonce is required: the persisted account/binding and replacement budget are EAPI-owned.
            result.AssertionReplayId = result.BindingHash;
            result.Nonce = result.BindingHash;
            result.ProofReplayId = Guid.NewGuid().ToString("N");
            result.ExpiresAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(120);
            return result;
        }

        public ValidatedLinkingTransaction ValidateRedemption(RedeemLinkingProofRequest request) =>
            request is null ? throw Invalid() : Validate(request.TransactionId, request.ClientId, request.Resource);

        private static ValidatedLinkingTransaction Validate(string transactionId, string clientId, string resource)
        {
            if (!Valid(transactionId, 128) || !Valid(clientId, 256) || !Valid(resource, 2048)) throw Invalid();
            var expectedClient = ConfigReader.Get(Environment.AccountLinkingTransactionClientId);
            var expectedResource = ConfigReader.Get(Environment.AccountLinkingTransactionResource);
            if (expectedClient.IsNullOrWhiteSpace() || expectedResource.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Account-linking client/resource policy is not configured.");
            if (!string.Equals(clientId, expectedClient, StringComparison.Ordinal) ||
                !string.Equals(resource, expectedResource, StringComparison.Ordinal)) throw Invalid();

            // Length-safe canonical representation; identifiers bind the proof but do not
            // attest to external OAuth state or consent. Those remain the proxy's responsibility.
            var binding = JsonConvert.SerializeObject(new[] { transactionId, clientId, resource, "account:read" });
            return new ValidatedLinkingTransaction
            {
                TransactionId = transactionId,
                BindingHash = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(binding)))
            };
        }

        private static bool Valid(string value, int maximum) =>
            !value.IsNullOrWhiteSpace() && value.Length <= maximum && !value.Any(char.IsControl);

        private static SecurityTokenValidationException Invalid() => new("The account-linking transaction is invalid.");
    }
}

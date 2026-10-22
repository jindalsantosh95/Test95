using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public sealed class RedisAccountLinkingProofRepository : IAccountLinkingProofRepository, IDisposable
    {
        private readonly ISingleUseDistributedCacheService _cache;
        public RedisAccountLinkingProofRepository(ISingleUseDistributedCacheService cache) =>
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        public async Task<DateTime> StoreBoundAsync(string proofHash, CreateLinkingProofRequest request,
            string consentReference, TimeSpan lifetime, CancellationToken cancellationToken)
        {
            var window = request.ExpiresAtUtc - DateTime.UtcNow;
            if (window <= TimeSpan.Zero) throw new AccountLinkingProofConflictException("The linking transaction has expired.");
            var expiry = await _cache.SetAsync(proofHash, new AccountLinkingRecord
            {
                FlyingClubAccountNumber = request.FlyingClubAccountNumber,
                ConsentReference = consentReference
            }, lifetime, CacheMode.SingleUse, new SingleUseCacheOptions
            {
                ScopeKey = Scope(request.TransactionId),
                Binding = request.TransactionBindingHash,
                Owner = JsonConvert.SerializeObject(new[] { request.FlyingClubAccountNumber, request.AssertionReplayId, request.Nonce }),
                IssuanceId = request.ProofReplayId,
                ScopeLifetime = window,
                MaximumReplacements = 3
            }, cancellationToken);
            return expiry ?? throw new AccountLinkingProofConflictException("The linking transaction or proof was replayed.");
        }

        public async Task<RedeemLinkingProofResponse> ConsumeBoundAsync(string proofHash, string transactionId,
            string transactionBindingHash, CancellationToken cancellationToken)
        {
            var record = await _cache.TryConsumeAsync<AccountLinkingRecord>(
                proofHash, Scope(transactionId), transactionBindingHash, cancellationToken);
            return record is null ? null : new RedeemLinkingProofResponse
            {
                FlyingClubAccountNumber = record.FlyingClubAccountNumber,
                ConsentReference = record.ConsentReference
            };
        }

        private static string Scope(string transactionId) => "account-linking:" + transactionId;
        public void Dispose() { } // DI owns the cache and its Redis connection.
    }
}

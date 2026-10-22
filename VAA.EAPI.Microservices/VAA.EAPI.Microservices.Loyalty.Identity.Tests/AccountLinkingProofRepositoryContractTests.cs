using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

public sealed class AccountLinkingProofRepositoryContractTests
{
    [Fact]
    public async Task FourthReplacementIsRejectedWithoutLosingCurrentProof()
    {
        var redisConnection = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (redisConnection.IsNullOrWhiteSpace())
            Assert.Skip("Set ACCOUNT_LINKING_REDIS_TEST_CONNECTION to run the Redis contract test.");

        using var connection = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        using var services = CreateServices(connection);
        var repository = services.GetRequiredService<IAccountLinkingProofRepository>();
        var request = new CreateLinkingProofRequest
        {
            TransactionId = Guid.NewGuid().ToString("N"), TransactionBindingHash = "binding",
            FlyingClubAccountNumber = "1234567890", AssertionReplayId = "assertion", Nonce = "nonce",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(2)
        };
        for (var attempt = 0; attempt <= 3; attempt++)
        {
            request.ProofReplayId = $"replay-{attempt}";
            await repository.StoreBoundAsync($"proof-{attempt}", request, "consent",
                TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
        }
        request.ProofReplayId = "replay-4";
        await Assert.ThrowsAsync<AccountLinkingProofConflictException>(() => repository.StoreBoundAsync(
            "proof-4", request, "consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken));
        var outcome = await repository.ConsumeBoundAsync(
            "proof-3", request.TransactionId, "binding", TestContext.Current.CancellationToken);
        Assert.Equal("1234567890", outcome?.FlyingClubAccountNumber);
    }

    [Fact]
    public async Task ShortProofDoesNotShortenTransactionOrReplayLifetime()
    {
        var redisConnection = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (redisConnection.IsNullOrWhiteSpace()) Assert.Skip("Set ACCOUNT_LINKING_REDIS_TEST_CONNECTION to run the Redis contract test.");
        using var connection = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        using var services = CreateServices(connection);
        var repository = services.GetRequiredService<IAccountLinkingProofRepository>();
        var transaction = Guid.NewGuid().ToString("N");
        var request = new CreateLinkingProofRequest
        {
            TransactionId = transaction,
            TransactionBindingHash = "binding",
            FlyingClubAccountNumber = "1234567890",
            AssertionReplayId = "assertion",
            Nonce = "nonce",
            ProofReplayId = "proof-jti",
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(120)
        };
        await repository.StoreBoundAsync("short-proof", request, "consent", TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        static string Hash(string value) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
        var prefix = $"single-use:{{{Hash("account-linking:" + transaction)}}}:";
        var database = connection.GetDatabase();
        Assert.InRange((await database.KeyTimeToLiveAsync(prefix + "state"))!.Value.TotalSeconds, 100, 120);
        Assert.InRange((await database.KeyTimeToLiveAsync(prefix + "issuance:" + Hash("proof-jti")))!.Value.TotalSeconds, 100, 120);
        Assert.InRange((await database.KeyTimeToLiveAsync(prefix + "item:" + Hash("short-proof")))!.Value.TotalSeconds, 0, 10);
        var persisted = JsonConvert.DeserializeAnonymousType((string)(await database.StringGetAsync(prefix + "item:" + Hash("short-proof")))!,
            new { Payload = string.Empty });
        var record = persisted!.Payload.DecompressTo<AccountLinkingRecord>();
        Assert.Equal("1234567890", record.FlyingClubAccountNumber);
        Assert.Equal("consent", record.ConsentReference);
        await database.KeyDeleteAsync(prefix + "item:" + Hash("short-proof"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.StoreBoundAsync(
            "replayed-proof", request, "consent", TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DistributedCacheBackend_FailsClosedBecauseConsumptionIsNotAtomic()
    {
        using var services = CreateServices();
        var repository = services.GetRequiredService<IAccountLinkingProofRepository>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ConsumeBoundAsync(
            "proof", "transaction", "binding", TestContext.Current.CancellationToken));

        Assert.Contains("atomic Redis backend", exception.Message);
    }

    [Fact]
    public async Task RedisScript_IsAtomicAndPreservesProofOnBindingMismatch()
    {
        var redisConnection = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (redisConnection.IsNullOrWhiteSpace())
            Assert.Skip("Set ACCOUNT_LINKING_REDIS_TEST_CONNECTION to run the Redis contract test.");

        using var firstConnection = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        using var secondConnection = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        using var firstServices = CreateServices(firstConnection);
        using var secondServices = CreateServices(secondConnection);
        var firstRepository = firstServices.GetRequiredService<IAccountLinkingProofRepository>();
        var secondRepository = secondServices.GetRequiredService<IAccountLinkingProofRepository>();
        var hash = $"contract-{Guid.NewGuid():N}";
        var transaction = $"contract-{Guid.NewGuid():N}";
        await firstRepository.StoreBoundAsync(hash, new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890", TransactionId = transaction, TransactionBindingHash = "binding",
            AssertionReplayId = "assertion", Nonce = "nonce", ProofReplayId = "proof",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1)
        }, "consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        var mismatch = await firstRepository.ConsumeBoundAsync(hash, transaction, "wrong", TestContext.Current.CancellationToken);
        Assert.Null(mismatch);

        var attempts = await Task.WhenAll(
            firstRepository.ConsumeBoundAsync(hash, transaction, "binding", TestContext.Current.CancellationToken),
            secondRepository.ConsumeBoundAsync(hash, transaction, "binding", TestContext.Current.CancellationToken));
        Assert.Equal("1234567890", Assert.Single(attempts, value => value is not null)!.FlyingClubAccountNumber);
        Assert.Single(attempts, value => value is null);
    }

    [Fact]
    public async Task BoundRedisScripts_ReplaceCurrentProofAndRejectReplay()
    {
        var redisConnection = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (redisConnection.IsNullOrWhiteSpace())
            Assert.Skip("Set ACCOUNT_LINKING_REDIS_TEST_CONNECTION to run the Redis contract test.");

        using var connection = await ConnectionMultiplexer.ConnectAsync(redisConnection!);
        using var services = CreateServices(connection);
        var repository = services.GetRequiredService<IAccountLinkingProofRepository>();
        var transactionId = $"bound-contract-{Guid.NewGuid():N}";
        var request = new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = transactionId,
            TransactionBindingHash = "binding",
            AssertionReplayId = "assertion-jti",
            Nonce = "nonce",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
            ProofReplayId = "proof-jti-1"
        };
        await repository.StoreBoundAsync("proof-1", request, "consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<AccountLinkingProofConflictException>(() => repository.StoreBoundAsync(
            "proof-replay", request, "ignored-consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken));
        request.ProofReplayId = "proof-jti-2";
        request.FlyingClubAccountNumber = "9999999999";
        await Assert.ThrowsAsync<AccountLinkingProofConflictException>(() => repository.StoreBoundAsync(
            "wrong-account", request, "consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken));
        request.FlyingClubAccountNumber = "1234567890";
        using var replacementServices = CreateServices(connection);
        var replacementRepository = replacementServices.GetRequiredService<IAccountLinkingProofRepository>();
        await replacementRepository.StoreBoundAsync("proof-2", request, "ignored-consent", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        Assert.Null(await repository.ConsumeBoundAsync("proof-1", transactionId, "binding", TestContext.Current.CancellationToken));
        Assert.Null(await repository.ConsumeBoundAsync("proof-2", transactionId, "wrong", TestContext.Current.CancellationToken));
        var attempts = await Task.WhenAll(
            repository.ConsumeBoundAsync("proof-2", transactionId, "binding", TestContext.Current.CancellationToken),
            repository.ConsumeBoundAsync("proof-2", transactionId, "binding", TestContext.Current.CancellationToken));

        var winner = Assert.Single(attempts, value => value is not null);
        Assert.Equal("1234567890", winner!.FlyingClubAccountNumber);
        Assert.Equal("consent", winner.ConsentReference);
        Assert.Single(attempts, value => value is null);
    }

    private static ServiceProvider CreateServices(IConnectionMultiplexer? connection = null)
    {
        var services = new ServiceCollection().AddLogging()
            .AddSingleton<IDistributedCache, DistributedCache>()
            .AddSingleton<ISingleUseDistributedCacheService, DistributedCacheService>()
            .AddSingleton<IAccountLinkingProofRepository, RedisAccountLinkingProofRepository>();
        if (connection is not null) services.AddSingleton(connection);
        return services.BuildServiceProvider();
    }
}

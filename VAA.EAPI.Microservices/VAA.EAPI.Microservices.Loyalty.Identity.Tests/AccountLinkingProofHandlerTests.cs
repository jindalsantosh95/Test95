using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Security.Cryptography;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class AccountLinkingProofHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public AccountLinkingProofHandlerTests()
    {
        ConfigReader.Set(Constants.Environment.AccountLinkingTrustedCallerApplicationId, "graphql-app");
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDistributedCache, DistributedCache>()
            .AddSingleton<ISingleUseDistributedCacheService, DistributedCacheService>()
            .AddSingleton<IAccountLinkingProofRepository, RedisAccountLinkingProofRepository>()
            .AddSingleton<IAccountLinkingProofHandler, AccountLinkingProofHandler>()
            .AddSingleton(TimeProvider.System)
            .BuildServiceProvider();
    }

    [Fact]
    public async Task Create_ReportsUnavailableAtomicBackendAsTechnicalFailure()
    {
        var result = await _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>().CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction",
            TransactionBindingHash = "binding",
            AssertionReplayId = "assertion",
            Nonce = "nonce",
            ProofReplayId = "proof",
            AuthenticatedCallerApplicationId = "graphql-app",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1)
        }, TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Redeem_RejectsMissingTransactionBinding()
    {
        var handler = _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>();
        var result = await handler.RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = new string('A', 43),
            TransactionId = $"transaction-{Guid.NewGuid():N}",
            TransactionBindingHash = ""
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
    }

    [Fact]
    public async Task Redeem_ReportsUnavailableAtomicBackendAsTechnicalFailure()
    {
        var result = await _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>().RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = Convert.ToBase64String(new byte[32]).TrimEnd('=').Replace('+', '-').Replace('/', '_'),

            TransactionId = "transaction",
            TransactionBindingHash = "binding"
        }, TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("Account-linking proof redemption is temporarily unavailable.", result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("123456789012345678901")]
    public async Task Create_PreservesAccountNumberValidation(string? account)
    {
        var result = await _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>().CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = account,
            TransactionId = "transaction", TransactionBindingHash = "binding",
            AssertionReplayId = "assertion", Nonce = "nonce", ProofReplayId = "proof",
            AuthenticatedCallerApplicationId = "graphql-app", ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1)
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
        Assert.Equal("ACCOUNT_LINKING_PROOF_INVALID", Assert.Single(result.Value.Errors).Code);
    }

    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(128, 128, true)]
    [InlineData(129, 128, false)]
    [InlineData(128, 129, false)]
    public async Task Redeem_PreservesTransactionMetadataBounds(int transactionLength, int bindingLength, bool accepted)
    {
        var result = await _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>().RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = new string('A', 43),

            TransactionId = new string('t', transactionLength),
            TransactionBindingHash = new string('b', bindingLength)
        }, TestContext.Current.CancellationToken);

        if (accepted) Assert.True(result.IsFailure); // Real repository rejects the non-atomic mock backend.
        else Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
    }

    [Theory]
    [InlineData(null, "nonce", "proof-replay")]
    [InlineData("assertion-replay", null, "proof-replay")]
    [InlineData("assertion-replay", "nonce", null)]
    public async Task Create_RejectsIncompleteBoundMetadata(string? assertionReplayId, string? nonce, string? proofReplayId)
    {
        var handler = _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>();

        var result = await handler.CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction",
            TransactionBindingHash = "binding",
            AssertionReplayId = assertionReplayId,
            Nonce = nonce,
            ProofReplayId = proofReplayId,
            AuthenticatedCallerApplicationId = "graphql-app"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
        Assert.Null(result.Value.Value);
    }

    [Fact]
    public async Task Create_RejectsUntrustedCallerApplication()
    {
        var handler = _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>();
        var result = await handler.CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction",
            TransactionBindingHash = "binding",
            AssertionReplayId = "assertion-replay",
            Nonce = "nonce",
            ProofReplayId = "proof-replay",
            AuthenticatedCallerApplicationId = "other-app",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1)
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.Forbidden, result.Value.Status);
        Assert.Null(result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("previous-app")]
    public async Task TrustedCallerFallbackRestoresPriorConfiguration(string? previous)
    {
        var original = ConfigReader.Try(Constants.Environment.FlyingClubTokenAuthorizedParty);
        try
        {
            ConfigReader.Set(Constants.Environment.FlyingClubTokenAuthorizedParty, previous!);

            await TrustedCallerFallbackPreservesExplicitOverride("", "graphql-app", "graphql-app", true);

            Assert.Equal(previous, ConfigReader.Try(Constants.Environment.FlyingClubTokenAuthorizedParty));
            Assert.Equal(previous,
                Environment.GetEnvironmentVariable(Constants.Environment.FlyingClubTokenAuthorizedParty));
        }
        finally
        {
            ConfigReader.Set(Constants.Environment.FlyingClubTokenAuthorizedParty, original!);
        }
    }

    [Theory]
    [InlineData("", "graphql-app", "graphql-app", true)]
    [InlineData("override-app", "graphql-app", "graphql-app", false)]
    [InlineData("override-app", "graphql-app", "override-app", true)]
    [InlineData("", "", "graphql-app", false)]
    public async Task TrustedCallerFallbackPreservesExplicitOverride(string configured, string existing, string caller, bool allowed)
    {
        var prior = ConfigReader.Try(Constants.Environment.FlyingClubTokenAuthorizedParty);
        try
        {
            ConfigReader.Set(Constants.Environment.AccountLinkingTrustedCallerApplicationId, configured);
            ConfigReader.Set(Constants.Environment.FlyingClubTokenAuthorizedParty, existing);
            var result = await _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>().CreateAsync(new CreateLinkingProofRequest
            {
                FlyingClubAccountNumber = "1234567890", TransactionId = "transaction", TransactionBindingHash = "binding",
                AssertionReplayId = "assertion", Nonce = "nonce", ProofReplayId = "proof",
                AuthenticatedCallerApplicationId = caller, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1)
            }, TestContext.Current.CancellationToken);
            // An authorised request reaches the deliberately unavailable atomic backend.
            if (allowed) Assert.True(result.IsFailure);
            else Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.Forbidden, result.Value.Status);
        }
        finally
        {
            ConfigReader.Set(Constants.Environment.AccountLinkingTrustedCallerApplicationId, "graphql-app");
            ConfigReader.Set(Constants.Environment.FlyingClubTokenAuthorizedParty, prior!);
        }
    }

    [Fact]
    public async Task Create_RejectsExpiredAbsoluteTransactionDeadline()
    {
        var handler = _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>();
        var result = await handler.CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction",
            TransactionBindingHash = "binding",
            AssertionReplayId = "assertion-replay",
            Nonce = "nonce",
            ProofReplayId = "proof-replay",
            AuthenticatedCallerApplicationId = "graphql-app",
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1)
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
        Assert.Null(result.Value.Value);
    }

    [Theory]
    [InlineData(45, 45)]
    [InlineData(300, 120)]
    public async Task CreateAndRedeem_ConsumesProofExactlyOnceWithAtomicRedisRepository(int transactionLifetime, int maximumProofLifetime)
    {
        var redisConnection = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (redisConnection is null)
            Assert.Skip("Set ACCOUNT_LINKING_REDIS_TEST_CONNECTION to run the Redis handler contract test.");

        using var connection = await ConnectionMultiplexer.ConnectAsync(redisConnection);
        using var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDistributedCache, DistributedCache>()
            .AddSingleton<ISingleUseDistributedCacheService, DistributedCacheService>()
            .AddSingleton<IConnectionMultiplexer>(connection)
            .AddSingleton<IAccountLinkingProofRepository, RedisAccountLinkingProofRepository>()
            .AddSingleton<IAccountLinkingProofHandler, AccountLinkingProofHandler>()
            .AddSingleton(TimeProvider.System)
            .BuildServiceProvider();

        var transactionId = $"handler-{Guid.NewGuid():N}";
        var result = await services.GetRequiredService<IAccountLinkingProofHandler>().CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = transactionId,
            TransactionBindingHash = "binding",
            AssertionReplayId = "assertion-replay",
            Nonce = "nonce",
            ProofReplayId = "proof-replay",
            AuthenticatedCallerApplicationId = "graphql-app",
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(transactionLifetime)
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, result.Value.Status);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", result.Value.Value.Proof);
        Assert.InRange((result.Value.Value.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds, maximumProofLifetime - 5, maximumProofLifetime);

        var redeemRequest = new RedeemLinkingProofRequest
        {
            Proof = result.Value.Value.Proof,

            TransactionId = transactionId,
            TransactionBindingHash = "binding"
        };
        var handler = services.GetRequiredService<IAccountLinkingProofHandler>();
        var firstRedemption = await handler.RedeemAsync(redeemRequest, TestContext.Current.CancellationToken);
        var retry = await handler.RedeemAsync(redeemRequest, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, firstRedemption.Value.Status);
        Assert.Equal("1234567890", firstRedemption.Value.Value.FlyingClubAccountNumber);
        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, retry.Value.Status);
        Assert.Null(retry.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64url")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA!")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA+")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB")]
    public async Task Redeem_RejectsMalformedOrNonCanonicalProof(string proof)
    {
        var handler = _serviceProvider.GetRequiredService<IAccountLinkingProofHandler>();
        var result = await handler.RedeemAsync(new RedeemLinkingProofRequest
        {
            Proof = proof,

            TransactionId = "transaction",
            TransactionBindingHash = "binding"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.BadRequest, result.Value.Status);
    }

    public void Dispose() => _serviceProvider.Dispose();
}

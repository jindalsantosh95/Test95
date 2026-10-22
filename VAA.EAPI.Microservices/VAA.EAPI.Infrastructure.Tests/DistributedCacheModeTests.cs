using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Infrastructure.Tests;

public sealed class DistributedCacheModeTests
{
    [Fact]
    public async Task DefaultModePreservesRepeatedReadsCompressionAndRemoval()
    {
        using var services = CreateServices();
        var cache = services.GetRequiredService<ISingleUseDistributedCacheService>();
        var key = "DEFAULT-" + Guid.NewGuid().ToString("N");
        Assert.Null(await cache.SetAsync(key, "value", TimeSpan.FromMinutes(1), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("value", await cache.Get<string>(key.ToLowerInvariant()));
        Assert.Equal("value", await cache.Get<string>(key));
        await cache.Remove(key);
        Assert.Null(await cache.Get<string>(key));
    }

    [Fact]
    public async Task LegacyInterfaceStillSuppressesCorruptCacheRead()
    {
        using var services = CreateServices();
        var raw = services.GetRequiredService<IDistributedCache>();
        var cache = services.GetRequiredService<IDistributedCacheService>();
        var key = Guid.NewGuid().ToString("N");
        await raw.SetStringAsync(key, "not-compressed", TestContext.Current.CancellationToken);
        Assert.Null(await cache.Get<string>(key));
        await cache.Set(key, "normal", false, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        Assert.Equal("normal", await cache.Get<string>(key));
        await cache.Refresh(key);
        Assert.Equal("normal", await cache.Get<string>(key));
    }

    [Fact]
    public async Task SingleUseDoesNotFallBackToNonAtomicDistributedCache()
    {
        using var services = CreateServices();
        var cache = services.GetRequiredService<ISingleUseDistributedCacheService>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => cache.SetAsync("key", "value",
            TimeSpan.FromMinutes(1), CacheMode.SingleUse, Options("scope"), TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cache.TryConsumeAsync<string>("key", "scope", "binding", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RedisSingleUsePreservesBindingAndHasOneConcurrentWinner()
    {
        using var connection = await ConnectRedis();
        using var services = CreateServices(connection);
        var cache = services.GetRequiredService<ISingleUseDistributedCacheService>();
        var scope = Guid.NewGuid().ToString("N");
        Assert.NotNull(await cache.SetAsync("key", "value", TimeSpan.FromSeconds(30),
            CacheMode.SingleUse, Options(scope), TestContext.Current.CancellationToken));
        Assert.Null(await cache.Get<string>("key")); // Single-use namespace cannot be read through legacy Get.
        Assert.Null(await cache.TryConsumeAsync<string>("key", scope, "wrong", TestContext.Current.CancellationToken));
        var results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ =>
            cache.TryConsumeAsync<string>("key", scope, "binding", TestContext.Current.CancellationToken)));
        Assert.Equal("value", Assert.Single(results, result => result is not null));
        Assert.Null(await cache.TryConsumeAsync<string>("key", scope, "binding", TestContext.Current.CancellationToken));
        Assert.Null(await cache.SetAsync("key", "again", TimeSpan.FromSeconds(30),
            CacheMode.SingleUse, Options(scope), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RedisSingleUseExpiresWithoutRefreshingOnMismatch()
    {
        using var connection = await ConnectRedis();
        using var services = CreateServices(connection);
        var cache = services.GetRequiredService<ISingleUseDistributedCacheService>();
        var scope = Guid.NewGuid().ToString("N");
        Assert.NotNull(await cache.SetAsync("key", "value", TimeSpan.FromSeconds(1),
            CacheMode.SingleUse, Options(scope), TestContext.Current.CancellationToken));
        Assert.Null(await cache.TryConsumeAsync<string>("key", scope, "wrong", TestContext.Current.CancellationToken));
        await Task.Delay(1200, TestContext.Current.CancellationToken);
        Assert.Null(await cache.TryConsumeAsync<string>("key", scope, "binding", TestContext.Current.CancellationToken));
    }

    private static SingleUseCacheOptions Options(string scope) => new()
    {
        ScopeKey = scope,
        Binding = "binding",
        Owner = "owner",
        IssuanceId = "issuance",
        ScopeLifetime = TimeSpan.FromSeconds(30),
        MaximumReplacements = 0
    };

    private static async Task<ConnectionMultiplexer> ConnectRedis()
    {
        var setting = Environment.GetEnvironmentVariable("ACCOUNT_LINKING_REDIS_TEST_CONNECTION");
        if (setting.IsNullOrWhiteSpace()) Assert.Skip("A dedicated Redis test connection is required.");
        return await ConnectionMultiplexer.ConnectAsync(setting!);
    }

    private static ServiceProvider CreateServices(IConnectionMultiplexer? connection = null)
    {
        var services = new ServiceCollection().AddLogging()
            .AddSingleton<IDistributedCache, VAA.EAPI.Core.Xunit.Caching.DistributedCache>()
            .AddSingleton<DistributedCacheService>()
            .AddSingleton<ISingleUseDistributedCacheService>(provider => provider.GetRequiredService<DistributedCacheService>())
            .AddSingleton<IDistributedCacheService>(provider => provider.GetRequiredService<DistributedCacheService>());
        if (connection is not null) services.AddSingleton(connection);
        return services.BuildServiceProvider();
    }
}

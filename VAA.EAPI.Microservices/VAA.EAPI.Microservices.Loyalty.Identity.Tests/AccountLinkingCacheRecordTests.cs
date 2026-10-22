using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

public sealed class AccountLinkingCacheRecordTests
{
    [Fact]
    public void RedisCacheRegistrationResolvesSharedCacheAndProofRepository()
    {
        // Resolve the real Redis implementation without opening a connection or substituting the cache.
        using var services = new ServiceCollection()
            .AddLogging()
            .AddStackExchangeRedisCache(options => options.Configuration = "localhost:6379")
            .AddSingleton<VAA.EAPI.Infrastructure.Caching.ISingleUseDistributedCacheService,
                VAA.EAPI.Infrastructure.Caching.DistributedCacheService>()
            .AddSingleton<Infrastructure.Interfaces.IAccountLinkingProofRepository,
                Infrastructure.Repositories.RedisAccountLinkingProofRepository>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Assert.IsAssignableFrom<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache>(services.GetRequiredService<IDistributedCache>());
        Assert.IsType<VAA.EAPI.Infrastructure.Caching.DistributedCacheService>(
            services.GetRequiredService<VAA.EAPI.Infrastructure.Caching.ISingleUseDistributedCacheService>());
        Assert.IsType<Infrastructure.Repositories.RedisAccountLinkingProofRepository>(
            services.GetRequiredService<Infrastructure.Interfaces.IAccountLinkingProofRepository>());
    }

    [Theory]
    [InlineData("synthetic-consent")]
    [InlineData("synthetic-consent-\u00e9")]
    public async Task SharedCompressionRoundTripsAccountBindingWithoutEncryptionConfiguration(string consent)
    {
        using var services = new ServiceCollection()
            .AddSingleton<IDistributedCache, VAA.EAPI.Core.Xunit.Caching.DistributedCache>()
            .BuildServiceProvider();
        var cache = services.GetRequiredService<IDistributedCache>();
        var record = new AccountLinkingRecord { FlyingClubAccountNumber = "1234567890", ConsentReference = consent };
        var payload = Convert.ToBase64String(JsonConvert.SerializeObject(record).Compress());
        await cache.SetStringAsync("synthetic-proof", payload,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120) },
            TestContext.Current.CancellationToken);

        var stored = await cache.GetStringAsync("synthetic-proof", TestContext.Current.CancellationToken);
        Assert.NotNull(stored);
        var restored = stored.DecompressTo<AccountLinkingRecord>();
        Assert.Equal(record.FlyingClubAccountNumber, restored.FlyingClubAccountNumber);
        Assert.Equal(consent, restored.ConsentReference);
        Assert.DoesNotContain(record.FlyingClubAccountNumber, stored);
    }
}

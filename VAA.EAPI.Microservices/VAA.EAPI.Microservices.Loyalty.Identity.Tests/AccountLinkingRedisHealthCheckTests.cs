using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

public sealed class AccountLinkingRedisHealthCheckTests
{
    [Theory]
    [InlineData("ACCOUNT_LINKING_REDIS_TEST_CONNECTION", HealthStatus.Healthy)]
    [InlineData("ACCOUNT_LINKING_REDIS_NO_SCRIPT_TEST_CONNECTION", HealthStatus.Unhealthy)]
    public async Task ReadinessRequiresScripting(string connectionVariable, HealthStatus expected)
    {
        var configuration = Environment.GetEnvironmentVariable(connectionVariable);
        if (configuration.IsNullOrWhiteSpace()) Assert.Skip($"Set {connectionVariable} to run the Redis readiness contract test.");
        using var connection = await ConnectionMultiplexer.ConnectAsync(configuration!);
        // The restricted test identity must permit PING but deny scripting.
        await connection.GetDatabase().PingAsync();
        using var services = new ServiceCollection()
            .AddSingleton<IConnectionMultiplexer>(connection)
            .AddSingleton<AccountLinkingRedisHealthCheck>()
            .BuildServiceProvider();
        var result = await services.GetRequiredService<AccountLinkingRedisHealthCheck>()
            .CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);
        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void InjectedRedisConnectionRemainsOwnedByDependencyInjection()
    {
        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(AccountLinkingRedisHealthCheck)));
    }
}

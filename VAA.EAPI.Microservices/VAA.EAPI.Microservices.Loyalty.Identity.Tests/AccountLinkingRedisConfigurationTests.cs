using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class AccountLinkingRedisConfigurationTests : IDisposable
{
    private const string SharedKey = "RedisConnectionString";
    private readonly string? _shared = ConfigReader.Try(SharedKey);

    [Theory]
    [InlineData("shared.test:6380")]
    [InlineData("shared.test:6380,ssl=false,abortConnect=true")]
    public void ResolvesSharedOptionsWithRequiredTls(string shared)
    {
        ConfigReader.Set(SharedKey, shared);
        using var services = new ServiceCollection()
            .AddSingleton(_ => AccountLinkingRedisConfiguration.Read())
            .BuildServiceProvider();

        var options = services.GetRequiredService<ConfigurationOptions>();
        Assert.Equal("shared.test", Assert.IsType<DnsEndPoint>(Assert.Single(options.EndPoints)).Host);
        Assert.True(options.Ssl);
        Assert.False(options.AbortOnConnectFail);
    }

    [Fact]
    public void InvalidSharedConfigurationFailsClosed()
    {
        ConfigReader.Set(SharedKey, "shared.test:6380,ssl=invalid");
        Assert.Throws<ArgumentOutOfRangeException>(() => AccountLinkingRedisConfiguration.Read());
    }

    [Fact]
    public void MissingSharedConnectionFailsClosed()
    {
        ConfigReader.Set(SharedKey, "");
        Assert.ThrowsAny<ArgumentException>(() => AccountLinkingRedisConfiguration.Read());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("previous.test:6380")]
    public void DisposeRestoresMissingAndExistingConfiguration(string? previous)
    {
        ConfigReader.Set(SharedKey, previous!);
        using (var fixture = new AccountLinkingRedisConfigurationTests())
        {
            ConfigReader.Set(SharedKey, "temporary.test:6380");
        }

        Assert.Equal(previous, ConfigReader.Try(SharedKey));
        Assert.Equal(previous,
            Environment.GetEnvironmentVariable(SharedKey));
    }

    public void Dispose()
    {
        ConfigReader.Set(SharedKey, _shared!);
    }
}

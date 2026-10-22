using VAA.EAPI.GraphQL.Common.Diagnostics.Logging;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

public sealed class LoggingProviderResolverTests
{
    [Theory]
    [InlineData("AppInsights", LoggingProvider.AppInsights)]
    [InlineData("Datadog", LoggingProvider.Datadog)]
    [InlineData("Both", LoggingProvider.Both)]
    [InlineData("1", LoggingProvider.AppInsights)]
    [InlineData("unsupported", LoggingProvider.AppInsights)]
    public void ParseReturnsKnownProvidersAndFallsBackForInvalidConfiguration(string configuredValue, LoggingProvider expectedProvider)
    {
        Assert.Equal(expectedProvider, LoggingProviderResolver.Parse(configuredValue));
    }
}

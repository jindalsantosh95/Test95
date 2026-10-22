using VAA.EAPI.Automation.Book.Provisioning.Pnr;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.EndtoEnd;

public sealed class SeasonalRouteDateResolverTests
{
    [Fact]
    public void ConfiguredRoutesMustRetainFutureService()
    {
        IReadOnlyList<string> expiredRoutes =
            SeasonalRouteDateResolver.GetRoutesWithoutFutureService(DateTime.UtcNow.Date);

        Assert.True(
            expiredRoutes.Count == 0,
            $"Seasonal service periods require maintenance for: {string.Join(", ", expiredRoutes)}.");
    }
}

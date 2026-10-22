using System.Reflection;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class DiscountCodeScenarioContractTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(typeof(DiscountCodeValidationTests))]
    [InlineData(typeof(DiscountCodeLifecycleTests))]
    public void DiscountCodeScenario_OwnsOneIndependentReportedSession(Type scenarioType)
    {
        Assert.True(typeof(SearchBookTestBase).IsAssignableFrom(scenarioType));
        Assert.Null(scenarioType.GetCustomAttribute<CollectionAttribute>());
        MethodInfo method = Assert.Single(scenarioType.GetMethods(),
            candidate => candidate.GetCustomAttribute<FactAttribute>() is not null);
        Assert.Null(method.GetCustomAttribute<FactAttribute>()?.Skip);
    }
}

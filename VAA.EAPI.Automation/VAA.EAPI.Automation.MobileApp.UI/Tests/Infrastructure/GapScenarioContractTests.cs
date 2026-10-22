using System.Reflection;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class GapScenarioContractTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(typeof(FareDismissalTests))]
    [InlineData(typeof(SummaryChangeTests))]
    [InlineData(typeof(ReturnSummaryChangeTests))]
    [InlineData(typeof(SummaryUpgradeTests))]
    [InlineData(typeof(Tests.Account.MemberNotTravellingTests))]
    [InlineData(typeof(PassengerRequiredFieldTests))]
    [InlineData(typeof(PassengerTermsGateTests))]
    public void GapScenario_OwnsOneIndependentReportedSession(Type scenarioType)
    {
        Assert.True(typeof(SearchBookTestBase).IsAssignableFrom(scenarioType));
        Assert.Null(scenarioType.GetCustomAttribute<CollectionAttribute>());
        MethodInfo method = Assert.Single(scenarioType.GetMethods(),
            candidate => candidate.GetCustomAttribute<FactAttribute>() is not null);
        Assert.Null(method.GetCustomAttribute<FactAttribute>()?.Skip);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Selected", true)]
    [InlineData("Not selected", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TermsState_UsesExactObservedValue(string value, bool expected)
        => Assert.Equal(expected, NativeBookingPage.ReadBookingTermsState(value));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    public void TermsState_MissingOrUnknownEvidenceFails(string? value)
        => Assert.Throws<InvalidOperationException>(() => NativeBookingPage.ReadBookingTermsState(value));
}

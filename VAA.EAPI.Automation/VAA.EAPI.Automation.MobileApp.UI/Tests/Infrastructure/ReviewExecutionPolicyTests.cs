using System.Reflection;
using VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class ReviewExecutionPolicyTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(typeof(SearchAndBook.BackendBaggageTests))]
    [InlineData(typeof(PartnerBackendBaggageTests))]
    public void BackendComparisons_AreIncludedInRegressionAndRetainDiagnosticSubset(Type testClass)
    {
        MethodInfo scenario = Assert.Single(testClass.GetMethods(),
            method => method.GetCustomAttribute<FactAttribute>() != null);
        (string? Name, string? Value)[] traits = scenario.CustomAttributes
            .Where(attribute => attribute.AttributeType == typeof(TraitAttribute))
            .Select(attribute => (attribute.ConstructorArguments[0].Value as string,
                attribute.ConstructorArguments[1].Value as string)).ToArray();
        Assert.Contains(traits, trait => trait.Name == "Suite" && trait.Value == "Regression");
        Assert.DoesNotContain(traits, trait => trait.Name == "Suite" && trait.Value == "Sanity");
        Assert.Contains(traits, trait => trait.Name == "Suite" && trait.Value == "BackendRegression");
        Assert.Contains(traits, trait => trait.Name == "Category" &&
            trait.Value == "MobileApp-SearchBook-BackendValidation");
        Assert.Contains(traits, trait => trait.Name == "RegressionArea" && trait.Value == "SearchAndBook");
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void GuestSeatBooking_UsesSharedSeatEnabledLaunchProfile()
        => Assert.True(typeof(SeatScenarioTestBase).IsAssignableFrom(typeof(GuestSeatBookingTests)));
}

using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class PromoFareEvidenceTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void FareIndex_MatchesExactTitleAtDiscoveredIndex()
    {
        const string source = """
            <App>
              <Text name="NativeFlightFareSelectorFare0Title" label="Economy Light" />
              <Text name="NativeFlightFareSelectorFare7Title" label="Economy Classic" />
              <Text name="NativeFlightFareSelectorFare7Title" label="Economy Classic" />
              <Text name="NativeFlightFareSelectorFare8Title" label="Economy Classic Flex" />
            </App>
            """;
        Assert.Equal(7, NativeFlightSearchPage.FindFareIndex(source, "Economy Classic"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("<App />")]
    [InlineData("<App><Text name='NativeFlightFareSelectorFare0Title' label='Economy Classic'/><Text name='NativeFlightFareSelectorFare1Title' label='Economy Classic'/></App>")]
    public void FareIndex_RejectsMissingOrAmbiguousTitle(string source)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.FindFareIndex(source, "Economy Classic"));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(0, "Code OTHER applied")]
    [InlineData(1, "Code HEMAN applied")]
    [InlineData(2, "")]
    public void Promotion_UsesOnlyTheMatchedFareAndDeduplicatesInheritedIds(int index, string expected)
    {
        const string source = """
            <App>
              <Text name="NativeFlightFareSelectorAppliedPromotion" label="Obsolete global badge" />
              <Text name="NativeFlightFareSelectorFare0AppliedPromotion" label="Code OTHER applied" />
              <Text name="NativeFlightFareSelectorFare1AppliedPromotion" label="Code HEMAN applied" />
              <Text name="NativeFlightFareSelectorFare1AppliedPromotion" label="Code HEMAN applied" />
              <Text name="NativeFlightFareSelectorFare10AppliedPromotion" label="Wrong fare" />
            </App>
            """;
        Assert.Equal(expected, NativeFlightSearchPage.ReadFarePromotion(source, index));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Promotion_RejectsConflictingEvidence()
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ReadFarePromotion(
            "<App><Text name='NativeFlightFareSelectorFare1AppliedPromotion' label='HEMAN'/><Text name='NativeFlightFareSelectorFare1AppliedPromotion' label='OTHER'/></App>", 1));
}

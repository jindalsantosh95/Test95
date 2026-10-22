using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class FareBaggageAssertionsTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Coverage_DoesNotClaimDimensionsFromSummaryAlone()
    {
        string coverage = FareBaggageAssertions.AssertMatches(Create("Included, Hand baggage",
            "1 x 10kg Carry-on bag, Max dimensions: 56 x 36 x 23cm"));
        Assert.Contains("three-axis dimensions=0", coverage);
        Assert.Contains("rows displaying three-axis dimensions=1", coverage);
        Assert.Contains("NOT EXERCISED", coverage);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Coverage_CountsOnlyVerifiedAdvertisedValues()
    {
        string coverage = FareBaggageAssertions.AssertMatches(Create(
            "Included, 1 cabin bag 10kg 56 x 36 x 23cm",
            "1 x 10kg Carry-on bag, Max dimensions: 56 x 36 x 23cm"));
        Assert.Contains("inclusion=1, quantity=1, weight=1, three-axis dimensions=1", coverage);
        Assert.DoesNotContain("NOT EXERCISED", coverage);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ExcludedCheckedBaggage_MayBeAbsentWhenCarryOnIsReturned()
        => FareBaggageAssertions.AssertMatches(new FareBaggageObservation("Economy Light", "Economy Light",
            ["Included, Hand baggage", "Not included, Checked baggage not included"],
            ["1 x 10kg carry-on bag, Max dimensions: 56 x 36 x 23cm"]));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SummaryRows_ReadAllExactIndexedIdsAndDeduplicateInheritedEvidence()
    {
        string source = """
            <App>
              <Row name="NativeFlightSummaryBaggageAllowance0" label="1 x 10kg carry-on bag" value="1 x 10kg carry-on bag" />
              <Row name="NativeFlightSummaryBaggageAllowance0" label="1 x 10kg carry-on bag" />
              <Row name="NativeFlightSummaryBaggageAllowance3" label="1 x 23kg checked-in bag" visible="false" />
              <Row name="NativeFlightSummaryBaggageAllowance3Other" label="Ignore" />
            </App>
            """;
        Assert.Equal(["1 x 10kg carry-on bag", "1 x 23kg checked-in bag"],
            NativeFlightSearchPage.ReadSummaryBaggageAllowances(source));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("<App><Row name='NativeFlightSummaryBaggageAllowance0' /></App>")]
    [InlineData("<App><Row name='NativeFlightSummaryBaggageAllowance0' label='First' /><Row name='NativeFlightSummaryBaggageAllowance0' label='Conflicting' /></App>")]
    public void SummaryRows_MissingOrConflictingEvidenceFails(string source)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ReadSummaryBaggageAllowances(source));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Included, 1 checked bag up to 23kg", "1 x 23kg Checked-in bag")]
    [InlineData("Included, 2 x 32kg checked bags", "2 x 32kg Checked-in bag")]
    [InlineData("Not included, Checked baggage", "Not included Checked-in bag")]
    [InlineData("Included, Hand baggage", "1 x 10kg Carry-on bag")]
    [InlineData("Included, 1 cabin bag 10.0 kg", "1 x 10kg Carry-on bag")]
    [InlineData("Included, Hand baggage 56 x 36 x 23cm", "1 x 10kg Carry-on bag, Max dimensions: 56.0 × 36 × 23 cm")]
    public void MatchingBaggage_Passes(string benefit, string allowance)
        => FareBaggageAssertions.AssertMatches(Create(benefit, allowance));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Included, 1 checked bag 23kg", "1 x 10kg Checked-in bag")]
    [InlineData("Included, 2 checked bags 23kg", "1 x 23kg Checked-in bag")]
    [InlineData("Not included, Checked baggage", "1 x 23kg Checked-in bag")]
    [InlineData("Included, Checked baggage", "Not included Checked-in bag")]
    [InlineData("Included, Checked baggage", "1 x 10kg Carry-on bag")]
    [InlineData("Included, Checked baggage", "Checked-in bag")]
    [InlineData("Included, Checked baggage", "0 x 23kg Checked-in bag")]
    [InlineData("Included, Wi-Fi", "1 x 23kg Checked-in bag")]
    [InlineData("Checked baggage", "1 x 23kg Checked-in bag")]
    [InlineData("Included, Hand baggage 56 x 36 x 23cm", "1 x 10kg Carry-on bag, Max dimensions: 55 x 36 x 23cm")]
    [InlineData("Included, Hand baggage 56 x 36 x 23cm", "1 x 10kg Carry-on bag")]
    [InlineData("Included, Hand baggage 56 x 36 x 23cm", "1 x 10kg Carry-on bag, Max dimensions: 56 x 36 x 23in")]
    public void MissingOrMismatchedBaggage_Fails(string benefit, string allowance)
        => Assert.NotNull(Record.Exception(() => FareBaggageAssertions.AssertMatches(Create(benefit, allowance))));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ChangedFareOrMissingEvidence_Fails()
    {
        FareBaggageObservation matching = Create("Included, 1 checked bag", "1 x 23kg Checked-in bag");
        FareBaggageObservation[] invalid =
        [
            matching with { SummaryFareLabel = "Economy Light" },
            matching with { FareLabel = "" },
            matching with { BenefitLabels = [] },
            matching with { BaggageAllowances = [] },
            matching with { BaggageAllowances = ["1 x 23kg Checked-in bag", "1 x 23kg Checked-in bag"] },
        ];
        Assert.All(invalid, observation =>
            Assert.NotNull(Record.Exception(() => FareBaggageAssertions.AssertMatches(observation))));
    }

    private static FareBaggageObservation Create(string benefit, string allowance)
        => new("Economy Classic", "Economy Classic", [benefit], [allowance]);
}

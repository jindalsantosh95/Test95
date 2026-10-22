using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SeatPriceEvidenceTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("0 of 1 seats selected, + £0.00", 0)]
    [InlineData("1 of 1 seats selected, + £49.50", 49.50)]
    [InlineData("£1,234.56", 1234.56)]
    [InlineData("21A, Available, Window, £80.00", 80)]
    public void GbpPrice_ReadsOneObservedAmount(string label, decimal expected)
        => Assert.Equal(expected, SeatPriceEvidence.ReadGbp(label));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("Free")]
    [InlineData("$49.50")]
    [InlineData("€49.50")]
    [InlineData("£1,20")]
    [InlineData("£12.345")]
    [InlineData("-£49.50")]
    [InlineData("£10.00 £10.00")]
    [InlineData("£10.00 $20.00")]
    public void GbpPrice_RejectsMissingMalformedOrAmbiguousEvidence(string label)
        => Assert.Throws<InvalidOperationException>(() => SeatPriceEvidence.ReadGbp(label));
}

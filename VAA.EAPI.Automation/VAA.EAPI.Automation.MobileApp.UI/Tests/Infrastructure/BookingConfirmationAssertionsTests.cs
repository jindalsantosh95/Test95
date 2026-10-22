using VAA.EAPI.Automation.MobileApp.UI.Assertions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class BookingConfirmationAssertionsTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("(1) AdultOne Automation")]
    [InlineData(" (1)  AdultOne\nAutomation ")]
    [InlineData("(1) adultone automation")]
    public void Confirmation_AcceptsExactTravellerWithNativeWhitespace(string observed)
        => BookingConfirmationAssertions.AssertTravellers([observed, observed, null], ["AdultOne Automation"]);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Confirmation_MatchesEveryTravellerInSubmittedOrder()
        => BookingConfirmationAssertions.AssertTravellers(
            ["(3) AdultOne Automation, AdultTwo Automation, ChildOne Automation"],
            ["AdultOne Automation", "AdultTwo Automation", "ChildOne Automation"]);

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("1 Adult")]
    [InlineData("(2) AdultOne Automation")]
    [InlineData("(1) AdultOne")]
    [InlineData("(1) AdultOne AutomationExtra")]
    [InlineData("(1) Other Passenger")]
    [InlineData("(1) AdultOne Automation, Other Passenger")]
    public void Confirmation_RejectsMissingLegacyOrIncorrectIdentities(string observed)
    {
        Exception? failure = Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(
            [observed], ["AdultOne Automation"]));
        Assert.NotNull(failure);
        Assert.DoesNotContain("AdultOne", failure.Message);
        Assert.DoesNotContain("Other Passenger", failure.Message);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Confirmation_RejectsConflictingNativeValues()
        => Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(
            ["(1) AdultOne Automation", "(1) Other Passenger"], ["AdultOne Automation"])));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Confirmation_RejectsReorderedTravellers()
        => Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(
            ["(2) ChildOne Automation, AdultOne Automation"], ["AdultOne Automation", "ChildOne Automation"])));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Confirmation_UsesMemberIdentityRatherThanGeneratedGuest()
    {
        BookingConfirmationAssertions.AssertTravellers(["(1) Synthetic Member"], ["Synthetic Member"]);
        Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(
            ["(1) AdultOne Automation"], ["Synthetic Member"])));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Confirmation_RejectsEmptyExpectedOrObservedEvidence()
    {
        Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers([], ["Synthetic Member"])));
        Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(["(0)"], [])));
        Assert.NotNull(Record.Exception(() => BookingConfirmationAssertions.AssertTravellers(["(1)"], [" "])));
    }
}

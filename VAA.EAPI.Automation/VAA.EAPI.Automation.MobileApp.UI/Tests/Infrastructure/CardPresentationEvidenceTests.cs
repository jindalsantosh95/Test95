using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class CardPresentationEvidenceTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void CardPresentationInstructions_ReadsAllParagraphs()
        => NativeBookingPage.VerifyCardPresentationInstructions([
            "The credit card holder must bring photo identification.",
            "You won’t be able to travel otherwise.",
            "You won’t be able to check in online",
            "Your flight(s) are not being held"]);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void CardPresentationInstructions_RejectsFirstParagraphOnly()
        => Assert.Throws<InvalidOperationException>(() => NativeBookingPage.VerifyCardPresentationInstructions([
            "The credit card holder must bring photo identification."]));
}

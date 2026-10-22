using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class PassengerFlowCompatibilityTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PaymentEntryIds_MatchCurrentNativePaymentContract()
    {
        Assert.Equal("NativeFlightPaymentHeading", NativeBookingAccessibilityIds.PaymentHandoffTitle);
        Assert.Equal("NativeFlightContinueToPaymentButton", NativeBookingAccessibilityIds.PaymentContinueButton);
        Assert.Equal("NativeFlightPaymentRetryButton", NativeBookingAccessibilityIds.PaymentRetryButton);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Male", "Male", true)]
    [InlineData("Female", "Male", false)]
    [InlineData("Female", "Female", true)]
    [InlineData("Undisclosed", "Undisclosed", true)]
    [InlineData("Unspecified", "Unspecified", true)]
    [InlineData("Gender, Male", "Male", false)]
    [InlineData(null, "Male", false)]
    [InlineData("", "", false)]
    public void GenderMenu_RequiresExactNonEmptyOption(string? label, string gender, bool expected)
        => Assert.Equal(expected, NativeBookingPage.IsGenderMenuOption(label, gender));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(true, "up", false)]
    [InlineData(false, "down", true)]
    public void ContainerNavigation_SwipeIsScopedAndDoesNotSendScrollDistance(
        bool swipe, string direction, bool hasDistance)
    {
        Dictionary<string, object> arguments = NativeAppPage.ContainerNavigationArguments("editor-reference", 0.4, swipe);
        Assert.Equal("editor-reference", arguments["elementId"]);
        Assert.Equal(direction, arguments["direction"]);
        Assert.Equal(hasDistance, arguments.ContainsKey("distance"));
        if (hasDistance)
            Assert.Equal(0.4, arguments["distance"]);
        Assert.DoesNotContain("x", arguments.Keys);
        Assert.DoesNotContain("y", arguments.Keys);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(true, "PassengerDetailsHub")]
    [InlineData(false, "NativeFlightPassengerDetailsScrollView")]
    public void HubActions_UseThePageOwnedScrollContainer(bool redesigned, string expected)
        => Assert.Equal(expected, NativeBookingPage.PassengerHubScrollContainer(redesigned));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(287, true)]
    [InlineData(70, false)]
    public void SelectionTap_RejectsExpandedGroupButAcceptsHeaderOnlyBounds(int buttonHeight, bool expected)
        => Assert.Equal(expected, NativeBookingPage.SelectionTapOverlapsPicker(
            new System.Drawing.Rectangle(16, 401, 370, buttonHeight),
            new System.Drawing.Rectangle(16, 471, 370, 217)));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SelectionEvidence_ReadsScopedSwiftUiLabelsWithoutBorrowingAnotherPassenger()
    {
        const string source = """
            <App>
              <Text type="XCUIElementTypeStaticText" name="PassengerDetailsPassenger1Gender" label="Gender" visible="false" />
              <Text type="XCUIElementTypeStaticText" name="PassengerDetailsPassenger1Gender" label="Female" visible="false" />
              <Group name="PassengerDetailsPassenger1Gender">
                <Wheel type="XCUIElementTypePickerWheel">
                  <Text type="XCUIElementTypeStaticText" label="Male" />
                </Wheel>
              </Group>
              <Button type="XCUIElementTypeButton" name="PassengerDetailsPassenger2Gender" label="Gender, Male" />
              <Button type="XCUIElementTypeButton" name="PassengerDetailsContactCountryCode-PassengerDetailsContactCountryCode" label="Country code, +44" />
            </App>
            """;
        string gender = NativeBookingPage.ReadSelectionRowText(source, "PassengerDetailsPassenger1Gender");
        Assert.True(NativeBookingPage.AccessibleTextContainsOption(gender, "Female"));
        Assert.False(NativeBookingPage.AccessibleTextContainsOption(gender, "Male"));
        Assert.True(NativeBookingPage.AccessibleTextContainsOption(
            NativeBookingPage.ReadSelectionRowText(source, "PassengerDetailsContactCountryCode"), "+44"));
        Assert.Empty(NativeBookingPage.ReadSelectionRowText(source, "Missing"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Gender, Female", "Male", false)]
    [InlineData("Gender, Male", "Male", true)]
    [InlineData("Country code +44", "+44", true)]
    [InlineData("Country code +440", "+44", false)]
    public void SelectionEvidence_RequiresCompleteOption(string text, string option, bool expected)
        => Assert.Equal(expected, NativeBookingPage.AccessibleTextContainsOption(text, option));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(0, "PassengerDetailsPassengerCard1", "PassengerDetailsPassenger1FirstNameText")]
    [InlineData(2, "PassengerDetailsPassengerCard3", "PassengerDetailsPassenger3FirstNameText")]
    public void RedesignedPassengerIds_UseCurrentAppContract(int index, string card, string firstName)
    {
        Assert.Equal("PassengerDetailsHub", NativeBookingAccessibilityIds.PassengerDetailsHub);
        Assert.Equal("PassengerDetailsModalHeader", NativeBookingAccessibilityIds.RedesignedPassengerEditorHeader);
        Assert.Equal(card, NativeBookingAccessibilityIds.RedesignedTravellerCard(index));
        Assert.Equal(firstName, NativeBookingAccessibilityIds.RedesignedTravellerFirstNameText(index));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(false, 1, "traveller0,saveFinal,contact")]
    [InlineData(false, 3, "traveller0,save,traveller1,save,traveller2,saveFinal,contact")]
    [InlineData(true, 1, "traveller0,contact,saveFinal")]
    [InlineData(true, 3, "traveller0,contact,save,traveller1,save,traveller2,saveFinal")]
    [InlineData(true, 5, "traveller0,contact,save,traveller1,save,traveller2,save,traveller3,save,traveller4,saveFinal")]
    [InlineData(false, 5, "traveller0,save,traveller1,save,traveller2,save,traveller3,save,traveller4,saveFinal,contact")]
    public void GuestCheckout_EntersContactAtTheLayoutOwnedStage(bool redesigned, int count, string expected)
    {
        List<string> events = [];
        NativeBookingCheckoutFlow.CompleteGuestTravellers(count,
            index => events.Add($"traveller{index}"),
            () => redesigned,
            () => events.Add("contact"),
            final => events.Add(final ? "saveFinal" : "save"));
        Assert.Equal(expected, string.Join(",", events));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(0, true)]
    [InlineData(2, true)]
    public void TravellerCard_UsesPageContractNotCurrentCardVisibility(int index, bool redesigned)
        => Assert.Equal(redesigned
                ? NativeBookingAccessibilityIds.RedesignedTravellerCard(index)
                : NativeBookingAccessibilityIds.TravellerCard(index),
            NativeBookingPage.TravellerCardForPage(index, redesigned));
}

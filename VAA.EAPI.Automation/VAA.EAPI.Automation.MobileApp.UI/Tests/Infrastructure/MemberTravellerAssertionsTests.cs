using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class MemberTravellerAssertionsTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("true", true, true)]
    [InlineData("false", true, false)]
    [InlineData("true", false, true)]
    public void EditorSnapshot_ReadsOffscreenAndEmptyControlsButRejectsHiddenEditor(
        string editorVisible, bool includeValue, bool valid)
    {
        string value = includeValue ? "value='synthetic'" : "";
        string source = $"<App><Scroll name='PassengerDetailsEditorScrollView' visible='{editorVisible}'>" +
            $"<Field name='field' type='XCUIElementTypeTextField' visible='false' {value}/></Scroll></App>";
        Dictionary<string, string> keys = new() { ["telephone"] = "field" };
        if (valid)
        {
            Assert.Equal(includeValue ? "synthetic" : "", PageObjects.NativeBookingPage.ReadEditorFieldValues(source, keys)["telephone"]);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => PageObjects.NativeBookingPage.ReadEditorFieldValues(source, keys));
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("<Other name='field' type='XCUIElementTypeOther'/>")]
    public void EditorSnapshot_RejectsMissingControlOrUnknownEmptyNode(string child)
    {
        string source = $"<App><Scroll name='PassengerDetailsEditorScrollView' visible='true'>{child}</Scroll></App>";
        Assert.Throws<InvalidOperationException>(() => PageObjects.NativeBookingPage.ReadEditorFieldValues(
            source, new Dictionary<string, string> { ["telephone"] = "field" }));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void EditorSnapshot_EmptySelectionButtonDoesNotReadItsChildLabelAsAValue()
    {
        const string source = "<App><Scroll name='PassengerDetailsEditorScrollView' visible='true'>" +
            "<Button name='dob-dob' type='XCUIElementTypeButton'>" +
            "<Text name='dob' type='XCUIElementTypeStaticText' value='Date of birth'/></Button></Scroll></App>";
        Assert.Equal("", PageObjects.NativeBookingPage.ReadEditorFieldValues(
            source, new Dictionary<string, string> { ["dateOfBirth"] = "dob" })["dateOfBirth"]);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void EditorSnapshot_RejectsConflictingValues()
    {
        const string source = "<App><Scroll name='PassengerDetailsEditorScrollView' visible='true'>" +
            "<Field name='field' value='one'/><Field name='field' value='two'/></Scroll></App>";
        Assert.Throws<InvalidOperationException>(() => PageObjects.NativeBookingPage.ReadEditorFieldValues(
            source, new Dictionary<string, string> { ["telephone"] = "field" }));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void EditorSnapshot_ComposedSelectionButtonOwnsValueNotItsChildLabel()
    {
        const string source = "<App><Scroll name='PassengerDetailsEditorScrollView' visible='true'>" +
            "<Button name='dob-dob' type='XCUIElementTypeButton' value='synthetic-date'>" +
            "<Text name='dob' type='XCUIElementTypeStaticText' value='Date of birth'/></Button></Scroll></App>";
        Assert.Equal("synthetic-date", PageObjects.NativeBookingPage.ReadEditorFieldValues(
            source, new Dictionary<string, string> { ["dateOfBirth"] = "dob" })["dateOfBirth"]);
    }

    private static Dictionary<string, string> EmptyFields() => new()
    {
        ["firstName"] = "First name",
        ["lastName"] = "Last name",
        ["dateOfBirth"] = "Date of birth",
        ["gender"] = "Select gender",
        ["membership"] = "Membership number",
        ["email"] = "Email address",
        ["telephone"] = "Telephone number",
    };

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Clear_AllowsOnlyEmptyValuesOrKnownPlaceholders()
        => MemberTravellerAssertions.Cleared(EmptyFields());

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("firstName")]
    [InlineData("lastName")]
    [InlineData("dateOfBirth")]
    [InlineData("gender")]
    [InlineData("membership")]
    [InlineData("email")]
    [InlineData("telephone")]
    public void Clear_RejectsRetainedProfileDataWithoutLeakingIt(string key)
    {
        Dictionary<string, string> fields = EmptyFields();
        fields[key] = "sensitive-synthetic-value";
        Exception? error = Record.Exception(() => MemberTravellerAssertions.Cleared(fields));
        Assert.NotNull(error);
        Assert.DoesNotContain("sensitive-synthetic-value", error.Message);
        fields.Remove(key);
        Assert.Throws<InvalidOperationException>(() => MemberTravellerAssertions.Cleared(fields));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Retention_RejectsLostDetailsOrRestoredMemberNumber()
    {
        Dictionary<string, string> expected = new()
        {
            ["firstName"] = "Replacement",
            ["lastName"] = "Automation",
            ["dateOfBirth"] = "1 Jan 1990",
            ["gender"] = "Male",
            ["membership"] = "",
            ["email"] = "qa@example.com",
            ["telephone"] = "7700900123",
        };
        MemberTravellerAssertions.Retained(expected, expected);
        foreach (string key in expected.Keys)
        {
            Dictionary<string, string> changed = new(expected) { [key] = "changed" };
            Assert.NotNull(Record.Exception(() => MemberTravellerAssertions.Retained(expected, changed)));
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Member_RejectsDifferentAccountWithoutPrintingIdentifiers()
    {
        LoyaltyAccount member = new("login", "synthetic-number", "Stored", "Member", "Red", 0);
        Exception? error = Record.Exception(() => MemberTravellerAssertions.MatchesMember(EmptyFields(), member));
        Assert.NotNull(error);
        Assert.DoesNotContain(member.FlyingClubNumber, error.Message);
    }
}

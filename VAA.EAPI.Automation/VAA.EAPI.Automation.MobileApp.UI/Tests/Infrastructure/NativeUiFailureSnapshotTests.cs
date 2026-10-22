using System.Xml.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class NativeUiFailureSnapshotTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Snapshot_PreservesSeatSheetIdCollisionsWithoutLabelsOrValues()
    {
        XElement snapshot = NativeUiFailureSnapshot.Sanitise("""
            <App>
              <Button type="XCUIElementTypeButton" name="seatSelection.detailSheet" label="Private" visible="true" />
              <Button type="XCUIElementTypeButton" name="seatSelection.detailSheet" value="Secret" visible="true" />
              <Button name="seatSelection.detail.close" />
              <Button name="seatSelection.selectSeat" />
            </App>
            """);
        Assert.Equal(2, snapshot.Elements().Count(element =>
            (string?)element.Attribute("id") == "seatSelection.detailSheet"));
        Assert.Contains("seatSelection.detail.close", snapshot.ToString());
        Assert.Contains("seatSelection.selectSeat", snapshot.ToString());
        Assert.DoesNotContain("Private", snapshot.ToString());
        Assert.DoesNotContain("Secret", snapshot.ToString());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Snapshot_PreservesRepeatedCarrierIdsButOmitsPersonalData()
    {
        XElement result = NativeUiFailureSnapshot.Sanitise("""
            <App label="Private account">
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0CarrierVS" label="Virgin Atlantic" visible="true" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0CarrierVS" value="09:00" />
              <Input type="XCUIElementTypeTextField" name="private@example.com" label="Private name" value="secret" />
              <View type="XCUIElementTypeScrollView" name="PassengerDetailsEditorScrollView" label="Private name" />
            </App>
            """);
        Assert.Equal(2, result.Elements().Count(element => (string?)element.Attribute("id") == "NativeFlightResult0CarrierVS"));
        string text = result.ToString();
        Assert.DoesNotContain("Private", text);
        Assert.DoesNotContain("private@example.com", text);
        Assert.DoesNotContain("secret", text);
        Assert.DoesNotContain("Virgin Atlantic", text);
        Assert.DoesNotContain("09:00", text);
        Assert.Contains("visible=\"true\"", text);
        Assert.Contains("PassengerDetailsEditorScrollView", text);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Snapshot_BoundsOutputAndRejectsUnknownIdentifierSuffix()
    {
        string source = "<App>" + string.Concat(Enumerable.Repeat(
            "<Text name='NativeFlightResult0CarrierVSPrivate' value='secret' />", 2100)) + "</App>";
        XElement result = NativeUiFailureSnapshot.Sanitise(source);
        Assert.Equal(2000, result.Elements().Count());
        Assert.Equal("true", (string?)result.Attribute("truncated"));
        Assert.DoesNotContain("Private", result.ToString());
    }
}

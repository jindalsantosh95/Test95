using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace VAA.EAPI.Automation.Trips.UI.Pages;

public sealed class TripDetailsPage : BasePage
{
    private static readonly Regex TripDetailsUrl = new(
        "/my-trips-dev/trips/[^/]+/?$",
        RegexOptions.Compiled);

    public TripDetailsPage(IPage page)
        : base(page)
    {
    }

    public async Task WaitForTripAsync(string bookingReference, int timeoutMs)
    {
        await Page.WaitForURLAsync(
            TripDetailsUrl,
            new PageWaitForURLOptions { Timeout = timeoutMs });
        await Assertions.Expect(Page).ToHaveURLAsync(TripDetailsUrl);
        await Assertions.Expect(Page.GetByText(bookingReference, new() { Exact = true }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = timeoutMs });
    }

    public async Task AssertReturnJourneyAsync(int timeoutMs)
    {
        await Assertions.Expect(Page.GetByRole(AriaRole.Tablist, new() { Name = "Choose a journey" }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = timeoutMs });
        ILocator journeyTabs = Page.GetByRole(AriaRole.Tablist, new() { Name = "Choose a journey" })
            .GetByRole(AriaRole.Tab);
        await Assertions.Expect(journeyTabs)
            .ToHaveCountAsync(2, new LocatorAssertionsToHaveCountOptions { Timeout = timeoutMs });
    }

    public async Task VerifyExistingFlyingClubDetailsAsync(string passengerName)
    {
        ILocator passenger = Page.GetByRole(AriaRole.Article, new() { Name = $"{passengerName} passenger details" });
        await Assertions.Expect(passenger.GetByText("Gold member", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(passenger.GetByRole(AriaRole.Button, new() { Name = "Change Flying Club number", Exact = true })).ToBeVisibleAsync();
    }

    public async Task RequestAccessibleTravelAsync(string passengerName)
    {
        ILocator manageTrip = Page.GetByRole(AriaRole.Region, new() { Name = "Manage your trip" });
        await manageTrip.GetByRole(AriaRole.Button, new() { Name = "Accessible travel services", Exact = true }).ClickAsync();
        ILocator dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Accessible travel request" });
        await dialog.GetByRole(AriaRole.Button, new() { Name = passengerName }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Wheelchair assistance" }).ClickAsync();
        await dialog.GetByRole(AriaRole.Radio, new() { Name = "Wheelchair assistance to the aircraft", Exact = false }).CheckAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Review request" }).ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Submit request" }).ClickAsync();
        await Assertions.Expect(Page.GetByTestId("toast-component")).ToContainTextAsync("Your accessible travel request has been saved.");
    }
}

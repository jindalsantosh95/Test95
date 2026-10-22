using Microsoft.Playwright;

namespace VAA.EAPI.Automation.Trips.UI.Pages;

public sealed class AddTripPage : BasePage
{
    public AddTripPage(IPage page)
        : base(page)
    {
    }

    public async Task OpenAsync(string baseUrl)
    {
        await Page.GotoAsync(
            $"{baseUrl.TrimEnd('/')}/",
            new PageGotoOptions { WaitUntil = WaitUntilState.Commit });
        await WaitUntilLoadedAsync();
    }

    public Task WaitUntilLoadedAsync() =>
        Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Level = 1, Name = "Find your trip", Exact = true }))
            .ToBeVisibleAsync();

    public async Task EnterFlightDetailsAsync(string pnr, string firstName, string surname)
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Booking reference or ticket number" }).FillAsync(pnr);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true }).ClickAsync();
        await Assertions.Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1, Name = "Enter your flight details", Exact = true })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "First name" }).FillAsync(firstName);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Surname" }).FillAsync(surname);
    }

    public Task SubmitAsync() => Page.GetByRole(AriaRole.Button, new() { Name = "Find my trip", Exact = true }).ClickAsync();
}

using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Pages;
using VAA.EAPI.Automation.Trips.UI.Support;

namespace VAA.EAPI.Automation.Trips.UI.Flows;

public sealed class RequestAccessibleTravelFlow(PlaywrightBrowserOptions options)
{
    public async Task RunAsync(TripsUiTestData data, string passengerName)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        try
        {
            session.SetCurrentAction("starting accessible travel journey");
            AddTripPage add = new(session.Page);
            session.SetCurrentAction("opening the My Trips retrieval form");
            await add.OpenAsync(options.BaseUrl.ToString());
            session.SetCurrentAction("submitting the stored PNR and passenger details");
            await add.EnterFlightDetailsAsync(data.Pnr, data.FirstName, data.LastName);
            await add.SubmitAsync();
            TripDetailsPage details = new(session.Page);
            session.SetCurrentAction("waiting for the matching retrieved trip");
            await details.WaitForTripAsync(data.Pnr, 30_000);
            session.SetCurrentAction("submitting the accessible travel request");
            await details.RequestAccessibleTravelAsync(passengerName);
            await session.MarkPassedAsync("Submitted accessible travel request.");
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(await session.MarkFailedAsync(exception), exception);
        }
    }
}

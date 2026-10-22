using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Pages;
using VAA.EAPI.Automation.Trips.UI.Support;

namespace VAA.EAPI.Automation.Trips.UI.Flows;

public sealed class RetrieveTripFlow
{
    private readonly PlaywrightBrowserOptions _options;

    public RetrieveTripFlow(PlaywrightBrowserOptions options)
    {
        _options = options;
    }

    public async Task<RetrievedTrip> RunAsync(TripsUiTestData data)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        session.SetCurrentAction("opening the My Trips retrieval form", data.Pnr);

        try
        {
            AddTripPage addTripPage = new(session.Page);
            await addTripPage.OpenAsync(_options.BaseUrl.ToString());

            session.SetCurrentAction("submitting the stored PNR and passenger details");
            await addTripPage.EnterFlightDetailsAsync(data.Pnr, data.FirstName, data.LastName);
            await addTripPage.SubmitAsync();

            session.SetCurrentAction("waiting for the matching retrieved trip");
            await new TripDetailsPage(session.Page).WaitForTripAsync(data.Pnr, 30_000);

            await session.MarkPassedAsync($"Displayed storage PNR {data.Pnr}.");
            return new RetrievedTrip(data.Pnr);
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }

    public async Task<RetrievedTrip> RunReturnAsync(TripsUiTestData data)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        session.SetCurrentAction("starting return-trip retrieval journey");

        try
        {
            AddTripPage addTripPage = new(session.Page);
            session.SetCurrentAction("opening the My Trips retrieval form");
            await addTripPage.OpenAsync(_options.BaseUrl.ToString());
            session.SetCurrentAction("submitting the stored return-trip PNR and passenger details");
            await addTripPage.EnterFlightDetailsAsync(data.Pnr, data.FirstName, data.LastName);
            await addTripPage.SubmitAsync();

            TripDetailsPage tripDetailsPage = new(session.Page);
            session.SetCurrentAction("waiting for the matching retrieved return trip");
            await tripDetailsPage.WaitForTripAsync(data.Pnr, 30_000);
            session.SetCurrentAction("verifying both return-trip journeys are displayed");
            await tripDetailsPage.AssertReturnJourneyAsync(30_000);

            await session.MarkPassedAsync($"Displayed return-trip storage PNR {data.Pnr} with both journeys.");
            return new RetrievedTrip(data.Pnr);
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }
}

public sealed record RetrievedTrip(string BookingReference);

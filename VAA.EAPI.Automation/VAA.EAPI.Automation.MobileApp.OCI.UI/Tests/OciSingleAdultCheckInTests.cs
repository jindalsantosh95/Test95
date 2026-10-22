using VAA.EAPI.Automation.MobileApp.OCI.UI.Data;
using VAA.EAPI.Automation.MobileApp.OCI.UI.Models;
using VAA.EAPI.Automation.MobileApp.OCI.UI.PageObjects;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.OCI.UI.Tests;

public sealed class OciSingleAdultCheckInTests : SearchBookTestBase
{
    public OciSingleAdultCheckInTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact(Skip = "Not ready: Trips/OCI needs stable scoped Check In and Continue to Check In accessibility contracts.")]
    [Trait("Category", "MobileApp-OCI-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "OnlineCheckIn")]
    [Trait("Feature", "OnlineCheckIn")]
    [Trait("Journey", "SingleAdultPriorityRoute")]
    [Trait("Readiness", "NotReady")]
    [Trait("Platform", "iOS")]
    public void OnlineCheckIn_TC01_SingleAdultPriorityRoute_GeneratesBoardingPass()
    {
        TrySetBrowserStackSessionName();
        OciCandidate? candidate = null;

        try
        {
            candidate = new OciCandidateProvider(Fixture.Config).FindPreparedSingleAdult();
            Output.WriteLine(
                $"Prepared OCI candidate from {candidate.SourceTable}: " +
                $"{candidate.Origin}-{candidate.Destination}, departing {candidate.DepartureUtc:O}.");

            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            new TripsPage(Fixture.Driver).Open();

            OciCheckInPage checkIn = new(Fixture.Driver);
            checkIn.AddPreparedTrip(candidate);
            checkIn.CompleteCheckIn();
            checkIn.AssertBoardingPassGenerated();

            TrySetBrowserStackStatus("passed", "Single-adult OCI journey generated a boarding pass.");
        }
        catch (Exception exception)
        {
            string candidateContext = candidate is null
                ? "No candidate passed preparation."
                : $"Candidate route {candidate.Origin}-{candidate.Destination}.";
            TrySetBrowserStackStatus(
                "failed",
                $"{candidateContext} {DescribeBrowserStackFailure(exception)}");
            throw;
        }
    }
}

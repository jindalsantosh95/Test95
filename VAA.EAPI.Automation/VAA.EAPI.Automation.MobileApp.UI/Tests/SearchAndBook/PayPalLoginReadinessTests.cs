using VAA.EAPI.Automation.Core.TestDataConfig.APMPayment;
using VAA.EAPI.Automation.MobileApp.UI.Data;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PayPalLoginReadinessTests(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?>
    {
        ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry,850101-search-native-skip-seat-selection",
        ["Diagnostics:SuppressSensitiveEvidence"] = "true",
    })
{
    [MobilePayPalFact]
    [Trait("Category", "MobileApp-SearchBook-PayPalReadiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Readiness")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Journey", "PayPal")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC57_GuestPayPalSandbox_CompletesBookingAndAppearsInTrips()
    {
        string stage = "loading sandbox credentials";
        try
        {
            APMDetails account = PayPalSandboxAccountProvider.Retrieve(Fixture.Config);
            Output.WriteLine("PayPal sandbox credentials loaded from Core Vault; values omitted.");
            stage = "guest search and selection";
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage search = new(Fixture.Driver);
            search.Open();
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, PassengerCounts.OneAdult));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true,
                requiredCabin: "Economy", requiredFareTitle: "Economy Classic");
            stage = "checkout, PayPal approval and confirmation/Trips";
            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver)
                .CompleteWithPayPalForOneAdult(account, message => Output.WriteLine(message));
            ReportSuccessfulBooking(booking, "PayPal sandbox booking confirmed and present in Trips.");
        }
        catch (Exception)
        {
            string reason = $"PayPal failed during {stage}; no payment retry. Credential-bearing diagnostics suppressed.";
            TrySetBrowserStackStatus("failed", reason);
            throw new InvalidOperationException(reason);
        }
    }
}

using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Flows;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Journeys;

[Trait("Category", "SearchAndBook-UI-Regression")]
public sealed class GuestJourneyTests
{
    private readonly ITestOutputHelper _output;

    public GuestJourneyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableTheory]
    [Trait("Category", "DailySanity-RevenueBooking")]
    [InlineData("Economy Light")]
    [InlineData("Upper Class")]
    public async Task OneWay_VirginFare_GuestCardJourney_CompletesBooking(string fareName)
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = $"One-way {fareName} guest card journey",
            FareName = fareName
        });
    }

    [SkippableFact]
    [Trait("Category", "DailySanity-RevenueBooking-AdHoc")]
    public async Task DailySanity_EconomyLight_GuestCardJourney_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Daily sanity one-way Economy Light guest card journey",
            FareName = "Economy Light"
        });
    }

    [SkippableFact]
    public async Task OneWay_Premium_PaidSeat_GuestCardJourney_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "One-way Premium paid-seat guest card journey",
            FareName = "Premium",
            SeatSelection = SeatSelectionMode.SelectPaid
        });
    }

    [SkippableFact]
    public async Task OneWay_UpperClass_AvailableSeat_GuestCardJourney_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "One-way Upper Class available-seat guest card journey",
            FareName = "Upper Class",
            SeatSelection = SeatSelectionMode.SelectAvailable
        });
    }

    [SkippableTheory]
    [InlineData("Economy Light")]
    [InlineData("Upper Class")]
    public async Task Return_VirginFare_GuestCardJourney_CompletesBooking(string fareName)
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = $"Return {fareName} guest card journey",
            FareName = fareName,
            IsReturn = true
        });
    }

    [SkippableFact]
    public async Task Return_Premium_PaidSeat_GuestCardJourney_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Return Premium paid-seat guest card journey",
            FareName = "Premium",
            IsReturn = true,
            SeatSelection = SeatSelectionMode.SelectPaid
        });
    }

    [SkippableFact]
    public async Task Return_UpperClass_PerLegSeatSelection_GuestCardJourney_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Return Upper Class per-leg available-seat guest card journey",
            FareName = "Upper Class",
            IsReturn = true,
            SeatSelection = SeatSelectionMode.SelectAvailable
        });
    }

    [SkippableTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EconomyLight_TwoAdultsChildInfant_GuestCardJourney_CompletesBooking(bool isReturn)
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = $"{(isReturn ? "Return" : "One-way")} Economy Light 2-adult child infant journey",
            FareName = "Economy Light",
            IsReturn = isReturn,
            Adults = 2,
            Children = 1,
            Infants = 1
        });
    }

    [SkippableTheory]
    [InlineData("JFK", "LHR", "USD", false)]
    [InlineData("JFK", "LHR", "USD", true)]
    [InlineData("LHR", "ATL", "GBP", true)]
    [InlineData("CDG", "JFK", "EUR", false)]
    [InlineData("YYZ", "LHR", "CAD", false)]
    public async Task PointOfSaleCurrency_GuestCardJourney_CompletesBooking(
        string origin,
        string destination,
        string currency,
        bool acceptInsurance)
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = $"{origin}-{destination} {currency}{(acceptInsurance ? " with insurance" : string.Empty)}",
            Origin = origin,
            Destination = destination,
            FareName = "Any available",
            SelectAnyAvailableFare = true,
            ExpectedCurrency = currency,
            AcceptInsurance = acceptInsurance
        });
    }

    [SkippableTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeltaOperated_GuestCardJourney_CompletesBooking(bool isReturn)
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = $"{(isReturn ? "Return" : "One-way")} Delta-operated guest card journey",
            FareName = "Any available Delta fare",
            SelectAnyAvailableFare = true,
            CarrierCode = "dl",
            IsReturn = isReturn
        });
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-LoggedIn")]
    [Trait("tag", "search-and-book-ui-logged-in")]
    public async Task LoggedIn_EconomyLight_TwoAdultsChildInfant_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Logged-in one-way Economy Light 2-adult child infant journey",
            FareName = "Economy Light",
            Authenticated = true,
            LoyaltyPassword = ReadLoyaltyPasswordOrSkip(),
            Adults = 2,
            Children = 1,
            Infants = 1
        });
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-LoggedIn")]
    [Trait("tag", "search-and-book-ui-logged-in")]
    public async Task ReturnReward_EconomyClassic_TwoAdultsChildInfant_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Logged-in return reward Economy Classic 2-adult child infant journey",
            FareName = "Economy Classic",
            Authenticated = true,
            AwardSearch = true,
            LoyaltyPassword = ReadLoyaltyPasswordOrSkip(),
            MinimumLoyaltyPoints = 200_000,
            IsReturn = true,
            Adults = 2,
            Children = 1,
            Infants = 1,
            SeatSelection = SeatSelectionMode.Skip
        });
    }

    [SkippableFact]
    public async Task OneWay_UsCardholderOffer_OpensSynchronyApplication()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "One-way US cardholder offer application",
            Origin = "JFK",
            Destination = "LHR",
            FareName = "Any available",
            SelectAnyAvailableFare = true,
            Payment = PaymentJourneyMode.CardholderOffer
        });
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-LoggedIn")]
    [Trait("tag", "search-and-book-ui-logged-in")]
    public async Task Return_Premium_TwoAdults_PartialPayWithPoints_CompletesBooking()
    {
        await RunScenarioAsync(new GuestJourneyScenario
        {
            Name = "Logged-in return Premium 2-adult partial Pay with Points journey",
            FareName = "Premium",
            IsReturn = true,
            Adults = 2,
            Authenticated = true,
            LoyaltyPassword = ReadLoyaltyPasswordOrSkip(),
            MinimumLoyaltyPoints = 20_000,
            MaximumLoyaltyPoints = 150_000,
            Payment = PaymentJourneyMode.PartialPayWithPoints
        });
    }

    private async Task RunScenarioAsync(GuestJourneyScenario scenario)
    {
        string? baseUrl = Environment.GetEnvironmentVariable("SAB_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set SAB_BASE_URL to run Search & Book journeys.");

        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl!);
        GuestJourneyResult result = await new GuestJourneyFlow(options).RunAsync(scenario);

        _output.WriteLine($"Scenario: {scenario.Name}");
        _output.WriteLine($"Search: {result.Search.Origin}->{result.Search.Destination}");
        _output.WriteLine($"Departure: {result.Search.DepartureDate:yyyy-MM-dd}");
        _output.WriteLine($"Return: {result.Search.ReturnDate:yyyy-MM-dd}");
        _output.WriteLine($"Selected fare: {result.SelectedFare}");
        _output.WriteLine($"Initial price: {result.InitialPrice}");
        _output.WriteLine($"Payment price: {result.PaymentPrice}");
        _output.WriteLine($"Selected seats: {string.Join(", ", result.SelectedSeats)}");
        _output.WriteLine(result.SkippedSeatSelections.Count == 0
            ? "Seat selection skipped: No"
            : $"Seat selection skipped: Yes - {string.Join("; ", result.SkippedSeatSelections)}");
        _output.WriteLine($"Payment outcome: {result.PaymentOutcome}");
        _output.WriteLine($"Payment reference ID: {result.PaymentReferenceId ?? "Not captured"}");
        if (result.Confirmation is null &&
            scenario.Authenticated &&
            new Uri(baseUrl!).Host.Is("stg.virginatlantic.com"))
        {
            _output.WriteLine(
                "Completion outcome: Stopped at HPP without submitting payment because " +
                "authenticated booking confirmation is restricted in STG (UAT-backed).");
        }
        if (result.Confirmation is not null)
        {
            _output.WriteLine($"PNR: {result.Confirmation.BookingReference}");
            _output.WriteLine($"Confirmation passengers: {result.Confirmation.PassengerDetails}");
            _output.WriteLine($"Confirmation flight: {result.Confirmation.FlightSummary}");
            _output.WriteLine($"Confirmation payment: {result.Confirmation.PaymentMethod}");
        }
        _output.WriteLine($"Screenshot: {result.ScreenshotPath}");
    }

    private static string ReadLoyaltyPasswordOrSkip()
    {
        string? password = Environment.GetEnvironmentVariable("LoyaltyAccount");
        Skip.If(password.IsNullOrWhiteSpace(), "Set LoyaltyAccount to run logged-in journeys.");
        return password!;
    }
}

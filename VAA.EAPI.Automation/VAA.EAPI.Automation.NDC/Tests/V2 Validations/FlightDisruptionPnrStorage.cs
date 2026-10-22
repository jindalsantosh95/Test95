using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations;

public sealed class FlightDisruptionPnrStorage : IClassFixture<MemoryStoreFixture>
{
    public FlightDisruptionPnrStorage(MemoryStoreFixture memoryStoreFixture)
    {
        memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
    }

    public static IEnumerable<object[]> DisruptionScenarios => new[]
    {
        Scenario("VS5", LocationType.LHR, LocationType.MIA, JourneyType.Return, LocationType.MIA, LocationType.LHR, "Cancellation - no alternative", paymentType: "CPDVisa"),
        Scenario("VS7", LocationType.LHR, LocationType.LAX, JourneyType.OneWay, LocationType.None, LocationType.None, "Cancellation - reaccommodate", paymentType: "Cash"),
        Scenario("VS91", LocationType.LHR, LocationType.MCO, JourneyType.Return, LocationType.MCO, LocationType.LHR, "Flight number change", paymentType: "CPDVisa"),
        Scenario("VS148", LocationType.YYZ, LocationType.LHR, JourneyType.Return, LocationType.LHR, LocationType.YYZ, "Time change <2 hours", paymentType: "Cash"),
        Scenario("VS19", LocationType.LHR, LocationType.SFO, JourneyType.Return, LocationType.SFO, LocationType.LHR, "Time change >5 hours", paymentType: "CPDVisa"),
        Scenario("VS3", LocationType.LHR, LocationType.BOS, JourneyType.OneWay, LocationType.None, LocationType.None, "Cancellation - missed connection", "VS1989", paymentType: "Cash")
    };

    [Theory]
    [MemberData(nameof(DisruptionScenarios))]
    [Trait("Category", "NDC - FlightDisruptionPNR")]
    public void Create_disruption_booking_and_store_pnr_as_ocn(string flightNumber, LocationType origin, LocationType destination, JourneyType journeyType, LocationType returnOrigin,
        LocationType returnDestination, string disruptionScenario, string connectingFlight = null, string storageBookingType = "OCN",
        string storagePaymentType = "CPDVisa", string storageSeatType = "None")
    {
        // The schedule is disrupted on Wednesday and the flight date is exactly
        // nine months after that disruption date. The helper's one-day random
        // range makes the generated date deterministic.
        DateTime todayUtc = DateTime.UtcNow.Date;
        DateTime disruptionDate = NextWednesday(todayUtc);
        DateTime flightDate = disruptionDate.AddMonths(9);
        int addDays = (flightDate.AddDays(-1) - todayUtc).Days;

        string selectedFlightNumber = connectingFlight.IsNullOrWhiteSpace() ? flightNumber : connectingFlight;
        FlightScenarioType selectedFlightScenario = connectingFlight.IsNullOrWhiteSpace() ? FlightScenarioType.Virgin : FlightScenarioType.VirginAndNonVirgin;

        Dictionary<string, string> booking = OrderCreateHelper.CreatePnr(addDays: addDays, maximumDays: 2, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, flight: selectedFlightScenario, journeyType: journeyType,
            locationOne: origin, locationTwo: destination, locationThree: returnOrigin, locationFour: returnDestination,
            bookingType: "InstantPurchase", paymentType: storagePaymentType, requiredFlightNumber: selectedFlightNumber);

        string orderCreateResult = booking["orderCreateResult"];
        string pnr = FindAndReturnDataHelper.ReturnBookingId(orderCreateResult);

        Assert.False(pnr.IsNullOrWhiteSpace(),
            $"No PNR was returned for {flightNumber} ({origin}-{destination}), scenario '{disruptionScenario}'.");

        // OrderCreate stores the PNR automatically. Upsert it with the OCN
        // marker required by the disruption test data.
        StorageAccountHelper.SavePNRToStorageTable(pnr, Config.Environment, bookingType: storageBookingType, paymentType: storagePaymentType, seatType: storageSeatType);

        string[] storedPnrs = StorageAccountHelper.RetrievePNRFromStorageTable("NDCPNRsTable");

        Assert.Contains(pnr, storedPnrs);
    }

    private static object[] Scenario(string flightNumber, LocationType origin, LocationType destination, JourneyType journeyType,
        LocationType returnOrigin, LocationType returnDestination, string disruptionScenario, string connectingFlight = null,
        string storageBookingType = "OCN", string paymentType = "CPDVisa", string storageSeatType = "None") => new object[]
    {
        flightNumber,
        origin,
        destination,
        journeyType,
        returnOrigin,
        returnDestination,
        disruptionScenario,
        connectingFlight,
        storageBookingType,
        paymentType,
        storageSeatType
    };

    private static DateTime NextWednesday(DateTime date)
    {
        int daysUntilWednesday = ((int)DayOfWeek.Wednesday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysUntilWednesday == 0 ? 7 : daysUntilWednesday);
    }
}

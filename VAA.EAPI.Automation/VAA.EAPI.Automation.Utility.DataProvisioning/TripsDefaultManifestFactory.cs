using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Trips.DataProvider;

namespace VAA.EAPI.Automation.Utility.DataProvisioning;

internal static class TripsDefaultManifestFactory
{
    public static ProvisioningManifest Create()
    {
        List<PnrScenario> scenarios = new();
        AddCoreScenarios(scenarios, PnrDataScenarios.LoyaltyScenarios());
        AddCoreScenarios(scenarios, PnrDataScenarios.GlassTierScenarios());
        AddCoreScenarios(scenarios, PnrDataAwardScenarios.AwardScenarios());
        AddPartnerLoyaltyScenarios(scenarios);
        AddSeatSetupScenarios(scenarios);
        return new ProvisioningManifest
        {
            MaxConcurrency = 2,
            DataSets = new[]
            {
                new ProvisioningDataSet { Name = "trips", Scenarios = scenarios },
            },
        };
    }

    private static void AddCoreScenarios(List<PnrScenario> scenarios, IEnumerable<object[]> rows)
    {
        foreach (object[] row in rows)
            scenarios.Add((PnrScenario)row[0]);
    }

    private static void AddPartnerLoyaltyScenarios(List<PnrScenario> scenarios)
    {
        int rowNumber = 0;
        foreach (object[] row in LoyaltyAccountsScenarios.LoyaltyTestCases(true))
        {
            scenarios.Add(new PnrScenario(
                (LocationType)row[0],
                (LocationType)row[1],
                (JourneyType)row[2],
                (int)row[3],
                (int)row[4],
                (int)row[5],
                (int)row[6],
                (FlightType)row[7],
                (FareFamilyType)row[8],
                (CabinName)row[9],
                (bool)row[10],
                LoggedInUser: false,
                (string)row[11],
                Oci: false)
            {
                Name = $"Trips-PartnerLoyalty-{++rowNumber:D3}",
                LeadFirstName = (string)row[12],
                LeadLastName = (string)row[13],
                FrequentFlyerProgramme = (string)row[14],
            });
        }
    }

    private static void AddSeatSetupScenarios(List<PnrScenario> scenarios)
    {
        int rowNumber = 0;
        foreach (object[] row in SeatSelectionScenarios.PaidSeatsTestCases(true))
        {
            scenarios.Add(new PnrScenario(
                (LocationType)row[0],
                (LocationType)row[1],
                (JourneyType)row[2],
                (int)row[3],
                (int)row[4],
                (int)row[5],
                (int)row[6],
                (FlightType)row[7],
                (FareFamilyType)row[8],
                (CabinName)row[9],
                (bool)row[10],
                (bool)row[11],
                PaymentType: "CreditCard",
                Oci: false)
            {
                Name = $"Trips-SeatSetup-{++rowNumber:D3}",
            });
        }
    }
}

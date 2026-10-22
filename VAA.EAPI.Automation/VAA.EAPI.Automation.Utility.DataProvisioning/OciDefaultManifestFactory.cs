using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.OCI.Helpers;

namespace VAA.EAPI.Automation.Utility.DataProvisioning;

internal static class OciDefaultManifestFactory
{
    public static ProvisioningManifest Create()
    {
        List<PnrScenario> scenarios = new();
        AddRevenue(scenarios);
        AddRedemption(scenarios);
        AddSeats(scenarios);
        AddInsurance(scenarios);
        AddPartners(scenarios);
        AddMultiCity(scenarios);
        AddFilteredJourneys(scenarios);
        AddOperationalJourneys(scenarios);
        return new ProvisioningManifest
        {
            MaxConcurrency = 2,
            DataSets = new[]
            {
                new ProvisioningDataSet { Name = "oci", Scenarios = scenarios },
            },
        };
    }

    private static PnrScenario Base(
        string name,
        LocationType origin,
        LocationType destination,
        JourneyType journeyType,
        int adults,
        int youngAdults,
        int children,
        int infants,
        int departureOffset,
        FlightType airline = FlightType.VS,
        FareFamilyType fareFamily = FareFamilyType.ECON,
        CabinName cabinName = CabinName.NONE,
        string paymentType = "CreditCard")
    {
        return new PnrScenario(
            origin,
            destination,
            journeyType,
            adults,
            youngAdults,
            children,
            infants,
            airline,
            fareFamily,
            cabinName,
            Seats: false,
            LoggedInUser: false,
            paymentType,
            Oci: true)
        {
            Name = name,
            StoragePartitionKey = GetLegacyStoragePartitionKey(name),
            DepartureOffsetDays = departureOffset,
        };
    }

    private static string GetLegacyStoragePartitionKey(string scenarioName)
    {
        string[] parts = scenarioName.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 3 || !parts[0].Is("OCI"))
        {
            throw new InvalidOperationException(
                $"OCI scenario name '{scenarioName}' does not match the expected OCI-<Category>-... pattern.");
        }

        string category = parts[1];
        return category switch
        {
            "OneWayRevenue" => "OneWayScenarioRevenue",
            "OneWayReturnDate" => "OneWayReturnDateScenarioRevenue",
            "ReturnRevenue" => "ReturnScenarioRevenue",
            "OneWayRedemption" => "OneWayScenarioRedemption",
            "ReturnRedemption" => "ReturnScenarioRedemption",
            "ReturnWithSeat" => "ReturnWithSeatScenarioRevenue",
            "OneWayInsurance" => "OneWayWithInsuranceScenarioRevenue",
            "ReturnJVPartner" => "ReturnJVPartnersScenarioRevenue",
            "ReturnCodeshare" => "ReturnCodesharePartnersScenarioRevenue",
            "ReturnInterline" => "ReturnInterlinePartnersScenarioRevenue",
            "MultiCity3" => "MultiCity3SlicesScenarioRevenue",
            "MultiCity4" => "MultiCity4SlicesScenarioRevenue",
            "DirectWithStop" => "OneWayDirectWithStopScenarioRevenue",
            "TwoStops" => "OneWayWithTwoStopScenarioRevenue",
            "OneStop" => "OneWayWithOneStopScenarioRevenue",
            "Direct" => "OneWayDirectFlightScenarioRevenue",
            "LongLayover" => "OneWayLongLayoverFlightScenarioRevenue",
            "SameDayReturn" => "ReturnSameDayScenarioRevenue",
            "SkyPriority" => "OneWaySkyPriorityScenarioRevenue",
            "TSA" => "OneWayTSAScenarioRevenue",
            "Seamless" => "OneWaySeamlessScenarioRevenue",
            "NonSeamless" => "OneWayNonSeamlessScenarioRevenue",
            "ESTA" => "OneWayESTAScenarioRevenue",
            "Seasonal" => "OneWaySeasonalRouteScenarioRevenue",
            _ => throw new InvalidOperationException($"No legacy OCI storage partition key is configured for category '{category}'."),
        };
    }

    private static string Name(string category, int row, int offset) => $"OCI-{category}-{row:D3}-D{offset}";

    private static void AddRevenue(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetRevenuePNRCreationData())
        {
            scenarios.Add(Base(
                Name("OneWayRevenue", ++row, (int)data[8]),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], (int)data[8]) with
            {
                LeadFirstName = (string)data[6],
                LeadLastName = (string)data[7],
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetOCIReturnDateScenarioData())
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name("OneWayReturnDate", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset,
                paymentType: "PayPal") with
            { OfferSelection = PnrOfferSelection.Any });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetOCIReturnDateScenarioData())
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name("ReturnRevenue", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset));
        }
    }

    private static void AddRedemption(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetRedemptionPNRCreationData())
        {
            int offset = (int)data[3];
            foreach (JourneyType journeyType in new[] { JourneyType.OneWay, JourneyType.Return })
            {
                scenarios.Add(Base(Name($"{journeyType}Redemption", ++row, offset),
                    (LocationType)data[0], (LocationType)data[1], journeyType,
                    1, 1, 1, 1, offset, fareFamily: FareFamilyType.AWARDECONOMY) with
                {
                    LoggedInUser = true,
                    PreferredTier = (string)data[2],
                    MinimumLoyaltyPoints = 1_000_000,
                });
            }
        }
    }

    private static void AddSeats(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetRevenuePNRCreationDataWithSeat())
        {
            int offset = (int)data[9];
            scenarios.Add(Base(Name("ReturnWithSeat", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[3], (int)data[4], (int)data[5], (int)data[6], offset,
                fareFamily: (FareFamilyType)data[2], cabinName: (CabinName)data[8]) with
            {
                Seats = true,
                SeatProductName = (string)data[7],
            });
        }
    }

    private static void AddInsurance(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetPNRCreationData())
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name("OneWayInsurance", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset) with
            {
                IncludeInsurance = true,
            });
        }
    }

    private static void AddPartners(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetPNRCreationJVPartnersData())
        {
            int offset = (int)data[9];
            scenarios.Add(Base(Name("ReturnJVPartner", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset,
                (FlightType)data[7], (FareFamilyType)data[6]) with
            {
                InboundFareFamily = (FareFamilyType)data[8],
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetPNRCreationCodeshareData())
        {
            int offset = (int)data[10];
            scenarios.Add(Base(Name("ReturnCodeshare", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset,
                (FlightType)data[7], (FareFamilyType)data[6]) with
            {
                InboundFareFamily = (FareFamilyType)data[8],
                InboundAirline = (FlightType)data[9],
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetPNRCreationInterlineData())
        {
            int offset = (int)data[9];
            scenarios.Add(Base(Name("ReturnInterline", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset,
                (FlightType)data[7], (FareFamilyType)data[6]) with
            {
                InboundFareFamily = (FareFamilyType)data[8],
            });
        }
    }

    private static void AddMultiCity(List<PnrScenario> scenarios)
    {
        AddMultiCityRows(scenarios, OCIPnrDataCreationTestCases.GetPNRCreationMultiCity3SlicesData(), "MultiCity3");
        AddMultiCityRows(scenarios, OCIPnrDataCreationTestCases.GetPNRCreationMultiCity4SlicesData(), "MultiCity4");
    }

    private static void AddMultiCityRows(List<PnrScenario> scenarios, IEnumerable<object[]> rows, string category)
    {
        int row = 0;
        foreach (object[] data in rows)
        {
            LocationType[] locations = (LocationType[])data[0];
            int offset = (int)data[1];
            scenarios.Add(Base(Name(category, ++row, offset), locations[0], locations[1], JourneyType.MultiCity,
                1, 0, 0, 0, offset) with
            { MultiCityLocations = locations });
        }
    }

    private static void AddFilteredJourneys(List<PnrScenario> scenarios)
    {
        AddFilterRows(scenarios, OCIPnrDataCreationTestCases.GetOCIDirectWithStopScenarioData(), "DirectWithStop", PnrOfferSelection.DirectWithStop);
        AddFilterRows(scenarios, OCIPnrDataCreationTestCases.FlightsWithTwoStopScenarioData(), "TwoStops", PnrOfferSelection.TwoStops);
        AddFilterRows(scenarios, OCIPnrDataCreationTestCases.FlightsWithOneStopScenarioData(), "OneStop", PnrOfferSelection.OneStop);
        AddFilterRows(scenarios, OCIPnrDataCreationTestCases.DirectFlightsScenarioData(), "Direct", PnrOfferSelection.Direct);

        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.LongLayoverFlightsScenarioData())
        {
            int offset = (int)data[7];
            scenarios.Add(Base(Name("LongLayover", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset) with
            {
                OfferSelection = PnrOfferSelection.LongLayover,
                LongLayoverHours = (int)data[6],
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.GetPNRCreationData())
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name("SameDayReturn", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.Return,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset) with
            {
                SameDayReturn = true,
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.DirectFlightsScenarioData())
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name("SkyPriority", ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset,
                fareFamily: FareFamilyType.BUSFIRST, cabinName: CabinName.VSUP));
        }
    }

    private static void AddFilterRows(
        List<PnrScenario> scenarios,
        IEnumerable<object[]> rows,
        string category,
        PnrOfferSelection selection)
    {
        int row = 0;
        foreach (object[] data in rows)
        {
            int offset = (int)data[6];
            scenarios.Add(Base(Name(category, ++row, offset),
                (LocationType)data[0], (LocationType)data[1], JourneyType.OneWay,
                (int)data[2], (int)data[3], (int)data[4], (int)data[5], offset) with
            {
                OfferSelection = selection,
            });
        }
    }

    private static void AddOperationalJourneys(List<PnrScenario> scenarios)
    {
        int row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.TSAPNRCreationData())
        {
            int offset = (int)data[2];
            scenarios.Add(Base(Name("TSA", ++row, offset), LocationType.LHR, LocationType.JFK,
                JourneyType.OneWay, 1, 0, 0, 0, offset) with
            {
                LeadFirstName = (string)data[0],
                LeadLastName = (string)data[1],
                OfferSelection = PnrOfferSelection.Any,
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.SeamlessFlightsScenarioData())
        {
            int offset = (int)data[3];
            scenarios.Add(Base(Name("Seamless", ++row, offset), (LocationType)data[0], (LocationType)data[1],
                JourneyType.OneWay, 1, 0, 0, 0, offset, airline: (FlightType)data[2]));
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.NonSeamlessFlightsScenarioData())
        {
            int offset = (int)data[2];
            scenarios.Add(Base(Name("NonSeamless", ++row, offset), (LocationType)data[0], (LocationType)data[1],
                JourneyType.OneWay, 1, 0, 0, 0, offset, paymentType: "PayPal") with
            {
                OfferSelection = PnrOfferSelection.Any,
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.USABookingWithSpecialNamesESTAData())
        {
            int offset = (int)data[4];
            scenarios.Add(Base(Name("ESTA", ++row, offset), (LocationType)data[0], (LocationType)data[1],
                JourneyType.OneWay, 1, 0, 0, 0, offset) with
            {
                LeadFirstName = (string)data[2],
                LeadLastName = (string)data[3],
                OfferSelection = PnrOfferSelection.Any,
            });
        }

        row = 0;
        foreach (object[] data in OCIPnrDataCreationTestCases.SeasonalFlightsRoutes())
        {
            int offset = (int)data[2];
            scenarios.Add(Base(Name("Seasonal", ++row, offset), (LocationType)data[0], (LocationType)data[1],
                JourneyType.OneWay, 1, 0, 0, 0, offset) with
            {
                OfferSelection = PnrOfferSelection.Any,
                UseSeasonalSchedule = true,
            });
        }
    }
}

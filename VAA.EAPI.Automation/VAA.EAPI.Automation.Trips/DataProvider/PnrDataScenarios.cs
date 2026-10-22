using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Trips.DataProvider
{
    public class PnrDataScenarios
    {
        private static readonly Random random = new();

        // Computed once on first access (Lazy is thread-safe by default). The
        // seat-selection coin-flip uses a deterministically-seeded local Random
        // consumed inside GenerateAllTypedScenariosOnce(), so:
        //   - LoyaltyScenarios() and GlassTierScenarios() see the same rows
        //     regardless of which trait xUnit enumerates first.
        //   - Concurrent enumeration is safe (no shared mutable RNG).
        //   - Side-by-side comparison runs reproduce the same seat pattern,
        //     making per-row anomalies easier to spot.
        // Kept deliberately separate from GenerateTestCases() — the legacy
        // path still uses its time-seeded shared Random so its existing
        // nightly distribution is unchanged.
        //
        // Lazy<> (rather than a plain static initializer) so the factory runs
        // on first .Value access — by which point ValidRoutes is guaranteed
        // initialized regardless of field declaration order. Field-order
        // dependencies in static initializers are easy to break by re-ordering.
        private static readonly Lazy<IReadOnlyList<PnrScenario>> AllTypedScenarios =
            new(() => GenerateAllTypedScenariosOnce().ToList());

        public static IEnumerable<object[]> TestCases() => GenerateTestCases();

        public static IEnumerable<object[]> LoyaltyScenarios() =>
            AllTypedScenarios.Value.Where(s => s.LoggedInUser).Select(s => new object[] { s });

        public static IEnumerable<object[]> GlassTierScenarios() =>
            AllTypedScenarios.Value.Where(s => !s.LoggedInUser).Select(s => new object[] { s });
        private static IEnumerable<object[]> GenerateTestCases()
        {
            foreach (Tuple<LocationType, LocationType, FlightType> route in ValidRoutes)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;
                bool originBlocksSeats = origin is LocationType.LHR or LocationType.DEL or LocationType.JFK;
                bool destinationBlocksSeats = destination is LocationType.JFK or LocationType.LHR or LocationType.ATL or LocationType.DEL;
                bool seats = !(originBlocksSeats && destinationBlocksSeats) && random.Next(2) == 1;

                // Standard test cases with mix of logged in (FC members) and non-logged in (VAC) users
                // Parameters: [origin, destination, journeyType, adults, youngAdults, children, infants, flightType, fareFamily, cabinName, seats, loggedInUser, paymentType, oci]

                if (flightType == FlightType.DL) // Delta Airlines test cases
                {
                    // Economy class test cases
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, true, "CreditCard", false];  // Economy - FC member
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, false, "CreditCard", false]; // Economy - VAC user

                    // Economy Delight return journey with family
                    yield return [origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECONOMYDELIGHT, CabinName.DCP, seats, false, "CreditCard", false]; // Economy Delight Return - VAC user with family

                    // Premium Economy test case
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.DPPS, seats, true, "CreditCard", false]; // Premium Economy - FC member

                    // Business class test cases
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, false, "CreditCard", false]; // Business - VAC user
                    yield return [origin, destination, JourneyType.Return, 1, 1, 0, 2, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, true, "CreditCard", false];   // Business Return - FC member with family and infants
                }
                else // Virgin Atlantic (VS) test cases
                {
                    // Economy class test cases
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false]; // Economy - VAC user
                    yield return [origin, destination, JourneyType.OneWay, 1, 1, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, true, "CreditCard", false];  // Economy - FC member with young adult
                    yield return [origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];  // Economy Return - VAC user with full family

                    // Premium Economy test case
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, seats, true, "CreditCard", false]; // Premium Economy - FC member
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, seats, false, "CreditCard", false]; // Premium Economy - VAC user                      
                                                                                                                                                                                              // Upper Class test cases
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, true, "CreditCard", false];  // Upper Class - FC member
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, false, "CreditCard", false]; // Upper Class - VAC user
                }
            }
        }

        private static IEnumerable<PnrScenario> GenerateAllTypedScenariosOnce()
        {
            // Fixed seed so side-by-side runs against the legacy path produce
            // a reproducible seat-decision pattern.
            Random seatRng = new(Seed: 42);

            foreach (Tuple<LocationType, LocationType, FlightType> route in ValidRoutes)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;
                bool originBlocksSeats = origin is LocationType.LHR or LocationType.DEL or LocationType.JFK;
                bool destinationBlocksSeats = destination is LocationType.JFK or LocationType.LHR or LocationType.ATL or LocationType.DEL;
                bool seats = !(originBlocksSeats && destinationBlocksSeats) && seatRng.Next(2) == 1;

                if (flightType == FlightType.DL)
                {
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, loggedInUser: true);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECONOMYDELIGHT, CabinName.DCP, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.DPPS, seats, loggedInUser: true);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.Return, 1, 1, 0, 2, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, loggedInUser: true);
                }
                else
                {
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 1, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, loggedInUser: true);
                    yield return Build(origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECON, CabinName.NONE, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, seats, loggedInUser: true);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, seats, loggedInUser: false);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, loggedInUser: true);
                    yield return Build(origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, loggedInUser: false);
                }
            }
        }

        private static PnrScenario Build(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser)
        {
            string tier = loggedInUser ? "Loyalty" : "Glass";
            string name = $"Trips-{tier}-{airline}-{origin}{destination}-{journeyType}-{fareFamily}";

            return new PnrScenario(
                Origin: origin,
                Destination: destination,
                JourneyType: journeyType,
                Adults: adults,
                YoungAdults: youngAdults,
                Children: children,
                Infants: infants,
                Airline: airline,
                FareFamily: fareFamily,
                CabinName: cabinName,
                Seats: seats,
                LoggedInUser: loggedInUser,
                PaymentType: "CreditCard",
                Oci: false)
            { Name = name };
        }

        private static readonly Tuple<LocationType, LocationType, FlightType>[] ValidRoutes =
        [
          new (LocationType.LHR, LocationType.JFK, FlightType.DL),
          new (LocationType.JFK, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.JFK, FlightType.VS),
          new (LocationType.JFK, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.ATL, FlightType.VS),
          new (LocationType.ATL, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.ATL, FlightType.DL),
          new (LocationType.ATL, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.LAS, FlightType.DL),
          new (LocationType.LHR, LocationType.MIA, FlightType.DL),
          new (LocationType.LAS, LocationType.LHR, FlightType.DL),
          new (LocationType.MIA, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.DEL, FlightType.VS),
          new (LocationType.DEL, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.BOM, FlightType.VS),
          new (LocationType.BOM, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.DXB, FlightType.VS),
          new (LocationType.DXB, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.AMD, FlightType.VS),
          new (LocationType.MAN, LocationType.JFK, FlightType.VS),
          new (LocationType.DUB, LocationType.JFK, FlightType.VS),
          new (LocationType.LHR, LocationType.SVD, FlightType.VS),
          new (LocationType.LHR, LocationType.GND, FlightType.VS),
          new (LocationType.DEL, LocationType.MIA, FlightType.VS),
        ];
    }
}

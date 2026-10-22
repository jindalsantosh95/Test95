using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Trips.DataProvider
{
    public class PnrDataAwardScenarios
    {
        public static IEnumerable<object[]> AwardTestCases() => GenerateAwardTestCases();

        // PR-2 of the PNR-provisioner consolidation. Typed-scenario surface for
        // PnrDataSeed.AwardPnrSeed, mirroring AwardTestCases() row-for-row.
        public static IEnumerable<object[]> AwardScenarios()
        {
            foreach (Tuple<LocationType, LocationType, FlightType> route in AwardRoutes)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;

                yield return new object[] { BuildAward(origin, destination, flightType, FareFamilyType.AWARDECONOMY) };
                yield return new object[] { BuildAward(origin, destination, flightType, FareFamilyType.AWARDBUSINESSFIRST) };
            }
        }

        private static PnrScenario BuildAward(LocationType origin, LocationType destination, FlightType flightType, FareFamilyType fareFamily)
        {
            string name = $"Trips-Award-{flightType}-{origin}{destination}-OneWay-{fareFamily}";

            return new PnrScenario(
                Origin: origin,
                Destination: destination,
                JourneyType: JourneyType.OneWay,
                Adults: 1,
                YoungAdults: 0,
                Children: 0,
                Infants: 0,
                Airline: flightType,
                FareFamily: fareFamily,
                CabinName: CabinName.NONE,
                Seats: false,
                LoggedInUser: true,
                PaymentType: "CreditCard",
                Oci: false)
            { Name = name };
        }

        private static IEnumerable<object[]> GenerateAwardTestCases()
        {
            foreach (Tuple<LocationType, LocationType, FlightType> route in AwardRoutes)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;

                // Award bookings: loggedInUser=true, seats=false, FareFamilyType=AWARDECONOMY
                // Parameters: [origin, destination, journeyType, adults, youngAdults, children, infants, flightType, fareFamily, cabinName, seats, loggedInUser, paymentType, oci]
                yield return new object[] { origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.AWARDECONOMY, CabinName.NONE, false, true, "CreditCard", false };
                yield return new object[] { origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.AWARDBUSINESSFIRST, CabinName.NONE, false, true, "CreditCard", false };
            }
        }

        private static readonly Tuple<LocationType, LocationType, FlightType>[] AwardRoutes =
        [
            new (LocationType.LHR, LocationType.JFK, FlightType.VS),
            new (LocationType.JFK, LocationType.LHR, FlightType.VS),
            new (LocationType.LHR, LocationType.DEL, FlightType.VS),
            new (LocationType.LHR, LocationType.BOM, FlightType.VS),
            new (LocationType.LHR, LocationType.DXB, FlightType.VS),
        ];
    }
}

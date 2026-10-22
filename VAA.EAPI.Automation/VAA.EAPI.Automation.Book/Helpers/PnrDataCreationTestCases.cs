using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Helpers
{
    public class PnrDataCreationTestCases
    {
        private static readonly Random random = new();

        // Specific passenger combinations for loyalty test cases
        private static readonly (string FirstName, string LastName, string FQTProgram)[] LoyaltyPassengers =
        [
            ("HUNDREDSIXTYFIVE", "AITESTT", "VS-9999999494"),
            ("BASETEST", "AIRFRANCE", "AF-6000001006"),
            ("SILVERTEST", "AIRFRANCE", "AF-6000002001"),
            ("PLATINUMTEST", "AIRFRANCE", "AF-6000004005"),
            ("CLUBTWOTHOUSANDTEST", "AIRFRANCE", "AF-6000005000"),
            ("ASSEVEN", "BASEPCCUSE", "DL-8006428497"),
            ("AUTEN", "GOLDPCCUSE", "DL-8002409764"),
            ("AUFIVE", "PLATINUMPCCUSE", "DL-8004421171"),
            ("ATFOUR", "DIAMONDPCCUSE", "DL-8003419408"),
            ("BASETEST", "KLM", "KL-6000001006"),
            ("SILVERTEST", "KLM", "KL-6000002001"),
            ("PLATINUMTEST", "KLM", "KL-6000004005"),
            ("CLUBTWOTHOUSANDTEST", "KLM", "KL-6000005000")
        ];

        public static IEnumerable<object[]> TestCases() => TestCases(false);
        public static IEnumerable<object[]> TestCases(bool buisnessData)
        {
            foreach (var route in ValidRoutes)
            {
                var origin = route.Item1;
                var destination = route.Item2;
                var flightType = route.Item3;
                bool seats = (origin == LocationType.LHR && (destination == LocationType.JFK || destination == LocationType.ATL || destination == LocationType.DEL)) ? true : random.Next(2) == 1;

                if (buisnessData)
                {
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, true, "CreditCard", false];
                    yield return [origin, destination, JourneyType.Return, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, true, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 1, 1, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.Return, 1, 1, 0, 2, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", true];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, true, "CreditCard", true];
                    yield return [origin, destination, JourneyType.Return, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, true, "CreditCard", true];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, false, "CreditCard", true];
                    yield return [origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", true];
                    yield return [origin, destination, JourneyType.Return, 1, 1, 0, 2, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", true];
                }
                else
                {
                    if (flightType == FlightType.DL)
                    {
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, seats, false, "CreditCard", false];
                    }
                    else
                    {
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, true, "CreditCard", false];
                        yield return [origin, destination, JourneyType.Return, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, true, "CreditCard", false];
                        yield return [origin, destination, JourneyType.OneWay, 1, 1, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.Return, 1, 1, 1, 1, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                        yield return [origin, destination, JourneyType.Return, 1, 1, 0, 2, flightType, FareFamilyType.ECON, CabinName.NONE, seats, false, "CreditCard", false];
                    }
                }
            }
        }

        public static IEnumerable<object[]> LoyaltyTestCases(bool LoyaltyCases)
        {
            if (!LoyaltyCases)
                yield break;

            int passengerIndex = 0;

            foreach (var route in ValidRoutes)
            {
                var origin = route.Item1;
                var destination = route.Item2;
                var flightType = route.Item3;
                bool seats = (origin == LocationType.LHR && (destination == LocationType.JFK || destination == LocationType.ATL || destination == LocationType.DEL)) ? true : random.Next(2) == 1;

                // Get specific passenger details from the predefined list
                var passenger = LoyaltyPassengers[passengerIndex % LoyaltyPassengers.Length];
                var firstName = passenger.FirstName;
                var lastName = passenger.LastName;
                var fQTProgram = passenger.FQTProgram;

                yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, "CreditCard", firstName, lastName, fQTProgram];
                yield return [origin, destination, JourneyType.Return, 1, 1, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, "CreditCard", firstName, lastName, fQTProgram];

                // Move to next passenger for next route
                passengerIndex++;
            }
        }

        private static readonly Tuple<LocationType, LocationType, FlightType>[] ValidRoutes =
        [
          new (LocationType.LHR, LocationType.JFK, FlightType.DL),
          new (LocationType.LHR, LocationType.JFK, FlightType.VS),
          new (LocationType.LHR, LocationType.ATL, FlightType.VS),
          new (LocationType.LHR, LocationType.ATL, FlightType.DL),
          new (LocationType.LHR, LocationType.LAS, FlightType.DL),
          new (LocationType.LHR, LocationType.MIA, FlightType.DL),
          new (LocationType.LAS, LocationType.LHR, FlightType.DL),
          new (LocationType.MIA, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.DEL, FlightType.VS),
          new (LocationType.LHR, LocationType.AMD, FlightType.VS),
          new (LocationType.MAN, LocationType.JFK, FlightType.VS),
          new (LocationType.DUB, LocationType.JFK, FlightType.VS),
          new (LocationType.LHR, LocationType.SVD, FlightType.VS),
          new (LocationType.LHR, LocationType.GND, FlightType.VS),
          new (LocationType.DEL, LocationType.MIA, FlightType.VS),
        ];


        public static IEnumerable<object[]> PaidSeatsTestCases(bool SeatTestCases)
        {
            if (!SeatTestCases)
                yield break;

            int passengerIndex = 0;

            foreach (var route in ValidRoutesForSeats)
            {
                var origin = route.Item1;
                var destination = route.Item2;
                var flightType = route.Item3;

                if (flightType == FlightType.VS)
                {
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSCL, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSDT, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECONCLASSICFLEXIBLE, CabinName.VSFL, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECONDELIGHTFLEXIBLE, CabinName.VSFD, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, false, false, "CreditCard", false];
                }
                else if (flightType == FlightType.DL)
                {
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, false, false, "CreditCard", false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, false, false, "CreditCard", false];
                }

                // Move to next passenger for next route
                passengerIndex++;
            }
        }

        private static readonly Tuple<LocationType, LocationType, FlightType>[] ValidRoutesForSeats =
        [
          new (LocationType.LHR, LocationType.JFK, FlightType.DL),
          new (LocationType.JFK, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.JFK, FlightType.VS),
          new (LocationType.JFK, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.DEL, FlightType.VS),
          new (LocationType.DEL, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.LAS, FlightType.VS),
          new (LocationType.LAS, LocationType.LHR, FlightType.VS),
          new (LocationType.LAS, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.LAS, FlightType.DL),
          new (LocationType.LHR, LocationType.BOM, FlightType.VS),
          new (LocationType.BOM, LocationType.LHR, FlightType.VS),
        ];
    }
}
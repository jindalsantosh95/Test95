using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Trips.DataProvider
{
    public class LoyaltyAccountsScenarios
    {
        private static readonly Random random = new();

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

        public static IEnumerable<object[]> LoyaltyTestCases(bool LoyaltyCases)
        {
            if (!LoyaltyCases)
                yield break;

            int passengerIndex = 0;

            foreach (Tuple<LocationType, LocationType, FlightType> route in ValidRoutes)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;
                bool seats = origin == LocationType.LHR && (destination == LocationType.JFK || destination == LocationType.ATL || destination == LocationType.DEL) ? true : random.Next(2) == 1;

                (string FirstName, string LastName, string FQTProgram) passenger = LoyaltyPassengers[passengerIndex % LoyaltyPassengers.Length];
                string firstName = passenger.FirstName;
                string lastName = passenger.LastName;
                string fQTProgram = passenger.FQTProgram;

                yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, "CreditCard", firstName, lastName, fQTProgram];
                yield return [origin, destination, JourneyType.Return, 1, 1, 0, 0, flightType, FareFamilyType.ECON, CabinName.NONE, seats, "CreditCard", firstName, lastName, fQTProgram];

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
    }
}

using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Trips.DataProvider
{
    public class SeatSelectionScenarios
    {
        public static IEnumerable<object[]> PaidSeatsTestCases(bool SeatTestCases)
        {
            if (!SeatTestCases)
                yield break;

            int passengerIndex = 0;

            foreach (Tuple<LocationType, LocationType, FlightType> route in ValidRoutesForSeats)
            {
                LocationType origin = route.Item1;
                LocationType destination = route.Item2;
                FlightType flightType = route.Item3;

                if (flightType == FlightType.VS)
                {
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSCL, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSCL, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSCL, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSCL, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.VSDT, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECONCLASSICFLEXIBLE, CabinName.VSFL, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECONDELIGHTFLEXIBLE, CabinName.VSFD, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.COMFORTPLUSPREMIUMECON, CabinName.VSPE, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.VSUP, false, false];
                }
                else if (flightType == FlightType.DL)
                {
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.ECON, CabinName.MAIN, false, false];
                    yield return [origin, destination, JourneyType.OneWay, 1, 0, 0, 0, flightType, FareFamilyType.BUSFIRST, CabinName.D1, false, false];
                }

                passengerIndex++;
            }
        }

        private static readonly Tuple<LocationType, LocationType, FlightType>[] ValidRoutesForSeats =
        [
          new (LocationType.LHR, LocationType.JFK, FlightType.DL),
          new (LocationType.JFK, LocationType.LHR, FlightType.DL),
          new (LocationType.LHR, LocationType.JFK, FlightType.VS),
          new (LocationType.JFK, LocationType.LHR, FlightType.VS),
          new (LocationType.MIA, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.MIA, FlightType.VS),
          new (LocationType.LAS, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.LAS, FlightType.VS),
          new (LocationType.IAD, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.IAD, FlightType.VS),
          new (LocationType.TPA, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.TPA, FlightType.VS),
          new (LocationType.MCO, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.MCO, FlightType.VS),
          new (LocationType.ATL, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.ATL, FlightType.VS),
          new (LocationType.SEA, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.SEA, FlightType.VS),
          new (LocationType.SFO, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.SFO, FlightType.VS),
          new (LocationType.BOS, LocationType.LHR, FlightType.VS),
          new (LocationType.LHR, LocationType.BOS, FlightType.VS),
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

using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Search
    {
        public string DepartureDate { get; set; }
        public string ReturnDate { get; set; }
        public JourneyType JourneyType { get; set; }
        public LocationType Origin { get; set; }
        public LocationType Destination { get; set; }
        public FareFamilyType FareFamilyType { get; set; }
        public FlightType FlightType { get; set; }
        public MultiCity MultiCity { get; set; }
        public List<LocationType> Connections { get; set; }
        public Passengers Passengers { get; set; }
        public bool IsFlexiDateSearch { get; set; }
        public bool IsCalendarSearch { get; set; }
        public bool IsRewardSearch { get; set; }
        public string PromoCode { get; set; }
        public string CurrentTripIndexId { get; set; }
        public CabinName CabinName { get; set; }
        public FareFamilyType InboundFareFamilyType { get; set; }
        public FlightType InboundFlightType { get; set; }
        public string RequiredLegOperatingAirlineCode { get; set; }
        public string InboundRequiredLegOperatingAirlineCode { get; set; }

        public Search()
        {
            DepartureDate = DataGenerator.DepartureDate();
            ReturnDate = DataGenerator.ReturnDate(DepartureDate);
            JourneyType = JourneyType.OneWay;
            Origin = LocationType.LHR;
            Destination = LocationType.JFK;
            FareFamilyType = FareFamilyType.ECON;
            FlightType = FlightType.VS;
            MultiCity = new MultiCity(this);
            Connections = [LocationType.None, LocationType.None, LocationType.None];
            Passengers = new Passengers();
            IsFlexiDateSearch = false;
            IsCalendarSearch = false;
            IsRewardSearch = false;
            CurrentTripIndexId = "0";
            CabinName = CabinName.VSCL;
            InboundFareFamilyType = FareFamilyType.ECON;
            InboundFlightType = FlightType.VS;
            RequiredLegOperatingAirlineCode = string.Empty;
            InboundRequiredLegOperatingAirlineCode = string.Empty;
        }
    }
}
